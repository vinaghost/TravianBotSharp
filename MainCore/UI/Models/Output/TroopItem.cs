using Humanizer;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;
using ReactiveUI;
using System.Drawing;

namespace MainCore.UI.Models.Output
{
    public class TroopItem : ReactiveObject
    {
        public static readonly Dictionary<int, int> ImageOffset = new()
        {
            {1 , 0 },
            {2, 19 },
            {3, 38 },
            {4, 57 },
            {5, 76 },
            {6, 95 },
            {7, 114},
            {8, 133},
            {9, 152},
            {0, 171},
        };

        public static readonly Dictionary<TribeEnums, string> TribeImage = new()
        {
            {TribeEnums.Romans, "roman.png" },
            {TribeEnums.Teutons, "teuton.png" },
            {TribeEnums.Gauls, "gaul.png" },
            {TribeEnums.Nature, "nature.png" },
            {TribeEnums.Natars, "natar.png" },
            {TribeEnums.Egyptians, "egyptian.png" },
            {TribeEnums.Huns, "hun.png" },
            {TribeEnums.Spartan, "spartan.png" },  //REGE
        };

        public TroopItem(TroopEnums troop)
        {
            Troop = troop;
        }

        public static string GetImageSource(TroopEnums troop)
        {
            //const string url = "pack://application:,,,/MainCore;component/UI/Resources/";
            const string url = "pack://application:,,,/Resources/";
            if (troop == TroopEnums.None) return $"{url}{TribeImage[TribeEnums.Natars]}";
            return $"{url}{TribeImage[troop.GetTribe()]}";
        }

        public static Rectangle GetImageMask(TroopEnums troop)
        {
            if (troop == TroopEnums.None) return new(57, 0, 16, 16);
            return new(ImageOffset[(int)troop % 10], 0, 16, 16);
        }

        public TroopEnums Troop { get; }

        public string TroopName => Troop.Humanize();

        public string ImageSource => GetImageSource(Troop);
        public Rectangle ImageMask => GetImageMask(Troop);
    }
}