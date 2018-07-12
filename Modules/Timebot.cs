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
        {;
            await ReplyAsync("The bot is shutting down.");
            Context.Client.LogoutAsync().GetAwaiter().GetResult();
            Context.Client.StopAsync().GetAwaiter().GetResult();
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

            List<string> rtn_message = new List<string>();

            rtn_message.Add("```");
            rtn_message.Add("Here are the commands available");
            rtn_message.Add("tb!ping : Make sure the bot is alive");
            rtn_message.Add("tb!commands: you're using it right now!");
            rtn_message.Add("tb!addadmin @mention: adds a user as a bot admin");
            rtn_message.Add("tb!changedefaults#: Change the default speaking time");
            rtn_message.Add("tb!setbotusername: Initializes the bot's nickname and state");
            rtn_message.Add("tb!stopbot: PERMANENTLY STOPS THE BOT. Only Pelax should use this.");
            rtn_message.Add("tb!starttimer @mention: start a timer for a specific person");
            rtn_message.Add("tb!addspeaker @mention: adds a speaker to the list");
            rtn_message.Add("tb!listfaction: List the factions available to be added to");
            rtn_message.Add("tb!addfaction \"Faction Name with Spaces\": adds a speaker to the faction");
            rtn_message.Add("```");

            await ReplyAsync(String.Join(System.Environment.NewLine,rtn_message));
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