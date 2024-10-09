using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class AccountDetailDto
    {
        public string Username { get; set; }
        public string Server { get; set; }
        public string Password { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }

        public static AccountDetailDto Create(string username, string server, string password)
        {
            return new AccountDetailDto()
            {
                Username = username,
                Server = server,
                Password = password,
            };
        }

        public static AccountDetailDto Create(string username, string server, string password, string proxyHost, int proxyPort)
        {
            var dto = Create(username, server, password);
            dto.ProxyHost = proxyHost;
            dto.ProxyPort = proxyPort;
            return dto;
        }

        public static AccountDetailDto Create(string username, string server, string password, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
        {
            var dto = Create(username, server, password, proxyHost, proxyPort);
            dto.ProxyUsername = proxyUsername;
            dto.ProxyPassword = proxyPassword;
            return dto;
        }
    }

    [Mapper]
    public static partial class AccountDetailMapper
    {
        public static AccountDto ToDto(this AccountDetailDto dto)
        {
            var account = dto.ToAccount();
            var access = dto.ToAccess();
            account.Accesses = [access];
            return account;
        }

        private static partial AccountDto ToAccount(this AccountDetailDto dto);

        private static partial AccessDto ToAccess(this AccountDetailDto dto);
    }
}