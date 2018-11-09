// Generated by https://quicktype.io

namespace timebot.Classes.Tags
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Tag
    {
        [JsonProperty("Tag")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
    }

    public partial class Tag
    {
        public static Tag[] FromJson(string json) => JsonConvert.DeserializeObject<Tag[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Tag[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
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