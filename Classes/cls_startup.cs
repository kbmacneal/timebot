using Discord;
using Discord.WebSocket;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static timebot.Modules.Commands.commands;

namespace timebot.Classes
{
    public class Startup
    {
        public static async Task RegisterTimers(DiscordSocketClient client)
        {
            Program._timers.ForEach(e => e.Dispose());
            Program._timers.Clear();

            var chan = client.GetChannel(452989251966205964);

            var result = JsonConvert.DeserializeObject<List<nextevent>>(await "https://private.trilliantring.com"
                .AppendPathSegment("Home")
                .AppendPathSegment("GetAllPrivateEvents")
                .GetStringAsync());

            result.ForEach(async e =>
            {
                var max_possible_date = DateTimeOffset.Now.AddMilliseconds(Int32.MaxValue);

                if (DateTimeOffset.Parse(e.time) <= max_possible_date)
                {
                    await ReminderTimer.RegisterTimer(chan, e);
                }
            });
        }

        #pragma warning disable CS1998

        public static async Task SetUsername(DiscordSocketClient client)
        {
            client.Guilds.ToList().ForEach(async e =>
            {
                var _opt = new RequestOptions(){RetryMode = RetryMode.RetryRatelimit};
                var user = e.GetUser(client.CurrentUser.Id);
                await user.ModifyAsync(e=>e.Nickname = "Arch Lector Frederick of Timebot",_opt);
                await client.SetStatusAsync(Discord.UserStatus.Online);
                await client.SetGameAsync("World Domination",null,Discord.ActivityType.Playing);
            });
        }

        #pragma warning restore CS1998
    }
}