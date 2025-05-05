﻿namespace MainCore.DTO
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

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class AccessMapper
    {
        public static Access ToEntity(this AccessDto dto, AccountId accountId)
        {
            var entity = dto.ToEntity();
            entity.AccountId = accountId.Value;
            return entity;
        }

        [MapperIgnoreTarget(nameof(Access.AccountId))]
        public static partial Access ToEntity(this AccessDto dto);

        [MapperIgnoreTarget(nameof(AccessDto.Proxy))]
        public static partial AccessDto ToDto(this Access entity);

        public static partial IQueryable<AccessDto> ToDto(this IQueryable<Access> entities);

        private static int ToInt(this AccessId accessId) => accessId.Value;

        private static AccessId ToAccessId(this int value) => new(value);
    }
}