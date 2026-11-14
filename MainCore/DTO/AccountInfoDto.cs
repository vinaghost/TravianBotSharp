namespace MainCore.DTO
{
    public class AccountInfoDto
    {
        public int Tribe { get; set; }
        public int Gold { get; set; } = 0;
        public int Silver { get; set; } = 0;
        public bool HasPlusAccount { get; set; } = false;
    }
}