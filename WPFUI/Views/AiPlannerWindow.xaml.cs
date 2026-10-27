using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MainCore.Entities;
using MainCore.Services.AiPlanning;

namespace WPFUI.Views
{
    public partial class AiPlannerWindow : Window
    {
        private readonly AiPlannerService _service;
        private readonly AccountId _accountId;
        private readonly VillageId _villageId;
        private PlannerPreview _preview;
        private string _appliedId;
        private CancellationTokenSource _cancellation;
        private bool _initializing = true;
        private bool _busy;

        public AiPlannerWindow(AiPlannerService service, AccountId accountId, VillageId villageId)
        {
            _service = service;
            _accountId = accountId;
            _villageId = villageId;
            InitializeComponent();
            var options = service.Settings.Load();
            ProviderBox.SelectedIndex = (int)options.Provider;
            ModelBox.Text = options.Model;
            FreeOnlyBox.IsChecked = options.FreeOnly;
            FreeOnlyBox.IsEnabled = options.Provider == AiProvider.OpenRouter;
            GoalBox.SelectedIndex = (int)options.Goal;
            ModeBox.SelectedIndex = (int)options.ApplyMode;
            StepsBox.Text = options.MaxSteps.ToString();
            HoursBox.Text = options.HorizonHours.ToString();
            SpeedBox.Text = options.ServerSpeed.ToString();
            CapitalBox.SelectedIndex = 0; // Never carry village-specific assumptions to another village.
            SituationBox.Text = "";
            LoadSavedKey(options.Provider);
            _initializing = false;
        }

        private PlannerOptions ReadOptions()
        {
            if (!int.TryParse(StepsBox.Text, out var steps) || !int.TryParse(HoursBox.Text, out var hours) || !int.TryParse(SpeedBox.Text, out var speed))
                throw new InvalidOperationException("Enter whole numbers for upgrades, hours and speed.");
            var options = new PlannerOptions
            {
                Provider = (AiProvider)ProviderBox.SelectedIndex, Model = ModelBox.Text.Trim(), FreeOnly = FreeOnlyBox.IsChecked == true,
                Goal = (PlannerGoal)GoalBox.SelectedIndex, ApplyMode = (QueueApplyMode)ModeBox.SelectedIndex,
                MaxSteps = steps, HorizonHours = hours, ServerSpeed = speed,
                IsCapital = CapitalBox.SelectedIndex == 0 ? null : CapitalBox.SelectedIndex == 1, Situation = SituationBox.Text.Trim()
            };
            options.Validate();
            return options;
        }

        private void ProviderChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_initializing) return;
            var provider = (AiProvider)ProviderBox.SelectedIndex;
            FreeOnlyBox.IsEnabled = provider == AiProvider.OpenRouter;
            ModelBox.ItemsSource = null;
            ModelBox.Text = provider == AiProvider.OpenRouter ? "openrouter/free" : "gpt-4.1-mini";
            LoadSavedKey(provider);
            InvalidatePreview();
        }

        private void LoadSavedKey(AiProvider provider)
        {
            KeyBox.Clear();
            RememberKey.IsChecked = false;
            try { KeyBox.Password = _service.Settings.ReadKey(provider); RememberKey.IsChecked = KeyBox.Password.Length > 0; }
            catch (InvalidOperationException ex) { StatusText.Text = ex.Message; }
        }

        private void ForgetKey(object sender, RoutedEventArgs e)
        {
            try { _service.Settings.SaveKey((AiProvider)ProviderBox.SelectedIndex, ""); KeyBox.Clear(); RememberKey.IsChecked = false; StatusText.Text = "Saved key removed."; }
            catch (InvalidOperationException ex) { StatusText.Text = ex.Message; }
        }

        private void OptionChanged(object sender, RoutedEventArgs e)
        {
            if (!_initializing && !_busy) InvalidatePreview();
        }

        private void InvalidatePreview()
        {
            _preview = null;
            ApplyButton.IsEnabled = false;
            StepsGrid.ItemsSource = null;
            SummaryText.Text = "Settings changed. Analyze to create a new preview.";
            StatusText.Text = "Ready to analyze with the current settings.";
            AnalysisText.Clear(); ValidationText.Clear(); SourcesText.Clear();
        }

        private async void LoadModels(object sender, RoutedEventArgs e)
        {
            await RunAsync(async token =>
            {
                var options = ReadOptions();
                StatusText.Text = "Checking the key and loading available models…";
                var models = await _service.GetModelsAsync(options, KeyBox.Password, token);
                ModelBox.ItemsSource = models;
                ModelBox.Text = models.Any(x => x.Id == options.Model) ? options.Model : models.FirstOrDefault()?.Id ?? "";
                SaveSettings(options);
                InvalidatePreview();
                StatusText.Text = $"Key accepted. {models.Length} models available. Select a model, then analyze.";
            });
        }

        private void SaveSettings(PlannerOptions options)
        {
            _service.Settings.Save(options with { Situation = "", IsCapital = null });
            _service.Settings.SaveKey(options.Provider, RememberKey.IsChecked == true ? KeyBox.Password : "");
        }

        private async void AnalyzeVillage(object sender, RoutedEventArgs e)
        {
            InvalidatePreview();
            await RunAsync(async token =>
            {
                var options = ReadOptions();
                SaveSettings(options);
                using var progress = new PlannerProgress(message => StatusText.Text = message);
                _preview = await _service.AnalyzeAsync(_accountId, _villageId, options, KeyBox.Password,
                    progress, token);
                progress.Dispose();
                DisplayPreview(_preview);
            });
        }

        internal void DisplayPreview(PlannerPreview preview)
        {
                _preview = preview;
                var options = preview.Options;
                StepsGrid.ItemsSource = _preview.Validation.Steps;
                var costs = _preview.Validation.TotalCost;
                SummaryText.Text = $"{_preview.Validation.Steps.Length} upgrades · {options.ApplyMode} · estimated total cost: {string.Join(" / ", costs.Select(x => x.ToString("N0")))} (wood / clay / iron / crop)";
                var proposal = _preview.Response.Proposal;
                AnalysisText.Text = proposal.Assessment + "\n\nOther required actions\n" + string.Join("\n", proposal.FollowUpActions ?? [])
                    + "\n\nAssumptions\n" + string.Join("\n", proposal.Assumptions ?? [])
                    + $"\n\nModel: {_preview.Response.Model}\nTokens: {_preview.Response.InputTokens} input / {_preview.Response.OutputTokens} output";
                ValidationText.Text = (_preview.Validation.IsValid ? "Building checks passed.\n\n" : "Apply blocked:\n" + string.Join("\n", _preview.Validation.Errors) + "\n\n")
                    + string.Join("\n\n", _preview.Validation.Warnings);
                SourcesText.Text = string.Join("\n\n", _preview.Sources.Select(x => $"{x.Title}\n{x.Url}\n{x.Status}"));
                StatusText.Text = !_preview.Validation.IsValid ? "The proposal failed validation. Nothing was applied. Adjust context or try another model."
                    : _preview.Validation.CanApply ? "Preview ready. Review the analysis and validation notes, then apply."
                    : "No building upgrades proposed. Review the remaining actions and missing information.";
                ApplyButton.IsEnabled = !_busy && _preview.Validation.CanApply;
        }

        private async void ApplyQueue(object sender, RoutedEventArgs e)
        {
            var preview = _preview;
            if (preview is null || !preview.Validation.CanApply) return;
            if (preview.Options.ApplyMode == QueueApplyMode.ReplacePending && MessageBox.Show(this,
                $"Replace {preview.Snapshot.PendingJobs.Length} pending jobs with these {preview.Validation.Steps.Length} upgrades? Active construction is preserved.",
                "Apply preview", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            await RunAsync(async token =>
            {
                using var progress = new PlannerProgress(message => StatusText.Text = message);
                var count = await _service.ApplyAsync(preview, token, progress);
                progress.Dispose();
                _appliedId = preview.PlanId;
                _preview = null;
                StatusText.Text = $"Applied {count} upgrades. Resume the account when ready. Undo is available until pending jobs change.";
            });
        }

        private async void UndoApply(object sender, RoutedEventArgs e)
        {
            await RunAsync(_ =>
            {
                _service.Undo(_appliedId, _accountId, _villageId);
                _appliedId = null;
                InvalidatePreview();
                StatusText.Text = "Restored the pending queue from before this plan.";
                return Task.CompletedTask;
            });
        }

        private async Task RunAsync(Func<CancellationToken, Task> action)
        {
            if (_busy) return;
            _busy = true;
            _cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            OptionsPanel.IsEnabled = AnalyzeButton.IsEnabled = ApplyButton.IsEnabled = UndoButton.IsEnabled = false;
            CancelButton.IsEnabled = true;
            try { await action(_cancellation.Token); }
            catch (OperationCanceledException) { StatusText.Text = "Request cancelled or timed out. No new queue was applied."; }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or System.IO.IOException or UnauthorizedAccessException)
            { StatusText.Text = ex.Message; }
            catch (HttpRequestException) { StatusText.Text = "Unable to reach the provider. Check your connection and try again."; }
            catch (Exception) { StatusText.Text = "The planner could not complete this operation. Check the village browser and try again."; }
            finally
            {
                _cancellation.Dispose(); _cancellation = null; _busy = false;
                OptionsPanel.IsEnabled = AnalyzeButton.IsEnabled = true;
                ApplyButton.IsEnabled = _preview?.Validation.CanApply == true;
                UndoButton.IsEnabled = _appliedId is not null;
                CancelButton.IsEnabled = false;
            }
        }

        private void CancelRequest(object sender, RoutedEventArgs e) => _cancellation?.Cancel();
        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_busy) { _cancellation?.Cancel(); e.Cancel = true; StatusText.Text = "Cancelling the request; close this window after it stops."; }
        }
    }
}
