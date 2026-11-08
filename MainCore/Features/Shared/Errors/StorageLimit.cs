namespace MainCore.Features.Shared.Errors;

public sealed class StorageLimit : Error
{
    private StorageLimit(string type, long storage, long required) : base($"{type} doesn't have enough capacity, need {required} but have {storage} ({required - storage})")
    {
    }

    public static StorageLimit Warehouse(long storage, long required) => new("Warehouse", storage, required);
}