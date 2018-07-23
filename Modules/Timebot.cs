using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using timebot.Classes;

namespace timebot.Modules.Commands {

    public class commands : ModuleBase<SocketCommandContext> {
        private Tuple<string, string>[] Factions = new Tuple<string, string>[] {
            Tuple.Create ("14 Red Dogs Triad", "#AD4641"),
            Tuple.Create ("ACRE", "#915A2D"),
            Tuple.Create ("Church of Humanity Repentant", "#227F97"),
            Tuple.Create ("High Church of the Messiah-as-Emperox", "#F1C40F"),
            Tuple.Create ("House Aquila", "#C2A77A"),
            Tuple.Create ("House Crux", "#7851A9"),
            Tuple.Create ("House Eridanus", "#070000"),
            Tuple.Create ("House Fornax", "#C27C0E"),
            Tuple.Create ("House Lyra", "#853C67"),
            Tuple.Create ("House Pyxis", "#E3A041"),
            Tuple.Create ("House Reticulum", "#B00000"),
            Tuple.Create ("House Serpens", "#009115"),
            Tuple.Create ("House Triangulum", "#7DB6FF"),
            Tuple.Create ("House Vela", "#1B75BC"),
            Tuple.Create ("Houses Major", ""),
            Tuple.Create ("Houses Minor", ""),
            Tuple.Create ("PRISM", "#AB99B6"),
            Tuple.Create ("The Trilliant Ring", "#BBBBBB"),
            Tuple.Create ("The Deathless", "#8F5C5C"),
            Tuple.Create ("Unified People's Collective", "#89B951"),
            Tuple.Create ("\"House\" Vagrant", "#2F4CCA")
        };

        [Command ("stopbot")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task StopbotAsync () {;
            await ReplyAsync ("The bot is shutting down.");
            Context.Client.LogoutAsync ().GetAwaiter ().GetResult ();
            Context.Client.StopAsync ().GetAwaiter ().GetResult ();
            Context.Client.Dispose ();
        }

        [Command ("setbotusername")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task SetBotUserName () {
            var guild = Context.Client.GetGuild (Context.Guild.Id);

            var user = guild.GetUser (Context.Client.CurrentUser.Id);

            await user.ModifyAsync (e => e.Nickname = "Arch Lector Frederick of Timebot", null);

            await Context.Client.SetStatusAsync (UserStatus.Online);

            await Context.Client.SetGameAsync ("World Domination", null, StreamType.NotStreaming);

            await ReplyAsync ("Username changed");
        }

        private async Task SendPMAsync (string message, SocketUser user) {
            await user.SendMessageAsync (message);
        }

        [Command ("ping")]
        public async Task PingAsync () {
            await ReplyAsync ("Pong!");
        }

        [Command ("commands")]
        public async Task CommandsAsync () {

            List<string> rtn_message = new List<string> ();

            rtn_message.Add ("```");
            rtn_message.Add ("Here are the commands available");
            rtn_message.Add ("tb!ping : Make sure the bot is alive");
            rtn_message.Add ("tb!commands: you're using it right now!");
            rtn_message.Add ("tb!changedefaults #: Change the default speaking time");
            rtn_message.Add ("tb!setbotusername: Initializes the bot's nickname and state");
            rtn_message.Add ("tb!stopbot: PERMANENTLY STOPS THE BOT. Only Pelax should use this.");
            rtn_message.Add ("tb!starttimer @mention: start a timer for a specific person");
            rtn_message.Add ("tb!listfaction: List the factions available to be added to");
            rtn_message.Add ("tb!addfaction \"Faction Name with Spaces\": adds a speaker to the faction");
            rtn_message.Add ("tb!playbingo: starts a game of bingo, hosted by the bot.");
            rtn_message.Add ("tb!clearspeakers: clears the observers and speakers from having those specific roles");
            rtn_message.Add ("tb!clearchannel: clears all messages from the current channel");
            rtn_message.Add ("tb!removefaction: removes a user from a faction");
            rtn_message.Add ("tb!addrepresentative \"HOUSE NAME WITH SPACES\": adds you as the representative for your faction");
            rtn_message.Add ("tb!removerepresentative \"HOUSE NAME WITH SPACES\": removes you as the representative for your faction");
            rtn_message.Add ("tb!vote \"Faction name with spaces\" question# selection#: casts your factions vote for a specified option of a specified question");
            rtn_message.Add ("tb!tally #: Tallies the votes cast for a specified question.");
            rtn_message.Add ("tb!deletequestion #: Deletes the votes for the question specified.");
            rtn_message.Add ("```");

            await ReplyAsync (String.Join (System.Environment.NewLine, rtn_message));
        }

        [Command ("addspeaker")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task AddspeakerAsync (IGuildUser user) {
            Data.speaker spkr = Data.GuilduserToSpeaker (user);
            Data.insert_speaker (spkr);

            await ReplyAsync ("User been added as a speaker");
        }

        [Command ("changedefaults")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task ChangedefaultsAsync (int minutes) {
            Data.reset_speaking_time (minutes);

            await ReplyAsync ("Speaking times have been reset");
        }

        [Command ("starttimer")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task StarttimerAsync (IGuildUser user) {

            Data.speaker spkr = new Data.speaker ();

            if (!Data.is_speaker (user)) {
                await AddspeakerAsync (user);

                spkr = Data.get_speakers ().Where (s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator).First ();

                spkr.start_time = DateTime.Now;

                string msg = String.Concat ("You are now the speaker. You have ", spkr.speaking_time_minutes, " minutes remaining");

                await user.SendMessageAsync (msg);

                timer tmr = new timer ();

                tmr.user = user;

                tmr.StartTimer (spkr.speaking_time_minutes * 60 * 1000);
            } else {
                spkr = Data.get_speakers ().Where (s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator).First ();

                spkr.start_time = DateTime.Now;

                string msg = String.Concat ("You are now the speaker. You have ", spkr.speaking_time_minutes, " minutes remaining");

                await user.SendMessageAsync (msg);

                timer tmr = new timer ();

                tmr.user = user;

                tmr.StartTimer (spkr.speaking_time_minutes * 60 * 1000);
            }
        }

        [Command ("clearspeakers")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task ClearspeakersAsync () {
            List<ulong> roles = Context.Guild.Roles.Where (e => e.Name == "Speaker" || e.Name == "Observer" || e.Name == "Moderator").Select (e => e.Id).ToList ();

            List<SocketGuildUser> users = Context.Guild.Users.ToList ();

            foreach (SocketGuildUser usr in users) {
                if (roles.Any (usr.Roles.Select (e => e.Id).Contains)) {
                    foreach (ulong role in roles) {
                        await usr.RemoveRoleAsync (Context.Guild.GetRole (role));
                    }
                }
            }

            await ReplyAsync ("Tags removed");
        }

        [Command ("clearchannel")]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task ClearchannelAsync () {
            List<ulong> roles = Context.Guild.Roles.Where (e => e.Name == "Representative" || e.Name == "Moderator").Select (e => e.Id).ToList ();

            SocketGuildUser user = (SocketGuildUser) Context.User;

            if (roles.Any (user.Roles.Select (e => e.Id).Contains)) {
                List<SocketMessage> old = Context.Channel.GetCachedMessages ().ToList ();

                await Context.Channel.DeleteMessagesAsync (old);

                var current = await Context.Channel.GetMessagesAsync ().Flatten ();

                await Context.Channel.DeleteMessagesAsync (current);
            }

        }

        [Command ("cactus")]
        public async Task CactusAsync () {
            GuildEmote cactus = Context.Guild.Emotes.Where (e => e.Name == "cactusemo").First ();

            await Context.Message.AddReactionAsync (cactus);

            string result = new StringBuilder ().Insert (0, String.Concat ("<:cactusemo:", cactus.Id.ToString (), "> "), 10).ToString ();

            await ReplyAsync (result);

            return;
        }

        [Command ("vote")]
        public async Task vote (string faction, int question, int selection) {
            vote vote = new vote ();

            vote.name = Context.User.Username;
            vote.discriminator = Convert.ToUInt64 (Context.User.Discriminator);
            vote.selection = selection;
            vote.vote_id = question;
            vote.faction_name = faction;
            vote.faction_id = Context.Guild.Roles.Where (e => e.Name == faction).Select (e => e.Id).FirstOrDefault ();

            factionvoting voter = new factionvoting ();
            await voter.add_vote (vote);

            await ReplyAsync ("Vote cast");
        }

        [Command ("tally")]
        public async Task tally (int question_id) {
            List<string> results = new List<string> ();
            results.Add ("```");
            factionvoting voting = new factionvoting ();
            List<vote> votes = voting.return_tally (question_id);
            List<int> options = votes.Select (e => e.selection).Distinct ().ToList ();

            foreach (int option in options) {
                results.Add ("The tally for option " + option.ToString () + "is: " + votes.Where (e => e.selection == option).ToList ().Count ().ToString ());
            }
            results.Add ("```");

            string rtn = string.Join (System.Environment.NewLine, results);

            await ReplyAsync (rtn);
        }

        [Command ("deletequestion")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task DeletequestionAsync (int question_id) {
            factionvoting voting = new factionvoting ();

            await voting.delete_question (question_id);

            await ReplyAsync ("Votes for the question have been removed");
        }

        [Command ("setcolors")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task SetcolorsAsync () {

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            System.Drawing.ColorConverter converter = new System.Drawing.ColorConverter ();

            foreach (Tuple<string, string> Faction in Factions) {
                if (roles.Select (e => e.Name).Contains (Faction.Item1)) {
                    System.Drawing.Color colorhex = (System.Drawing.Color) converter.ConvertFromString (Faction.Item2);

                    await roles.Where (e => e.Name == Faction.Item1).FirstOrDefault ().ModifyAsync (r => r.Color = new Discord.Color (colorhex.R, colorhex.G, colorhex.B)).ConfigureAwait (false);
                }

            }

            await ReplyAsync ("Faction colors normalized.");
        }

    }

}