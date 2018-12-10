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
using System.Threading.Tasks;

namespace timebot.Classes
{
    public class new_years
    {
        public static List<Times> event_list = Times.FromJson("new_years.json").ToList();

        public static void schedule_times(CommandContext context)
        {
            event_list.ForEach(async e => await ScheduleAction(context, e));
        }

        public static Timer CreateTimer(CommandContext con, Times t)
        {

            return new Timer(async x => await new_year_callback(con, t));
        }


        public static async Task ScheduleAction(CommandContext con, Times e)
        {
            var t = CreateTimer(con, e);
            
            TimeSpan span = e.CentralStandardTime - DateTime.Now;

            // int msUntilTime = (int)((e.CentralStandardTime - now).TotalMilliseconds);

            t.Change((int)span.TotalMilliseconds, Timeout.Infinite);
        }

        public static async Task new_year_callback(CommandContext con, Times t)
        {
             var channels = await con.Guild.GetChannelsAsync();
             ISocketMessageChannel out_channel = (ISocketMessageChannel)con.Guild.GetChannelAsync(channels.FirstOrDefault(e=>e.Name == "new-years-announcements").Id).GetAwaiter().GetResult();

             if(out_channel==null)return;
             
             Embed output = Helper.ObjToEmbed(t,"");

             await out_channel.SendMessageAsync("",false,output, new RequestOptions{RetryMode = RetryMode.RetryRatelimit});
        }
    }



    public partial class Times
    {
        [JsonProperty("Central_Standard_Time")]
        public DateTime CentralStandardTime { get; set; }

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



