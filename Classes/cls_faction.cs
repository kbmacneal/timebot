using System;
using System.Collections.Generic;
using RestSharp;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace timebot.Classes
{
    public static class SwnbotFactionResponse {
        public static SwnbotResponse GetResponse (ulong ID) {
            string file = "swnbot.json";

            Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>> (System.IO.File.ReadAllText (file));

            string key = secrets["token"];

            string baseurl = string.Concat ("https://swnbot.itmebot.com/api/");

            var client = new RestClient (baseurl);

            var request = new RestRequest ("faction/{id}", Method.GET);
            request.AddParameter ("name", "value");
            request.AddUrlSegment ("id", ID);

            request.AddHeader ("Authorization", key);

            return SwnbotResponse.FromJson (client.Execute (request).Content);
        }
    }

    public partial class FactionList
    {
        [JsonProperty("factionName")]
        public string FactionName { get; set; }

        [JsonProperty("factionShortName")]
        public string FactionShortName { get; set; }

        [JsonProperty("factionBotID")]
        public string FactionBotId { get; set; }

        [JsonProperty("factionDiscordID")]
        public string FactionDiscordId { get; set; }

        [JsonProperty("factionMemberCount")]
        public long FactionMemberCount { get; set; }

        [JsonProperty("members")]
        public Member[] Members { get; set; }
    }

    public partial class Member
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("nick")]
        public string Nick { get; set; }

        [JsonProperty("roles")]
        public string[] Roles { get; set; }

        [JsonProperty("mute")]
        public bool Mute { get; set; }

        [JsonProperty("joined_at")]
        public string JoinedAt { get; set; }

        [JsonProperty("guild_id")]
        public object GuildId { get; set; }

        [JsonProperty("deaf")]
        public bool Deaf { get; set; }
    }

    public partial class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("discriminator")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Discriminator { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("bot")]
        public object Bot { get; set; }

        [JsonProperty("mfa_enabled")]
        public object MfaEnabled { get; set; }

        [JsonProperty("verified")]
        public object Verified { get; set; }

        [JsonProperty("email")]
        public object Email { get; set; }
    }

    public partial class FactionList
    {
        public static FactionList FromJson(string json) => JsonConvert.DeserializeObject<FactionList>(json, Converter.Settings);
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
