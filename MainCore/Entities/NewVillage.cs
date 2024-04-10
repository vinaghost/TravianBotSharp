﻿namespace MainCore.Entities
{
    public class NewVillage
    {
        public int Id { get; set; }
        public int AccountId { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public int VillageId { get; set; }

        public string NewVillageTemplatePath { get; set; } = "";
    }
}