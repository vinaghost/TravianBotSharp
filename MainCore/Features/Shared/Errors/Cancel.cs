namespace MainCore.Features.Shared.Errors;

public sealed class Cancel : Error
{
    private Cancel() : base("Pause button is pressed")
    {
    }

    public static Result Error => new Cancel();
}