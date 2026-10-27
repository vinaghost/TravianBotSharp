using MainCore.Services.AiPlanning;
using MainCore.Services;
using MainCore.Entities;
using MainCore.Enums;
using Xunit;

namespace MainCore.Test.AiPlanning;

public sealed class PlannerProgressTests
{
    [Theory]
    [InlineData(StatusEnums.Online, false)]
    [InlineData(StatusEnums.Pausing, true)]
    [InlineData(StatusEnums.Paused, true)]
    public async Task Running_account_is_rejected_before_reporting_refresh(StatusEnums status, bool executing)
    {
        var tasks = new TaskManager(null!, null!, null!);
        var accountId = new AccountId(1);
        var queue = tasks.GetTaskQueue(accountId);
        queue.Status = status;
        queue.IsExecuting = executing;
        var planner = new AiPlannerService(null!, null!, tasks, null!);
        var progress = new RecordingProgress();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => planner.AnalyzeAsync(
            accountId, new VillageId(1), new PlannerOptions(), "test-key", progress, CancellationToken.None));

        Assert.Contains("Pause this account", error.Message);
        Assert.Empty(progress.Messages);
    }

    [Theory]
    [InlineData("Pause this account before using the planner.")]
    [InlineData("Request cancelled or timed out.")]
    [InlineData("Preview ready.")]
    public void Queued_progress_cannot_overwrite_the_final_status(string finalStatus)
    {
        var context = new QueuedContext();
        var previous = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(context);
            var status = "Starting";
            using var progress = new PlannerProgress(message => status = message);
            progress.Report("Refreshing resource fields…");
            // An operation can finish or throw before the dispatcher delivers progress.
            progress.Dispose();
            status = finalStatus;
            context.Drain();
            Assert.Equal(finalStatus, status);
        }
        finally { SynchronizationContext.SetSynchronizationContext(previous); }
    }

    [Fact]
    public void Old_progress_cannot_overwrite_a_new_operation_but_current_progress_is_delivered()
    {
        var context = new QueuedContext();
        var previous = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(context);
            var status = "Starting";
            using var oldProgress = new PlannerProgress(message => status = message);
            oldProgress.Report("Previous operation");
            oldProgress.Dispose();
            using var newProgress = new PlannerProgress(message => status = message);
            status = "New operation";
            context.Drain();
            Assert.Equal("New operation", status);
            newProgress.Report("Reading village buildings");
            context.Drain();
            Assert.Equal("Reading village buildings", status);
        }
        finally { SynchronizationContext.SetSynchronizationContext(previous); }
    }

    private sealed class QueuedContext : SynchronizationContext
    {
        private readonly Queue<Action> _callbacks = new();
        public override void Post(SendOrPostCallback callback, object? state) => _callbacks.Enqueue(() => callback(state));
        public void Drain()
        {
            while (_callbacks.TryDequeue(out var callback)) callback();
        }
    }

    private sealed class RecordingProgress : IProgress<string>
    {
        public List<string> Messages { get; } = [];
        public void Report(string value) => Messages.Add(value);
    }
}
