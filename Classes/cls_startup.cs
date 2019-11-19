using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using static timebot.Modules.Commands.commands;

namespace timebot.Classes {
    public class Startup {
        public static async Task RegisterTimers (DiscordSocketClient client) {
            Program._timers.ForEach(async e => await e.DisposeAsync());
            Program._timers.Clear();

            var chan = client.GetChannel (452989251966205964);

            var result = JsonConvert.DeserializeObject<List<nextevent>> (await "https://private.trilliantring.com"
                .AppendPathSegment ("Home")
                .AppendPathSegment ("GetAllPrivateEvents")
                .GetStringAsync ());

            result.ForEach (async e => {
                await ReminderTimer.RegisterTimer (chan,e);
            });
        }

        public static async Task SetUsername(DiscordSocketClient client)
        {
            client.Guilds.ToList().ForEach(async e =>
            {
                var user = e.GetUser(client.CurrentUser.Id);
                await user.ModifyAsync(e=>e.Nickname = "Arch Lector Frederick of Timebot",null);
                await client.SetStatusAsync(Discord.UserStatus.Online);
                await client.SetGameAsync("World Domination",null,Discord.ActivityType.Playing);
            });
        }
    }
}