using MainCore.Common.Enums;
using MainCore.Entities;
using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class AccountSettingDto
    {
        public int Id { get; set; }
        public AccountSettingEnums Setting { get; set; }
        public int Value { get; set; }
    }

    [Mapper]
    public static partial class AccountSettingMapper
    {
        public static AccountSetting ToEntity(this AccountSettingDto dto, AccountId accountId)
        {
            var entity = dto.ToEntity();
            entity.AccountId = accountId.Value;
            return entity;
        }

        public static partial AccountSettingDto ToDto(this AccountSetting dto);

        private static partial AccountSetting ToEntity(this AccountSettingDto dto);
    }
}