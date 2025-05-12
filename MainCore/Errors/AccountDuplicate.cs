namespace MainCore.Errors
{
    public class AccountDuplicate() : Error("Account is duplicated")
    {
        public static AccountDuplicate Error => new();
    }
}