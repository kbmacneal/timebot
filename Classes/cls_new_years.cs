using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JsonFlatFileDataStore;
using Newtonsoft.Json;
using NodaTime;

namespace timebot.Classes {
    public class new_years {

        public static List<Timer> timers {get;set;} = new List<Timer>();
        public static async Task<List<string>> schedule_times (SocketCommandContext context) {

            // Times.FromJson("new_years.json").ToList();

            List<string> rtn = new List<string> ();

            var event_list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Times>> (System.IO.File.ReadAllText ("new_years.json"));

            // await ScheduleAction(context, null);

            List<string> times = new List<string> ();

            event_list.ForEach (e => {
                ScheduleAction (context, e);
                times.Add ("Scheduled " + e.ToString());
                rtn.Add (e.Countries);
            });

            times.ForEach(e=>Console.WriteLine(e.ToString()));
            Console.WriteLine("Timers Scheduled:");
            Console.WriteLine(timers.Count());

            await System.IO.File.WriteAllLinesAsync ("timers_scheduled.txt", times);
            System.IO.File.AppendAllText("timers_scheduled.txt","\nTimers Scheduled:");
            System.IO.File.AppendAllText("timers_scheduled.txt","\n" + timers.Count().ToString());

            return rtn;

        }

        public static Timer CreateTimer (SocketCommandContext con, Times t) {
            var tmr = new Timer (async x => await new_year_callback (con, t));

            timers.Add(tmr);

            return tmr;
        }

        public static void ScheduleAction (SocketCommandContext con, Times e) {
            var t = CreateTimer (con, e);

            Instant now = SystemClock.Instance.GetCurrentInstant ();

            ZonedDateTime now_zoned = new ZonedDateTime (now, DateTimeZoneProviders.Tzdb.GetSystemDefault ());

            NodaTime.ZonedDateTime dt = new NodaTime.ZonedDateTime (Instant.FromDateTimeOffset (new DateTimeOffset (DateTime.Parse (e.CentralStandardTime))), NodaTime.DateTimeZone.ForOffset (NodaTime.Offset.FromHours (-6)), NodaTime.CalendarSystem.Gregorian);

            Duration interval = dt - now_zoned;

            t.Change ((int) interval.TotalMilliseconds, Timeout.Infinite);

        }

        public static async Task new_year_callback (SocketCommandContext con, Times t) {

            var out_channel = con.Guild.GetChannel (con.Guild.Channels.First (e => e.Name == "new-years-announcements").Id) as ISocketMessageChannel;

            if (out_channel == null) return;

            List<string> output = new List<string> ();
            output.Add ("HAPPY NEW YEAR!");
            output.Add (t.CentralStandardTime);
            output.Add (t.Countries);
            output.Add (t.Cities);

            await out_channel.SendMessageAsync (String.Join (System.Environment.NewLine, output), false, null, new RequestOptions { RetryMode = RetryMode.RetryRatelimit });
        }
    }

    public partial class Times {
        [JsonProperty ("Central_Standard_Time")]
        public string CentralStandardTime { get; set; }

        [JsonProperty ("Countries")]
        public string Countries { get; set; }

        [JsonProperty ("Cities")]
        public string Cities { get; set; }

        public override string ToString()        
        {
            return this.CentralStandardTime + " " + this.Countries + " " + this.Cities;
        }
    }

    public partial class Times {
        public static Times[] FromJson (string json) => JsonConvert.DeserializeObject<Times[]> (json, Converter.Settings);
    }
}