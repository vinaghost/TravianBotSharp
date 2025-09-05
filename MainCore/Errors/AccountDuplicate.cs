namespace MainCore.Errors
{
    public class AccountDuplicate : Error
    {
        private AccountDuplicate() : base("Account is duplicated")
        {
        }

        public static AccountDuplicate Error => new();
    }
}
