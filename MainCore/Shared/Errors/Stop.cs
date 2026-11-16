namespace MainCore.Shared.Errors;

public sealed class Stop : Error
{
    private Stop() : base("Bot must stop")
    {
    }

    public static Result Error => new Stop();
}