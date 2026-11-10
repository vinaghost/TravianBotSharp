#nullable disable

namespace MainCore.DTO
{
    public class AccountDto
    {
        public AccountId Id { get; set; }
        public string Username { get; set; }
        public string Server { get; set; }
        public List<AccessDto> Accesses { get; set; }
    }
}