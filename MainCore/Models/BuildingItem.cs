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

        // Resource planning i�in mevcut ger�ek seviyeyi kullan, queue seviyesini de�il
        // ��nk� queue hen�z tamamlanmam�� in�ay� temsil ediyor
        public int Level => Math.Max(CurrentLevel, JobLevel);
        
        // UI g�sterimi i�in t�m seviyeleri g�ster
        public int DisplayLevel => Math.Max(Math.Max(CurrentLevel, QueueLevel), JobLevel);
    }
}
