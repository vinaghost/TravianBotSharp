using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class AccountDto
    {
        public AccountId Id { get; set; }
        public string Username { get; set; }
        public string Server { get; set; }
        public List<AccessDto> Accesses { get; set; }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class AccountMapper
    {
        [MapperIgnoreTarget(nameof(Account.Info))]
        [MapperIgnoreTarget(nameof(Account.Settings))]
        [MapperIgnoreTarget(nameof(Account.Villages))]
        [MapperIgnoreTarget(nameof(Account.HeroItems))]
        [MapperIgnoreTarget(nameof(Account.FarmLists))]
        public static partial Account ToEntity(this AccountDto dto);

        public static partial AccountDto ToDto(this Account entity);

        public static partial IQueryable<AccountDto> ToDto(this IQueryable<Account> entities);

        private static int ToInt(this AccessId accessId) => accessId.Value;

        private static int ToInt(this AccountId accountId) => accountId.Value;

        private static AccountId ToAccountId(this int id) => new AccountId(id);

        private static AccessId ToAccessId(this int value) => new(value);

        private static Access ToAccessEntity(this AccessDto dto) => dto.ToEntity();

        private static AccessDto ToAccessDto(this Access entity) => entity.ToDto();
    }
}