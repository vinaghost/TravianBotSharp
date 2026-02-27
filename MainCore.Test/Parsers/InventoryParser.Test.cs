using MainCore.Enums;

namespace MainCore.Test.Parsers
{
    public class InventoryParser : BaseParser
    {
        private const string HeroInventory = "Parsers/Inventory/HeroInventory.html";
        private const string Buildings = "Parsers/Inventory/Buildings.html";
        private const string AmountDialogResourceTransfer = "Parsers/Inventory/AmountDialogResourceTransfer.html";

        [Theory]
        [InlineData(HeroInventory, true)]
        [InlineData(Buildings, false)]
        public void IsInventoryPage(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.InventoryParser.IsInventoryPage(_html);
            actual.ShouldBe(expected);
        }

        [Fact]
        public void GetHeroAvatar()
        {
            _html.Load(HeroInventory);
            var actual = MainCore.Parsers.InventoryParser.GetHeroAvatar(_html);
            actual.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(HeroInventory, true)]
        [InlineData(Buildings, false)]
        public void IsInventoryLoaded(string file, bool expected)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.InventoryParser.IsInventoryLoaded(_html);
            actual.ShouldBe(expected);
        }

        [Theory]
        [InlineData(HeroInventory, HeroItemEnums.Crop)]
        [InlineData(HeroInventory, HeroItemEnums.Wood)]
        [InlineData(HeroInventory, HeroItemEnums.Iron)]
        [InlineData(HeroInventory, HeroItemEnums.Clay)]
        public void GetItemSlot(string file, HeroItemEnums type)
        {
            _html.Load(file);
            var actual = MainCore.Parsers.InventoryParser.GetItemSlot(_html, type);
            actual.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(HeroItemEnums.Wood, "lumber")]
        [InlineData(HeroItemEnums.Clay, "clay")]
        [InlineData(HeroItemEnums.Iron, "iron")]
        [InlineData(HeroItemEnums.Crop, "crop")]
        public void GetAmountBox_ResourceDialog_ByItem(HeroItemEnums item, string expectedName)
        {
            _html.Load(AmountDialogResourceTransfer);
            var actual = MainCore.Parsers.InventoryParser.GetAmountBox(_html, item);
            actual.ShouldNotBeNull();
            actual.GetAttributeValue("name", "").ShouldBe(expectedName);
        }

        [Fact]
        public void GetConfirmButton_ResourceDialog_ShouldPickTransferAction()
        {
            _html.Load(AmountDialogResourceTransfer);
            var actual = MainCore.Parsers.InventoryParser.GetConfirmButton(_html, HeroItemEnums.Wood);
            actual.ShouldNotBeNull();
            actual.GetAttributeValue("id", "").ShouldBe("transferBtn");
        }
    }
}
