using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using System.Collections.Generic;
using System.Globalization;

namespace timebot.Classes
{
    public static class SwnbotResponseGet
    {
        public static SwnbotResponse GetResponse(ulong ID)
        {
            Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(Program.secrets_file));

            string key = secrets["token"];

            string baseurl = string.Concat("https://swnbot.itmebot.com/api/");

            var client = new RestClient(baseurl);

            var request = new RestRequest("user/{id}", Method.GET);
            request.AddParameter("name", "value");
            request.AddUrlSegment("id", ID);

            request.AddHeader("Authorization", key);

            var response = client.Execute(request);

            if (!response.IsSuccessful) return null;

            var content = response.Content.Replace('’', '\'');

            return SwnbotResponse.FromJson(content);
        }
    }

    public partial class SwnbotResponse
    {
        [JsonProperty("userID")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("userNick")]
        public string UserNick { get; set; }

        [JsonProperty("userRoles")]
        public UserRole[] UserRoles { get; set; }
    }

    public partial class UserRole
    {
        [JsonProperty("roleID")]
        public string RoleId { get; set; }

        [JsonProperty("factionName")]
        public string FactionName { get; set; }

        [JsonProperty("roleName")]
        public string RoleName { get; set; }
    }

    public partial class SwnbotResponse
    {
        public static SwnbotResponse FromJson(string json) => JsonConvert.DeserializeObject<SwnbotResponse>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this SwnbotResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}