using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class AccountInfoDto
    {
        public TribeEnums Tribe { get; set; }
        public int Gold { get; set; }
        public int Silver { get; set; }
        public bool HasPlusAccount { get; set; }
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