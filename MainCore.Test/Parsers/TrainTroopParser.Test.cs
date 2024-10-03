using MainCore.Common.Enums;

namespace MainCore.Test.Parsers
{
    public class TrainTroopParser : BaseParser
    {
        private const string BarrackPage = "Parsers/TrainTroop/BarrackPage.html";
        private const string StablePage = "Parsers/TrainTroop/StablePage.html";

        [Theory]
        [InlineData(BarrackPage)]
        [InlineData(StablePage)]
        public void GetTrainButton(string file)
        {
            _html.Load(file);
            var result = MainCore.Parsers.TrainTroopParser.GetTrainButton(_html);
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData(BarrackPage, TroopEnums.Phalanx)]
        [InlineData(StablePage, TroopEnums.Pathfinder)]
        public void GetInputBox(string file, TroopEnums troop)
        {
            _html.Load(file);
            var result = MainCore.Parsers.TrainTroopParser.GetInputBox(_html, troop);
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData(BarrackPage, TroopEnums.Phalanx)]
        [InlineData(StablePage, TroopEnums.Pathfinder)]
        public void GetTrainTime(string file, TroopEnums troop)
        {
            _html.Load(file);
            var result = MainCore.Parsers.TrainTroopParser.GetMaxAmount(_html, troop);
            result.Should().BeGreaterThan(-1);
        }
    }
}