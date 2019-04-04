using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using timebot.Classes.FactionCount;
using timebot.Contexts;

namespace timebot.Classes
{
    public class PopCount
    {
        public int ID { get; set; }
        public ulong FactionID { get; set; }
        public string FactionName { get; set; }
        public int MemCount { get; set; }
        public DateTime timestamp { get; set; }

        public static async Task<List<PopCount>> GetCounts()
        {
            List<Classes.Faction> official_factions = Classes.Factions.get_factions ().apiFactions.ToList ();

            List<PopCount> rtn = new List<PopCount> ();

            official_factions.ForEach (e => rtn.Add (new PopCount () { FactionID = Convert.ToUInt64 (e.FactionDiscordID), FactionName = e.FactionName, timestamp = DateTime.Now, MemCount = Convert.ToInt32 (FactionCountGet.GetCount (e.FactionShortName).Members.Count ().ToString ()) }));

            using (var context = new Context ())
            {
                foreach (var item in rtn)
                {
                    await context.PopCounts.AddAsync (item);
                }

                await context.SaveChangesAsync ();
            }

            return rtn;
        }
    }
}