using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class AccountInfoDto
    {
        public TribeEnums Tribe { get; set; } = TribeEnums.Any;
        public int Gold { get; set; } = 0;
        public int Silver { get; set; } = 0;
        public bool HasPlusAccount { get; set; } = false;
    }

    [Mapper]
    public static partial class AccountInfoMapper
    {
        public static AccountInfo ToEntity(this AccountInfoDto dto, AccountId accountId)
        {
            var entity = ToEntity(dto);
            entity.AccountId = accountId.Value;
            return entity;
        }

        public static partial void To(this AccountInfoDto dto, AccountInfo entity);

        private static partial AccountInfo ToEntity(AccountInfoDto dto);
    }
}