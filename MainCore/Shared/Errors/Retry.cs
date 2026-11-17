namespace MainCore.Shared.Errors;

public sealed class Retry : Error
{
    private Retry() : base("Bot must retry")
    {
    }

    public static Result Error => new Retry();
}