namespace MainCore.Test.Parsers
{
    public class StorageParser : BaseParser
    {
        private const string Buildings = "Parsers/Storage/Buildings.html";

        [Fact]
        public void GetWood()
        {
            _html.Load(Buildings);
            var result = MainCore.Parsers.StorageParser.GetWood(_html);
            result.ShouldBeGreaterThan(-1);
        }

        [Fact]
        public void GetClay()
        {
            _html.Load(Buildings);
            var result = MainCore.Parsers.StorageParser.GetClay(_html);
            result.ShouldBeGreaterThan(-1);
        }

        [Fact]
        public void GetIron()
        {
            _html.Load(Buildings);
            var result = MainCore.Parsers.StorageParser.GetIron(_html);
            result.ShouldBeGreaterThan(-1);
        }

        [Fact]
        public void GetCrop()
        {
            _html.Load(Buildings);
            var result = MainCore.Parsers.StorageParser.GetCrop(_html);
            result.ShouldBeGreaterThan(-1);
        }

        [Fact]
        public void GetFreeCrop()
        {
            _html.Load(Buildings);
            var result = MainCore.Parsers.StorageParser.GetFreeCrop(_html);
            result.ShouldBeGreaterThan(-1);
        }

        [Fact]
        public void GetWarehouseCapacity()
        {
            _html.Load(Buildings);
            var result = MainCore.Parsers.StorageParser.GetWarehouseCapacity(_html);
            result.ShouldBeGreaterThan(-1);
        }

        [Fact]
        public void GetGranaryCapacity()
        {
            _html.Load(Buildings);
            var result = MainCore.Parsers.StorageParser.GetGranaryCapacity(_html);
            result.ShouldBeGreaterThan(-1);
        }
    }
}
