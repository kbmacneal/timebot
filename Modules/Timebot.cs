using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Threading;
using timebot.Classes;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace timebot.Modules.Commands
{

    public class commands : ModuleBase<SocketCommandContext>
    {
        [Command("stopbot")]
        public async Task StopbotAsync()
        {
            var bot_chan = Context.Guild.Channels.Where(e=>e.Name.StartsWith("bot"));
            await ReplyAsync("The bot is shutting down.");
            await Context.Client.LogoutAsync();
            await Context.Client.StopAsync();
            Context.Client.Dispose();
        }


        [Command("setbotusername")]
        public async Task SetBotUserName()
        {
            var guild = Context.Client.GetGuild(Context.Guild.Id);

            var user = guild.GetUser(Context.Client.CurrentUser.Id);

            await user.ModifyAsync(e => e.Nickname = "Arch Lector Frederick of Timebot", null);

            await Context.Client.SetStatusAsync(UserStatus.Online);

            await Context.Client.SetGameAsync("World Domination", null, StreamType.NotStreaming);

            await ReplyAsync("Username changed");
        }

        public async Task SendPMAsync(string message, SocketUser user)
        {
            await user.SendMessageAsync(message);
        }

        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!");
        }

        [Command("commands")]
        public async Task CommandsAsync()
        {

            await ReplyAsync(String.Concat("```Here are the commands available" + System.Environment.NewLine +
                "tb!ping : Make sure the bot is alive" + System.Environment.NewLine +
                "tb!commands: you're using it right now!" + System.Environment.NewLine +
                "tb!addadmin @mention: adds a user as a bot admin" + System.Environment.NewLine +
                "tb!changedefaults#: Change the default speaking time" + System.Environment.NewLine +
                "tb!setbotusername: Initializes the bot's nickname and state" + System.Environment.NewLine +
                "tb!stopbot: PERMANENTLY STOPS THE BOT. Only Pelax should use this." + System.Environment.NewLine +
                "tb!starttimer @mention: start a timer for a specific person" + System.Environment.NewLine +
                "tb!addspeaker @mention: adds a speaker to the list" + System.Environment.NewLine + "```"));
        }

        [Command("addadmin")]
        public async Task AddadminAsync(IGuildUser user)
        {
            Data.user usr = new Data.user();

            List<Data.user> users = Data.get_users();

            if (users.Where(e => e.Name == user.Username && e.Discriminator == user.Discriminator).Count() > 0)
            {
                Data.set_user_as_admin(user);
            }
            else
            {
                Data.Adduser(user, true);
            }

            await ReplyAsync("User is now admin");

        }

        [Command("addspeaker")]
        public async Task AddspeakerAsync(IGuildUser user)
        {
            Data.speaker spkr = Data.GuilduserToSpeaker(user);
            Data.insert_speaker(spkr);

            await ReplyAsync("User been added as a speaker");
        }

        [Command("changedefaults")]
        public async Task ChangedefaultsAsync(int minutes)
        {
            Data.reset_speaking_time(minutes);

            await ReplyAsync("Speaking times have been reset");
        }

        [Command("starttimer")]
        public async Task StarttimerAsync(IGuildUser user)
        {

            Data.speaker spkr = new Data.speaker();

            if (!Data.is_speaker(user))
            {
                await AddspeakerAsync(user);

                spkr = Data.get_speakers().Where(s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator).First();

                spkr.start_time = DateTime.Now;

                string msg = String.Concat("You are now the speaker. You have ", spkr.speaking_time_minutes, " minutes remaining");

                await user.SendMessageAsync(msg);

                timer tmr = new timer();

                tmr.user = user;

                tmr.StartTimer(spkr.speaking_time_minutes * 60 * 1000);
            }
            else
            {
                spkr = Data.get_speakers().Where(s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator).First();

                spkr.start_time = DateTime.Now;

                string msg = String.Concat("You are now the speaker. You have ", spkr.speaking_time_minutes, " minutes remaining");

                await user.SendMessageAsync(msg);

                timer tmr = new timer();

                tmr.user = user;

                tmr.StartTimer(spkr.speaking_time_minutes * 60 * 1000);
            }
        }

    }

}