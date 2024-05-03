using MainCore.DTO;
using ReactiveUI;
using Riok.Mapperly.Abstractions;

namespace MainCore.UI.Models.Input
{
    public class AccessInput : ReactiveObject
    {
        public void Clear()
        {
            Password = "";
            ProxyHost = "";
            ProxyPort = 0;
            ProxyUsername = "";
            ProxyPassword = "";
            Useragent = "";
            LastUsed = DateTime.MinValue;
        }

        public void CopyTo(AccessInput target)
        {
            target.Password = Password;
            target.ProxyHost = ProxyHost;
            target.ProxyPort = ProxyPort;
            target.ProxyUsername = ProxyUsername;
            target.ProxyPassword = ProxyPassword;
            target.Useragent = Useragent;
            target.LastUsed = LastUsed;
        }

        public AccessInput Clone()
        {
            return new AccessInput()
            {
                Password = Password,
                ProxyHost = ProxyHost,
                ProxyPort = ProxyPort,
                ProxyUsername = ProxyUsername,
                ProxyPassword = ProxyPassword,
                Useragent = Useragent,
                LastUsed = LastUsed,
            };
        }

        public AccessId Id { get; set; }
        private string _password;
        private string _proxyHost;
        private int _proxyPort;
        private string _proxyUsername;
        private string _proxyPassword;
        private string _useragent;
        private DateTime _lastUsed;

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public string ProxyHost
        {
            get => _proxyHost;
            set => this.RaiseAndSetIfChanged(ref _proxyHost, value);
        }

        public int ProxyPort
        {
            get => _proxyPort;
            set => this.RaiseAndSetIfChanged(ref _proxyPort, value);
        }

        public string ProxyUsername
        {
            get => _proxyUsername;
            set => this.RaiseAndSetIfChanged(ref _proxyUsername, value);
        }

        public string ProxyPassword
        {
            get => _proxyPassword;
            set => this.RaiseAndSetIfChanged(ref _proxyPassword, value);
        }

        public string Useragent
        {
            get => _useragent;
            set => this.RaiseAndSetIfChanged(ref _useragent, value);
        }

        public DateTime LastUsed
        {
            get => _lastUsed;
            set => this.RaiseAndSetIfChanged(ref _lastUsed, value);
        }
    }

    [Mapper]
    public static partial class AccessInputMapper
    {
        public static partial AccessDto ToDto(this AccessInput input);

        public static partial AccessInput ToInput(this AccessDto dto);
    }
}