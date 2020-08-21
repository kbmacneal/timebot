using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace timebot.Classes
{
    public class Startup
    {
#pragma warning disable CS1998

        public static async Task SetUsername(DiscordSocketClient client)
        {
            client.Guilds.ToList().ForEach(async e =>
            {
                var _opt = new RequestOptions() { RetryMode = RetryMode.RetryRatelimit };
                var user = e.GetUser(client.CurrentUser.Id);
                await user.ModifyAsync(e => e.Nickname = "Arch Lector Frederick of Timebot", _opt);
                await client.SetStatusAsync(Discord.UserStatus.Online);
                await client.SetGameAsync("World Domination", null, Discord.ActivityType.Playing);
            });
        }

#pragma warning restore CS1998
    }
}