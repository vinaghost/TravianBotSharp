using FluentResults;

namespace MainCore.Common.Errors.Database
{
    public class AccountExist : Error
    {
        private AccountExist(string username, string server) : base($"{username} [{server}] is already added")
        {
        }

        public static AccountExist Error(string username, string server) => new(username, server);
    }
}