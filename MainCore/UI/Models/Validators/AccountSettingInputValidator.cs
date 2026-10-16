using FluentValidation;

using MainCore.UI.Models.Input;

namespace MainCore.UI.Models.Validators
{
    public class AccountSettingInputValidator : AbstractValidator<AccountSettingInput>
    {
        public AccountSettingInputValidator()
        {
            RuleFor(x => x.ClickDelay.Min)
                .LessThanOrEqualTo(x => x.ClickDelay.Max)
                .WithMessage("Minimum click delay ({PropertyValue}) should be less than maximum click delay ({ComparisonValue})");
            RuleFor(x => x.ClickDelay.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum click delay ({PropertyValue}) should be positive number");
            RuleFor(x => x.TaskDelay.Min)
                .LessThanOrEqualTo(x => x.TaskDelay.Max)
                .WithMessage("Minimum task delay ({PropertyValue}) should be less than maximum task delay ({ComparisonValue})");
            RuleFor(x => x.TaskDelay.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum task delay ({PropertyValue}) should be positive number");

            RuleFor(x => x.FarmInterval.Min)
                .LessThanOrEqualTo(x => x.FarmInterval.Max)
                .WithMessage("Minimum farm interval ({PropertyValue}) should be less than maximum farm interval ({ComparisonValue})");
            RuleFor(x => x.FarmInterval.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum farm interval ({PropertyValue}) should be positive number");

            RuleFor(x => x.InactiveFinderRadius.Value)
                .GreaterThan(0)
                .WithMessage("Inactive finder radius ({PropertyValue}) should be greater than 0");
            RuleFor(x => x.InactiveFinderPopulation.Min)
                .LessThanOrEqualTo(x => x.InactiveFinderPopulation.Max)
                .WithMessage("Minimum inactive population ({PropertyValue}) should be less than maximum inactive population ({ComparisonValue})");
            RuleFor(x => x.InactiveFinderPopulation.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum inactive population ({PropertyValue}) should be positive number");
            RuleFor(x => x.InactiveFinderTroopSlot.Value)
                .InclusiveBetween(1, 10)
                .WithMessage("Farm list troop ({PropertyValue}) should be between 1 and 10");
            RuleFor(x => x.InactiveFinderTroopAmount.Value)
                .GreaterThan(0)
                .WithMessage("Farm list troop amount ({PropertyValue}) should be greater than 0");
        }
    }
}