using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net.Http;

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

            string url = string.Concat("https://swnbot.itmebot.com/api/user/", ID.ToString());

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", key);

            HttpResponseMessage res = client.GetAsync(url).GetAwaiter().GetResult();

            HttpContent content = res.Content;

            string data = content.ReadAsStringAsync().GetAwaiter().GetResult();

            return SwnbotResponse.FromJson(data);
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
