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
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task StopbotAsync()
        {;
            await ReplyAsync("The bot is shutting down.");
            Context.Client.LogoutAsync().GetAwaiter().GetResult();
            Context.Client.StopAsync().GetAwaiter().GetResult();
            Context.Client.Dispose();
        }


        [Command("setbotusername")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetBotUserName()
        {
            var guild = Context.Client.GetGuild(Context.Guild.Id);

            var user = guild.GetUser(Context.Client.CurrentUser.Id);

            await user.ModifyAsync(e => e.Nickname = "Arch Lector Frederick of Timebot", null);

            await Context.Client.SetStatusAsync(UserStatus.Online);

            await Context.Client.SetGameAsync("World Domination", null, StreamType.NotStreaming);

            await ReplyAsync("Username changed");
        }

        private async Task SendPMAsync(string message, SocketUser user)
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
            rtn_message.Add("tb!changedefaults #: Change the default speaking time");
            rtn_message.Add("tb!setbotusername: Initializes the bot's nickname and state");
            rtn_message.Add("tb!stopbot: PERMANENTLY STOPS THE BOT. Only Pelax should use this.");
            rtn_message.Add("tb!starttimer @mention: start a timer for a specific person");
            rtn_message.Add("tb!listfaction: List the factions available to be added to");
            rtn_message.Add("tb!addfaction \"Faction Name with Spaces\": adds a speaker to the faction");
            rtn_message.Add("tb!playbingo: starts a game of bingo, hosted by the bot.");
            rtn_message.Add("tb!clearspeakers: clears the observers and speakers from having those specific roles");
            rtn_message.Add("tb!clearchannel: clears all messages from the current channel");
            rtn_message.Add("tb!removefaction: removes a user from a faction");
            rtn_message.Add("tb!addrepresentative \"HOUSE NAME WITH SPACES\": adds you as the representative for your faction");
            rtn_message.Add("tb!removerepresentative \"HOUSE NAME WITH SPACES\": removes you as the representative for your faction");
            rtn_message.Add("```");

            await ReplyAsync(String.Join(System.Environment.NewLine,rtn_message));
        }

        [Command("addspeaker")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddspeakerAsync(IGuildUser user)
        {
            Data.speaker spkr = Data.GuilduserToSpeaker(user);
            Data.insert_speaker(spkr);

            await ReplyAsync("User been added as a speaker");
        }

        [Command("changedefaults")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ChangedefaultsAsync(int minutes)
        {
            Data.reset_speaking_time(minutes);

            await ReplyAsync("Speaking times have been reset");
        }

        [Command("starttimer")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
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

        [Command("clearspeakers")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearspeakersAsync()
        {
            List<ulong> roles = Context.Guild.Roles.Where(e=>e.Name=="Speaker" || e.Name=="Observer").Select(e=>e.Id).ToList();

            List<SocketGuildUser> users = Context.Guild.Users.ToList();

            foreach(SocketGuildUser usr in users)
            {
                if(roles.Any(usr.Roles.Select(e=>e.Id).Contains))
                {
                    foreach(ulong role in roles)
                    {
                       await usr.RemoveRoleAsync(Context.Guild.GetRole(role));
                    }
                }
            }

            await ReplyAsync("Tags removed");
        }

        [Command("clearchannel")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task ClearchannelAsync()
        {
            List<SocketRole> roles = Context.Guild.Roles.ToList();

            SocketGuildUser user = (SocketGuildUser)Context.User;

            if (!(user.Roles.Select(e=>e.Name).ToList().Contains("Representative")))
            {
                await ReplyAsync("You are not authorized to clear channels");
                return;
            }

            List<SocketMessage> old = Context.Channel.GetCachedMessages().ToList();

            await Context.Channel.DeleteMessagesAsync(old);

            var current = await Context.Channel.GetMessagesAsync().Flatten();

            await Context.Channel.DeleteMessagesAsync(current);

            
        }

        [Command("cactus")]
        public async Task CactusAsync()
        {
            GuildEmote cactus = Context.Guild.Emotes.Where(e=>e.Name == "cactusemo").First();

            await Context.Message.AddReactionAsync(cactus);

            await ReplyAsync("<:cactusemo:470328501283848197> <:cactusemo:470328501283848197> <:cactusemo:470328501283848197> <:cactusemo:470328501283848197> <:cactusemo:470328501283848197> <:cactusemo:470328501283848197>");
            return;
        }

    }

}