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

        public Account GetAccount()
        {
            return new Account()
            {
                Username = Username,
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