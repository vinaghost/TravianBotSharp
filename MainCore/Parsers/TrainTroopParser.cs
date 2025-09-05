namespace MainCore.Parsers
{
    public static class TrainTroopParser
    {
        public static HtmlNode? GetInputBox(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            if (node is null) return null;
            var cta = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("cta"));
            if (cta is null) return null;

            // Önce class="text" olan input'u ara
            var input = cta.Descendants("input")
                .FirstOrDefault(x => x.HasClass("text"));
            
            // Eğer bulunamazsa, name attribute'u ile ara (t1, t2, t3, t4 gibi)
            if (input is null)
            {
                var troopIndex = (int)troop;
                var nameAttribute = $"t{troopIndex}";
                input = cta.Descendants("input")
                    .FirstOrDefault(x => x.GetAttributeValue("name", "") == nameAttribute);
            }
            
            // Hala bulunamazsa, cta içindeki herhangi bir input'u al
            if (input is null)
            {
                input = cta.Descendants("input")
                    .FirstOrDefault(x => x.GetAttributeValue("type", "") == "text");
            }
            
            return input;
        }

        public static int GetMaxAmount(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            if (node is null) return 0;
            var cta = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("cta"));
            if (cta is null) return 0;
            var a = cta.Descendants("a")
                .FirstOrDefault();
            if (a is null) return 0;

            return a.InnerText.ParseInt();
        }

        public static HtmlNode GetTrainButton(HtmlDocument doc)
        {
            return doc.GetElementbyId("s1");
        }

        // Alternatif input bulma yöntemi - yeni HTML yapısına göre
        public static HtmlNode? GetInputBoxAlternative(HtmlDocument doc, TroopEnums troop)
        {
            // Önce yeni yapıya göre ara: nonFavouriteTroops veya favouriteTroops
            var nonFavouriteTroops = doc.GetElementbyId("nonFavouriteTroops");
            var favouriteTroops = doc.GetElementbyId("favouriteTroops");
            
            // Troop enum'ına göre hangi bölümde olduğunu belirle
            var troopIndex = (int)troop;
            var troopDiv = GetTroopDiv(nonFavouriteTroops, troop) ?? GetTroopDiv(favouriteTroops, troop);
            
            if (troopDiv is not null)
            {
                // Yeni yapıya göre input'u bul: div/div[2]/div[4]/input
                var input = troopDiv.Descendants("input")
                    .FirstOrDefault(x => x.GetAttributeValue("type", "") == "text");
                
                if (input is not null) return input;
            }
            
            // Fallback: Eski yöntem - form içindeki name attribute ile ara
            var form = doc.DocumentNode.Descendants("form").FirstOrDefault();
            if (form is not null)
            {
                var nameAttribute = $"t{troopIndex}";
                var input = form.Descendants("input")
                    .FirstOrDefault(x => x.GetAttributeValue("name", "") == nameAttribute);
                
                if (input is not null) return input;
            }
            
            return null;
        }

        // Troop div'ini bulma yardımcı metodu
        private static HtmlNode? GetTroopDiv(HtmlNode? container, TroopEnums troop)
        {
            if (container is null) return null;
            
            var troopIndex = (int)troop;
            var troopDivs = container.Descendants("div")
                .Where(x => x.HasClass("action") && x.HasClass("troop"))
                .ToList();
            
            // Troop index'e göre doğru div'i bul
            if (troopDivs.Count > troopIndex - 1)
            {
                return troopDivs[troopIndex - 1];
            }
            
            return null;
        }

        // Yeni HTML yapısına özel input bulma metodu
        public static HtmlNode? GetInputBoxNewStructure(HtmlDocument doc, TroopEnums troop)
        {
            // Troop enum değerini HTML name attribute'una çevir
            var troopIndex = GetTroopIndex(troop);
            if (troopIndex == -1) return null;
            
            // Önce nonFavouriteTroops'ta ara
            var nonFavouriteTroops = doc.GetElementbyId("nonFavouriteTroops");
            if (nonFavouriteTroops is not null)
            {
                var troopDivs = nonFavouriteTroops.Descendants("div")
                    .Where(x => x.HasClass("action") && x.HasClass("troop"))
                    .ToList();
                
                if (troopDivs.Count > troopIndex - 1)
                {
                    var input = troopDivs[troopIndex - 1].Descendants("input")
                        .FirstOrDefault(x => x.GetAttributeValue("type", "") == "text");
                    if (input is not null) return input;
                }
            }
            
            // Sonra favouriteTroops'ta ara
            var favouriteTroops = doc.GetElementbyId("favouriteTroops");
            if (favouriteTroops is not null)
            {
                var troopDivs = favouriteTroops.Descendants("div")
                    .Where(x => x.HasClass("action") && x.HasClass("troop"))
                    .ToList();
                
                if (troopDivs.Count > troopIndex - 1)
                {
                    var input = troopDivs[troopIndex - 1].Descendants("input")
                        .FirstOrDefault(x => x.GetAttributeValue("type", "") == "text");
                    if (input is not null) return input;
                }
            }
            
            return null;
        }

        // Troop enum değerini HTML name attribute'una çeviren yardımcı metod
        private static int GetTroopIndex(TroopEnums troop)
        {
            return troop switch
            {
                // Romans
                TroopEnums.Legionnaire => 1,
                TroopEnums.Praetorian => 2,
                TroopEnums.Imperian => 3,
                TroopEnums.EquitesLegati => 4,
                TroopEnums.EquitesImperatoris => 5,
                TroopEnums.EquitesCaesaris => 6,
                TroopEnums.RomanRam => 7,
                TroopEnums.RomanCatapult => 8,
                TroopEnums.RomanChief => 9,
                TroopEnums.RomanSettler => 10,

                // Teutons
                TroopEnums.Clubswinger => 1,
                TroopEnums.Spearman => 2,
                TroopEnums.Axeman => 3,
                TroopEnums.Scout => 4,
                TroopEnums.Paladin => 5,
                TroopEnums.TeutonicKnight => 6,
                TroopEnums.TeutonRam => 7,
                TroopEnums.TeutonCatapult => 8,
                TroopEnums.TeutonChief => 9,
                TroopEnums.TeutonSettler => 10,

                // Gauls
                TroopEnums.Phalanx => 1,
                TroopEnums.Swordsman => 2,
                TroopEnums.Pathfinder => 3,
                TroopEnums.TheutatesThunder => 4,
                TroopEnums.Druidrider => 5,
                TroopEnums.Haeduan => 6,
                TroopEnums.GaulRam => 7,
                TroopEnums.GaulCatapult => 8,
                TroopEnums.GaulChief => 9,
                TroopEnums.GaulSettler => 10,

                // Egyptians
                TroopEnums.SlaveMilitia => 1,
                TroopEnums.AshWarden => 2,
                TroopEnums.KhopeshWarrior => 3,
                TroopEnums.SopduExplorer => 4,
                TroopEnums.AnhurGuard => 5,
                TroopEnums.ReshephChariot => 6,
                TroopEnums.EgyptianRam => 7,
                TroopEnums.EgyptianCatapult => 8,
                TroopEnums.EgyptianChief => 9,
                TroopEnums.EgyptianSettler => 10,

                // Huns
                TroopEnums.Mercenary => 1,
                TroopEnums.Bowman => 2,
                TroopEnums.Spotter => 3,
                TroopEnums.SteppeRider => 4,
                TroopEnums.Marksman => 5,
                TroopEnums.Marauder => 6,
                TroopEnums.HunRam => 7,
                TroopEnums.HunCatapult => 8,
                TroopEnums.HunChief => 9,
                TroopEnums.HunSettler => 10,

                _ => -1
            };
        }

        private static HtmlNode? GetNode(HtmlDocument doc, TroopEnums troop)
        {
            var nodes = doc.DocumentNode.Descendants("div")
               .Where(x => x.HasClass("troop"))
               .Where(x => !x.HasClass("empty"))
               .AsEnumerable();

            foreach (var node in nodes)
            {
                var img = node.Descendants("img")
                .FirstOrDefault(x => x.HasClass("unit"));
                if (img is null) continue;
                var classes = img.GetClasses();
                var type = classes
                    .Where(x => x.StartsWith('u'))
                    .FirstOrDefault(x => !x.Equals("unit"));
                if (type is null) continue;
                if (type.ParseInt() == (int)troop) return node;
            }
            return null;
        }
    }
}
