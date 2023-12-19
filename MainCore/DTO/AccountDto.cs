using MainCore.Entities;
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

    [Mapper]
    public static partial class AccountMapper
    {
        public static partial Account ToEntity(this AccountDto dto);

        public static partial AccountDto ToDto(this Account entity);

        private static int ToInt(this AccountId accountId) => accountId.Value;

        private static AccountId ToAccountId(this int value) => new(value);

        private static int ToInt(this AccessId accessId) => accessId.Value;

        private static AccessId ToAccessId(this int value) => new(value);

        public static partial IQueryable<AccountDto> ToDto(this IQueryable<Account> entities);
    }
}