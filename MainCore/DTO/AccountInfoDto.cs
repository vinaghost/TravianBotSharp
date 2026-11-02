namespace MainCore.DTO
{
    public class AccountInfoDto
    {
        public TribeEnums Tribe { get; set; } = TribeEnums.Any;
        public int Gold { get; set; } = 0;
        public int Silver { get; set; } = 0;
        public bool HasPlusAccount { get; set; } = false;
    }
}