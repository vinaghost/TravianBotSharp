namespace MainCore.Shared.Errors;

public class MissingResource : Error
{
    private MissingResource()
    {
    }

    public static Result Error => new MissingResource();
}