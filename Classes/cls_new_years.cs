using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JsonFlatFileDataStore;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using NodaTime;

namespace timebot.Classes
{
    public class new_years
    {
        public static List<Times> event_list = new List<Times>();

        public static async Task<List<string>> schedule_times(SocketCommandContext context)
        {

            // Times.FromJson("new_years.json").ToList();

            List<string> rtn = new List<string>();

            var event_list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Times>>(System.IO.File.ReadAllText("new_years.json"));

            // await ScheduleAction(context, null);

            event_list.ForEach(async e => {
                await ScheduleAction(context, e);
                rtn.Add(e.Countries);
            }); 


            return rtn;

        }

        public static Timer CreateTimer(SocketCommandContext con, Times t)
        {

            return new Timer(async x => await new_year_callback(con, t));
        }


        public static async Task ScheduleAction(SocketCommandContext con, Times e)
        {


            e = new Times()
            {
                CentralStandardTime = DateTime.Now.ToString(),
                Cities = "test",
                Countries = "test"
            };

            var t = CreateTimer(con, e);

            Instant now = SystemClock.Instance.GetCurrentInstant();

            ZonedDateTime now_zoned = new ZonedDateTime(now, DateTimeZoneProviders.Tzdb.GetSystemDefault());

            NodaTime.ZonedDateTime dt = new NodaTime.ZonedDateTime(Instant.FromDateTimeOffset(new DateTimeOffset(DateTime.Parse(e.CentralStandardTime))), NodaTime.DateTimeZone.ForOffset(NodaTime.Offset.FromHours(-6)), NodaTime.CalendarSystem.Gregorian);

            Duration interval = dt - now_zoned;

            t.Change((int)interval.TotalMilliseconds, Timeout.Infinite);

            // t.Change(10000, Timeout.Infinite);
        }

        public static async Task new_year_callback(SocketCommandContext con, Times t)
        {
            //  var channels = await con.Guild.GetChannelsAsync();
            //  ISocketMessageChannel out_channel = (ISocketMessageChannel)con.Guild.GetChannelAsync(channels.FirstOrDefault(e=>e.Name == "new-years-announcements").Id).GetAwaiter().GetResult();

            var out_channel = con.Guild.GetChannel(con.Guild.Channels.First(e => e.Name == "new-years-announcements").Id) as ISocketMessageChannel;

            if (out_channel == null) return;

            List<string> output = new List<string>();
            output.Add(t.CentralStandardTime);
            output.Add(t.Countries);
            output.Add(t.Cities);

            await out_channel.SendMessageAsync(String.Join(System.Environment.NewLine,output), false, null, new RequestOptions { RetryMode = RetryMode.RetryRatelimit });
        }
    }



    public partial class Times
    {
        [JsonProperty("Central_Standard_Time")]
        public string CentralStandardTime { get; set; }

        [JsonProperty("Countries")]
        public string Countries { get; set; }

        [JsonProperty("Cities")]
        public string Cities { get; set; }
    }

    public partial class Times
    {
        public static Times[] FromJson(string json) => JsonConvert.DeserializeObject<Times[]>(json, Converter.Settings);
    }
}



