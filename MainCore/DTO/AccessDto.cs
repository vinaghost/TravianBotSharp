using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class AccessDto
    {
        public AccessId Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string ProxyHost { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        public string Useragent { get; set; }
        public DateTime LastUsed { get; set; }

        public string Proxy => string.IsNullOrEmpty(ProxyHost) ? "[default]" : ProxyHost;
    }

    [Mapper]
    public static partial class AccessStaticMapper
    {
        public static Access ToEntity(this AccessDto dto, AccountId accountId)
        {
            var entity = dto.ToEntity();
            entity.AccountId = accountId.Value;
            return entity;
        }

        private static partial Access ToEntity(this AccessDto dto);

        public static partial AccessDto ToDto(this Access entity);

        public static partial IQueryable<AccessDto> ToDto(this IQueryable<Access> entities);

        private static int ToInt(this AccessId accessId) => accessId.Value;

        private static AccessId ToAccessId(this int value) => new(value);
    }
}