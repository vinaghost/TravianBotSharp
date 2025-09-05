namespace MainCore.DTO
{
    public class AccountInfoDto
    {
        public TribeEnums Tribe { get; set; } = TribeEnums.Any;
        public int Gold { get; set; } = 0;
        public int Silver { get; set; } = 0;
        public bool HasPlusAccount { get; set; } = false;
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class AccountInfoMapper
    {
        public static AccountInfo ToEntity(this AccountInfoDto dto, AccountId accountId)
        {
            var entity = ToEntity(dto);
            entity.AccountId = accountId.Value;
            return entity;
        }

        [MapperIgnoreTarget(nameof(AccountInfo.Id))]
        [MapperIgnoreTarget(nameof(AccountInfo.AccountId))]
        public static partial void To(this AccountInfoDto dto, AccountInfo entity);

        [MapperIgnoreTarget(nameof(AccountInfo.Id))]
        [MapperIgnoreTarget(nameof(AccountInfo.AccountId))]
        private static partial AccountInfo ToEntity(AccountInfoDto dto);
    }
}
