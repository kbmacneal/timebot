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
            rtn_message.Add ("tb!listfaction: List the factions available to be added to");
            rtn_message.Add ("tb!addfaction \"Faction Name with Spaces\": adds a user to the faction");
            rtn_message.Add ("tb!playbingo: starts a game of bingo, hosted by the bot.");
            rtn_message.Add ("tb!removefaction: removes a user from a faction");
            rtn_message.Add ("tb!addrepresentative \"HOUSE NAME WITH SPACES\": adds you as the representative for your faction");
            rtn_message.Add ("tb!removerepresentative \"HOUSE NAME WITH SPACES\": removes you as the representative for your faction");
            rtn_message.Add ("tb!vote \"Faction name with spaces\" question# selection#: casts your factions vote for a specified option of a specified question");
            rtn_message.Add ("tb!tally #: Tallies the votes cast for a specified question.");
            rtn_message.Add ("tb!deletequestion #: Deletes the votes for the question specified.");
            rtn_message.Add ("tb!setcolors: normalizes the server's faction colors.");
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
            List<ulong> roles = Context.Guild.Roles.Where (e => e.Name == "Speaker" || e.Name == "Observer").Select (e => e.Id).ToList ();

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
            List<ulong> roles = Context.Guild.Roles.Where (e => e.Name == "Representative" || e.Name == "Moderator" || e.Name == "admin").Select (e => e.Id).ToList ();

            SocketGuildUser user = (SocketGuildUser) Context.User;

            if (roles.Any (user.Roles.Select (e => e.Id).Contains)) {
                var messages = await this.Context.Channel.GetMessagesAsync (int.MaxValue).Flatten ();
                await this.Context.Channel.DeleteMessagesAsync (messages);
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

            if(Context.Guild.Roles.Where (e => e.Name == faction).FirstOrDefault () == null)
            {
                await ReplyAsync("Faction invalid.");
                return;
            }

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
                results.Add("The factions who voted for this option are:");
                results.Add(String.Join(", ",votes.Select(e=>e.faction_name)));
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

    }

}