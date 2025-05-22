using MainCore.UI.Models.Input;

namespace MainCore.UI.Models.Validators
{
    public class VillageSettingInputValidator : AbstractValidator<VillageSettingInput>
    {
        public VillageSettingInputValidator()
        {
            RuleFor(x => x.Tribe.SelectedItem.Tribe)
                .NotEqual(TribeEnums.Any)
                .WithMessage("Tribe should be specific");

            RuleFor(x => x.TrainTroopRepeatTime.Min)
                .LessThanOrEqualTo(x => x.TrainTroopRepeatTime.Max)
                .WithMessage("Minimum next train troop ({PropertyValue}) should be less than maximum next train troop ({ComparisonValue})");
            RuleFor(x => x.TrainTroopRepeatTime.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum next train troop ({PropertyValue}) should be positive number");

            RuleFor(x => x.BarrackAmount.Min)
                .LessThanOrEqualTo(x => x.BarrackAmount.Max)
                .WithMessage("Minimum barrack amount troop ({PropertyValue}) should be less than maximum barrack amount troop ({ComparisonValue})");
            RuleFor(x => x.BarrackAmount.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum barrack amount troop ({PropertyValue}) should be positive number");

            RuleFor(x => x.StableAmount.Min)
                .LessThanOrEqualTo(x => x.StableAmount.Max)
                .WithMessage("Minimum stable amount troop ({PropertyValue}) should be less than maximum stable amount troop ({ComparisonValue})");
            RuleFor(x => x.StableAmount.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum stable amount troop ({PropertyValue}) should be positive number");

            RuleFor(x => x.GreatBarrackAmount.Min)
                .LessThanOrEqualTo(x => x.GreatBarrackAmount.Max)
                .WithMessage("Minimum greatbarrack amount troop ({PropertyValue}) should be less than maximum greatbarrack amount troop ({ComparisonValue})");
            RuleFor(x => x.GreatBarrackAmount.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum greatbarrack amount troop ({PropertyValue}) should be positive number");

            RuleFor(x => x.GreatStableAmount.Min)
                .LessThanOrEqualTo(x => x.GreatStableAmount.Max)
                .WithMessage("Minimum greatstable amount troop ({PropertyValue}) should be less than maximum greatstable amount troop ({ComparisonValue})");
            RuleFor(x => x.GreatStableAmount.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum greatstable amount troop ({PropertyValue}) should be positive number");

            RuleFor(x => x.WorkshopAmount.Min)
                .LessThanOrEqualTo(x => x.WorkshopAmount.Max)
                .WithMessage("Minimum workshop amount troop ({PropertyValue}) should be less than maximum workshop amount troop ({ComparisonValue})");
            RuleFor(x => x.WorkshopAmount.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum workshop amount troop ({PropertyValue}) should be positive number");
        }
    }
}