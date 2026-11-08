namespace MainCore.Features.Shared.Errors;

public sealed class Stop : Error
{
    private Stop() : base("Bot must stop")
    {
    }

    public static Result Error => new Stop();
    public static Result DriverNotReady => Error.WithError("Driver is not ready.");
}