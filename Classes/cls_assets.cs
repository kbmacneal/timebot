// Generated by https://quicktype.io

namespace timebot.Classes.Assets
{
    using System.Collections.Generic;
    using System.Linq;
    using timebot.Contexts;

    public class TrackerAsset
    {
        public string Owner { get; set; }
        public string Asset { get; set; }
        public string Cost { get; set; }
        public string Stealthed { get; set; }
        public string Stat { get; set; }
        public string HP { get; set; }
        public string MaxHP { get; set; }
        public string CombinedHP { get; set; }
        public string Type { get; set; }
        public string Attack { get; set; }
        public string Counter { get; set; }
        public string Notes { get; set; }
        public string Upkeep { get; set; }
        public string Location { get; set; }
    }

    public class Asset
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string HP { get; set; }
        public string Attack { get; set; }
        public string Counterattack { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public string Tier { get; set; }

        public string TechLevel { get; set; }

        public string Cost { get; set; }

        public string AssetType { get; set; }

        public static List<Asset> GetAssets()
        {
            using (var context = new Context())
            {
                var assets = context.Assets.ToList();

                return assets;
            }
        }
    }
}