namespace MainCore.DTO
{
    public class AccountSettingDto
    {
        public int Id { get; set; }
        public AccountSettingEnums Setting { get; set; }
        public int Value { get; set; }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class AccountSettingMapper
    {
        public static AccountSetting ToEntity(this AccountSettingDto dto, AccountId accountId)
        {
            var entity = dto.ToEntity();
            entity.AccountId = accountId.Value;
            return entity;
        }

        public static partial AccountSettingDto ToDto(this AccountSetting dto);

        [MapperIgnoreTarget(nameof(AccountSetting.AccountId))]
        private static partial AccountSetting ToEntity(this AccountSettingDto dto);
    }
}