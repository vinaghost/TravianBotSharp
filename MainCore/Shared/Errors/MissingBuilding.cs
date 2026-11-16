namespace MainCore.Shared.Errors;

public sealed class MissingBuilding : Error
{
    private MissingBuilding()
    {
    }

    public static Result Error => new MissingBuilding();
}