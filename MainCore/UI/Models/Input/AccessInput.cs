namespace MainCore.UI.Models.Input
{
    public partial class AccessInput : ReactiveObject
    {
        public void Clear()
        {
            Id = AccessId.Empty;
            Username = "";
            Password = "";
            ProxyHost = "";
            ProxyPort = 0;
            ProxyUsername = "";
            ProxyPassword = "";
            Useragent = "";
            LastUsed = DateTime.MinValue;
            Cookies = "";
        }

        public void CopyTo(AccessInput target)
        {
            target.Id = Id;
            target.Username = Username;
            target.Password = Password;
            target.ProxyHost = ProxyHost;
            target.ProxyPort = ProxyPort;
            target.ProxyUsername = ProxyUsername;
            target.ProxyPassword = ProxyPassword;
            target.Useragent = Useragent;
            target.LastUsed = LastUsed;
            target.Cookies = Cookies;
        }

        public AccessInput Clone()
        {
            return new AccessInput()
            {
                Id = Id,
                Username = Username,
                Password = Password,
                ProxyHost = ProxyHost,
                ProxyPort = ProxyPort,
                ProxyUsername = ProxyUsername,
                ProxyPassword = ProxyPassword,
                Useragent = Useragent,
                LastUsed = LastUsed,
                Cookies = Cookies,
            };
        }

        public AccessId Id { get; set; }

        [Reactive]
        private string _username = "";

        [Reactive]
        private string _password = "";

        [Reactive]
        private string _proxyHost = "";

        [Reactive]
        private int _proxyPort;

        [Reactive]
        private string _proxyUsername = "";

        [Reactive]
        private string _proxyPassword = "";

        [Reactive]
        private string _useragent = "";

        [Reactive]
        private DateTime _lastUsed;

        [Reactive]
        private string _cookies = "";
    }

    public static class AccessInputExtensions
    {
        public static AccessInput ToInput(this AccessDto dto)
        {
            return new AccessInput()
            {
                Id = dto.Id,
                Username = dto.Username,
                Password = dto.Password,
                ProxyHost = dto.ProxyHost,
                ProxyPort = dto.ProxyPort,
                ProxyUsername = dto.ProxyUsername,
                ProxyPassword = dto.ProxyPassword,
                Useragent = dto.Useragent,
                LastUsed = dto.LastUsed,
                Cookies = dto.Cookies,
            };
        }

        public static AccessDto ToDto(this AccessInput input)
        {
            return new AccessDto()
            {
                Id = input.Id,
                Username = input.Username,
                Password = input.Password,
                ProxyHost = input.ProxyHost,
                ProxyPort = input.ProxyPort,
                ProxyUsername = input.ProxyUsername,
                ProxyPassword = input.ProxyPassword,
                Useragent = input.Useragent,
                LastUsed = input.LastUsed,
                Cookies = input.Cookies,
            };
        }
    }
}