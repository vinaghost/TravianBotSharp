namespace MainCore.Features.Shared.Errors;

public sealed class Skip : Error
{
    private Skip() : base("Skip this task")
    {
    }

    public static Result Error => new Skip();
}