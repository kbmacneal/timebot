namespace timebot.Classes
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class SwnbotResponse
    {
        private static IConfigurationRoot Configuration;
        const string ConnectionSecretName = "SWNBotToken";

        [JsonProperty("userID")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("userNick")]
        public object UserNick { get; set; }

        [JsonProperty("userRoles")]
        public UserRole[] UserRoles { get; set; }

        public static SwnbotResponse Get_Response(uint ID)
        {
            SwnbotResponse rtn = new SwnbotResponse();

            

            return rtn;
        }
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
