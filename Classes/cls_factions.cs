using Newtonsoft.Json;
using RestSharp;

namespace timebot.Classes
{
    public partial class Factions
    {
        [JsonProperty("factions")]
        public Faction[] apiFactions { get; set; }

        public static Factions get_factions()
        {
            string baseurl = string.Concat("https://swnbot.itmebot.com/api/");

            var client = new RestClient(baseurl);

            var request = new RestRequest("faction", Method.GET);

            var complete = client.Execute(request);

            string content = complete.Content;

            return Factions.FromJson(content);
        }
    }

    public partial class Faction
    {
        [JsonProperty("factionName")]
        public string FactionName { get; set; }

        [JsonProperty("factionShortName")]
        public string FactionShortName { get; set; }

        [JsonProperty("factionBotID")]
        public string FactionBotId { get; set; }

        [JsonProperty("factionDiscordID")]
        public string FactionDiscordID { get; set; }
    }

    public partial class Factions
    {
        public static Factions FromJson(string json) => JsonConvert.DeserializeObject<Factions>(json, Converter.Settings);
    }
}