#nullable disable

namespace MainCore.UI.Models.Input
{
    public class AccountsInput
    {
        public string Username { get; set; }
        public string Server { get; set; }
        public string Password { get; set; }
        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }

        public Account ToEntity()
        {
            return new Account()
            {
                Username = Username.Sanitize(),
                Server = Server,
                Accesses = new List<Access>()
                {
                    new()
                    {
                        Password = Password,
                        ProxyHost = ProxyHost,
                        ProxyPort = ProxyPort,
                        ProxyUsername = ProxyUsername,
                        ProxyPassword = ProxyPassword
                    }
                },
            };
        }
    }
}
