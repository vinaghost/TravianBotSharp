using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class StorageDto
    {
        public long Wood { get; set; }
        public long Clay { get; set; }
        public long Iron { get; set; }
        public long Crop { get; set; }
        public long Warehouse { get; set; }
        public long Granary { get; set; }
        public long FreeCrop { get; set; }
    }
}