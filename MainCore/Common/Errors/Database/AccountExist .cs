using FluentResults;

namespace MainCore.Common.Errors.Database
{
    public class AccountExist : Error
    {
        public AccountExist(string username, string server) : base($"{username} [{server}] is already added")
        {
        }
    }
}