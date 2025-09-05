namespace MainCore.Models
{
    public class BuildingItem
    {
        public BuildingId Id { get; set; }
        public int Location { get; set; }
        public BuildingEnums Type { get; set; }

        public int CurrentLevel { get; set; }
        public int QueueLevel { get; set; }
        public int JobLevel { get; set; }

        // Resource planning için mevcut gerçek seviyeyi kullan, queue seviyesini deðil
        // Çünkü queue henüz tamamlanmamýþ inþayý temsil ediyor
        public int Level => Math.Max(CurrentLevel, JobLevel);
        
        // UI gösterimi için tüm seviyeleri göster
        public int DisplayLevel => Math.Max(Math.Max(CurrentLevel, QueueLevel), JobLevel);
    }
}
