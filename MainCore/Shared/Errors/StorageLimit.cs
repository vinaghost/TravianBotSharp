namespace MainCore.Shared.Errors;

public sealed class StorageLimit : Error
{
    private StorageLimit()
    {
    }

    public static Result Error => new StorageLimit();
}