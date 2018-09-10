namespace timebot.Classes.FactionCount {
    using System.Collections.Generic;
    using System.Globalization;
    using System;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json;
    using RestSharp;

    public static class FactionCountGet {
        public static FactionCount GetCount (string short_name) {

            Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>> (System.IO.File.ReadAllText (Program.secrets_file));

            string key = secrets["token"];

            string baseurl = string.Concat ("https://swnbot.itmebot.com/api/");

            var client = new RestClient (baseurl);

            var request = new RestRequest ("faction/{id}", Method.GET);
            request.AddParameter ("name", "value");
            request.AddUrlSegment ("id", short_name);

            request.AddHeader ("Authorization", key);

            return FactionCount.FromJson (client.Execute (request).Content);
        }
    }

    public partial class FactionCount {
        [JsonProperty ("factionName")]
        public string FactionName { get; set; }

        [JsonProperty ("factionShortName")]
        public string FactionShortName { get; set; }

        [JsonProperty ("factionBotID")]
        public string FactionBotId { get; set; }

        [JsonProperty ("factionDiscordID")]
        public string FactionDiscordId { get; set; }

        [JsonProperty ("factionMemberCount")]
        public long FactionMemberCount { get; set; }

        [JsonProperty ("members")]
        public Member[] Members { get; set; }
    }

    public partial class Member {
        [JsonProperty ("user")]
        public User User { get; set; }

        [JsonProperty ("nick")]
        public string Nick { get; set; }

        [JsonProperty ("roles")]
        public string[] Roles { get; set; }

        [JsonProperty ("mute")]
        public bool Mute { get; set; }

        [JsonProperty ("joined_at")]
        public string JoinedAt { get; set; }

        [JsonProperty ("guild_id")]
        public object GuildId { get; set; }

        [JsonProperty ("deaf")]
        public bool Deaf { get; set; }
    }

    public partial class User {
        [JsonProperty ("id")]
        public string Id { get; set; }

        [JsonProperty ("username")]
        public string Username { get; set; }

        [JsonProperty ("discriminator")]
        public string Discriminator { get; set; }

        [JsonProperty ("avatar")]
        public string Avatar { get; set; }

        [JsonProperty ("bot")]
        public object Bot { get; set; }

        [JsonProperty ("mfa_enabled")]
        public object MfaEnabled { get; set; }

        [JsonProperty ("verified")]
        public object Verified { get; set; }

        [JsonProperty ("email")]
        public object Email { get; set; }
    }

    public partial class FactionCount {
        public static FactionCount FromJson (string json) => JsonConvert.DeserializeObject<FactionCount> (json, timebot.Classes.FactionCount.Converter.Settings);
    }

    public static class Serialize {
        public static string ToJson (this FactionCount self) => JsonConvert.SerializeObject (self, timebot.Classes.FactionCount.Converter.Settings);
    }

    internal static class Converter {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}