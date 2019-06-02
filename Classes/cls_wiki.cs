// Generated by https://quicktype.io

namespace timebot.Classes
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public partial class Wiki
    {
        [JsonProperty("batchcomplete")]
        public string Batchcomplete { get; set; }

        [JsonProperty("query")]
        public Query Query { get; set; }
    }

    public partial class Query
    {
        [JsonProperty("pages")]
        public Dictionary<string, Page> Pages { get; set; }
    }

    public partial class Page
    {
        [JsonProperty("pageid")]
        public int ID { get; set; }

        [JsonProperty("ns")]
        public string ns { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("extract")]
        public string Extract { get; set; }
    }

    public partial class Wiki
    {
        public static Wiki FromJson(string json) => JsonConvert.DeserializeObject<Wiki>(json, Converter.Settings);
    }

    public partial class Info
    {
        [JsonProperty("batchcomplete")]
        public string Batchcomplete { get; set; }

        [JsonProperty("query")]
        public InfoQuery Query { get; set; }
    }

    public partial class InfoQuery
    {
        [JsonProperty("pages")]
        public Dictionary<string, InfoPage> Pages { get; set; }
    }

    public partial class InfoPage
    {
        [JsonProperty("pageid")]
        public long Pageid { get; set; }

        [JsonProperty("ns")]
        public long Ns { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("contentmodel")]
        public string Contentmodel { get; set; }

        [JsonProperty("pagelanguage")]
        public string Pagelanguage { get; set; }

        [JsonProperty("pagelanguagehtmlcode")]
        public string Pagelanguagehtmlcode { get; set; }

        [JsonProperty("pagelanguagedir")]
        public string Pagelanguagedir { get; set; }

        [JsonProperty("touched")]
        public DateTimeOffset Touched { get; set; }

        [JsonProperty("lastrevid")]
        public long Lastrevid { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("fullurl")]
        public string URL { get; set; }
    }

    public partial class Info
    {
        public static Info FromJson(string json) => JsonConvert.DeserializeObject<Info>(json, Converter.Settings);
    }
}