// Generated by https://quicktype.io

namespace timebot.Classes.Assets
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class TrackerAsset
    {
        public string Owner {get;set;}
        public string Asset {get;set;}
        public string Stealthed {get;set;}
        public string Stat {get;set;}
        public string HP {get;set;}
        public string MaxHP  {get;set;}
        public string Type  {get;set;}
        public string Attack  {get;set;}
        public string Counter  {get;set;}
        public string Notes  {get;set;}
        public string Location  {get;set;}
    }

    public partial class Asset
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("HP")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Hp { get; set; }

        [JsonProperty("Attack")]
        public string Attack { get; set; }

        [JsonProperty("Counterattack")]
        public string Counterattack { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Tier")]
        public string Tier { get; set; }

        [JsonProperty("TechLevel")]
        public string TechLevel { get; set; }

        [JsonProperty("Cost")]
        public string Cost { get; set; }

        [JsonProperty("AssetType")]
        public string AssetType { get; set; }
    }

    public enum TypeEnum { Cunning, Force, Special, Wealth };

    public partial class Asset
    {
        public static Asset[] FromJson(string json) => JsonConvert.DeserializeObject<Asset[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Asset[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                TypeEnumConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
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

    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Cunning":
                    return TypeEnum.Cunning;
                case "Force":
                    return TypeEnum.Force;
                case "Special":
                    return TypeEnum.Special;
                case "Wealth":
                    return TypeEnum.Wealth;
            }
            throw new Exception("Cannot unmarshal type TypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TypeEnum)untypedValue;
            switch (value)
            {
                case TypeEnum.Cunning:
                    serializer.Serialize(writer, "Cunning");
                    return;
                case TypeEnum.Force:
                    serializer.Serialize(writer, "Force");
                    return;
                case TypeEnum.Special:
                    serializer.Serialize(writer, "Special");
                    return;
                case TypeEnum.Wealth:
                    serializer.Serialize(writer, "Wealth");
                    return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
    }
}
