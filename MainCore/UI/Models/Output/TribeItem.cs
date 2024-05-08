using ReactiveUI;
using System.Collections.Immutable;
using System.Drawing;

namespace MainCore.UI.Models.Output
{
    public class TribeItem : ReactiveObject
    {
        public static readonly ImmutableDictionary<TribeEnums, string> TribeImage = new Dictionary<TribeEnums, string>
        {
            {TribeEnums.Any, "natar_big.png" },
            {TribeEnums.Romans, "roman_big.png" },
            {TribeEnums.Teutons, "teuton_big.png" },
            {TribeEnums.Gauls, "gaul_big.png" },
            {TribeEnums.Nature, "nature_big.png" },
            {TribeEnums.Natars, "natar_big.png" },
            {TribeEnums.Egyptians, "egyptian_big.png" },
            {TribeEnums.Huns, "hun_big.png" },
        }.ToImmutableDictionary();

        public TribeItem(TribeEnums tribe)
        {
            Tribe = tribe;
        }

        public static string GetImageSource(TribeEnums tribe)
        {
            const string url = "pack://application:,,,/Resources/";
            return $"{url}{TribeImage[tribe]}";
        }

        public TribeEnums Tribe { get; }

        public string ImageSource => GetImageSource(Tribe);
        public static Rectangle ImageMask => new(0, 0, 61, 61);
    }
}