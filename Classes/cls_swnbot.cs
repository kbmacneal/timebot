using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;

namespace timebot.Classes
{
    public static class SwnbotResponseGet
    {
        public static SwnbotResponse GetResponse(uint ID)
        {
            string file = "swnbot.json";

            Dictionary<string, string> secrets = new Dictionary<string, string>();

            secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(file));

            string key = secrets["token"];

            string baseurl = string.Concat("https://swnbot.itmebot.com/api/");

            var client = new RestClient(baseurl);

            var request = new RestRequest("user/{id}", Method.GET);
            request.AddParameter("name", "value");
            request.AddUrlSegment("id", ID);

            IRestResponse response = client.Execute(request);
            var content = response.Content;

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
        public object UserNick { get; set; }

        [JsonProperty("userRoles")]
        public UserRole[] UserRoles { get; set; }


    }

    public partial class UserRole
    {
        [JsonProperty("roleID")]
        public string RoleId { get; set; }

        [JsonProperty("roleName")]
        public string RoleName { get; set; }
    }

    public partial class SwnbotResponse
    {
        public static SwnbotResponse FromJson(string json) => JsonConvert.DeserializeObject<SwnbotResponse>(json, timebot.Classes.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this SwnbotResponse self) => JsonConvert.SerializeObject(self, timebot.Classes.Converter.Settings);
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
