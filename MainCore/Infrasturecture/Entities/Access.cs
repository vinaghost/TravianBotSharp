using System.ComponentModel.DataAnnotations.Schema;

using StronglyTypedIds;

#nullable disable

namespace MainCore.Infrasturecture.Entities
{
    public class Access
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        public string Useragent { get; set; }
        public DateTime LastUsed { get; set; }

        public int AccountId { get; set; }
    }

    [StronglyTypedId]
    public partial struct AccessId
    { }

    public class AccessDto
    {
        public AccessId Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }

        public required string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public required string ProxyUsername { get; set; }
        public required string ProxyPassword { get; set; }
        public string UserAgent { get; set; } = "";
        public DateTime LastUsed { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(ProxyHost) ? "[default]" : ProxyHost;
        }
    }
}