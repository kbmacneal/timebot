using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using timebot.Classes;
using timebot.Classes.Utilities;
using RestSharp;

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

            rtn_message.Add ("For complete instructions, go to https://highchurch.space/Bot");

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

                spkr = Data.get_speakers ().First (s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator);

                spkr.start_time = DateTime.Now;

                string msg = String.Concat ("You are now the speaker. You have ", spkr.speaking_time_minutes, " minutes remaining");

                await user.SendMessageAsync (msg);

                timer tmr = new timer ();

                tmr.user = user;

                tmr.StartTimer (spkr.speaking_time_minutes * 60 * 1000);
            } else {
                spkr = Data.get_speakers ().First (s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator);

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
            List<ulong> roles = Context.Guild.Roles.Where (e => e.Name == "Speaker" || e.Name == "Observer" || e.Name == "NACHO").Select (e => e.Id).ToList ();

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
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task ClearchannelAsync (int count) {
            var messages = await this.Context.Channel.GetMessagesAsync (count).Flatten ();

            RequestOptions opt = new RequestOptions ();

            opt.RetryMode = RetryMode.RetryRatelimit;

            foreach (var message in messages) {
                await message.DeleteAsync (opt);

                System.Threading.Thread.Sleep (2500);
            }

            // await this.Context.Channel.DeleteMessagesAsync(messages);

        }

        [Command ("clearchannel")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task ClearchannelAsync () {
            var messages = await this.Context.Channel.GetMessagesAsync (Int32.MaxValue).Flatten ();

            await Context.Channel.DeleteMessagesAsync (messages);

            // await this.Context.Channel.DeleteMessagesAsync(messages);

        }

        [Command ("cactus")]
        public async Task CactusAsync () {
            GuildEmote cactus = Context.Guild.Emotes.Where (e => e.Name == "cactusemo").First ();

            await Context.Message.AddReactionAsync (cactus);

            string result = new StringBuilder ().Insert (0, String.Concat ("<:cactusemo:", cactus.Id.ToString (), ">"), 11).ToString ();

            await ReplyAsync (result);

            return;
        }

        [Command ("virtues")]
        public async Task VirtuesAsync () {
            List<string> rtner = new List<string> ();

            rtner.Add ("```The First Virtue is Faith. Recitation: “Faith above all. We must trust God and their chosen Emperor to guide us.” Faith is exemplified by daily prayer, regular attendance of Church ceremonies, dutiful tithing, and pilgrimage to holy sites.```");
            rtner.Add ("```The Second Virtue is Propriety, which flows from Faith. Recitation: “We must be obedient to tradition, ceremony, courtesy and station.” Propriety is exemplified by respectful loyalty to righteous authority and cultural norms, along with unfailing intolerance for all heretics and heathens. Custom, dress, and technology all must adhere to Propriety.```");
            rtner.Add ("```The Third Virtue is Justice, which flows from Propriety. Recitation: “We must reward those who behave rightly and punish those who do not.” Justice is exemplified by its unflinching enforcement, and we must correct our own failings before looking to those of others.```");
            rtner.Add ("```The Fourth Virtue is Fortitude, which reinforces Justice. Recitation: “We must patiently endure the challenges laid upon us and follow the rightful path despite them.” Fortitude is exemplified by steadfast courage and endurance in the face of adversity, particularly in avoiding all cringing or complaint when upon holy ground or fulfilling holy duties.```");
            rtner.Add ("```The Fifth Virtue is Wisdom, which accompanies Fortitude. Recitation: “We must strive to see the world in its truth and shape it according to God’s will.” Wisdom is exemplified by perceived the flawed world as it is, but never losing sight of what it should be.  Daily reflection upon the sacred texts and their application to our lives is essential to Wisdom.```");
            await ReplyAsync (string.Join (System.Environment.NewLine, rtner), false, null, null);

            rtner = null;
            rtner = new List<string> ();

            rtner.Add ("```The Sixth Virtue is Temperance, which flows from Wisdom. Recitation: “We must show prudent moderation and diligent control over our desires.” Temperance is exemplified by self-restraint in all facets, particularly regular fasting and avoidance of intoxicants.```");
            rtner.Add ("```The Seventh Virtue is Diligence, which reinforces Temperance.  Recitation: “We must be ever persistent and expend all effort and attention in keeping ourselves and others to the true path.” Diligence is exemplified by constant, tireless vigilance against temptation and treachery in all aspects of life.```");
            rtner.Add ("```The Eighth Virtue is Charity, which echoes Justice. Recitation: “We must show compassion to those worthy of God’s mercy.” Charity is exemplified by philanthropic acts and outreach to faithful sufferers.```");
            rtner.Add ("```The Ninth Virtue is Integrity, which echoes Propriety. Recitation: “We must honor our oaths and uphold the truth.” Integrity is exemplified by unfailing honesty and the exposure of deviants, as well as regular confession of our failings.```");
            rtner.Add ("```The Tenth Virtue is Hope, which echoes Faith. Recitation: “We must never despair, no matter how dark the hour, as God shines their light upon us.”  Hope is exemplified by the conquest of despair, symbolized most prominently by the restriction of mourning to a designated period, as well as the teaching of the Virtues to the ignorant.```");

            await ReplyAsync (string.Join (System.Environment.NewLine, rtner), false, null, null);

            return;
        }

        [Command ("churchapproved")]
        public async Task ChurchapprovedAsync () {
            await Context.Channel.SendFileAsync ("Cactus_Pius_seal.png", "Officially Approved by the High Church", false, null);

            return;
        }

        [Command ("savior")]
        public async Task SaviorAsync () {
            await Context.Channel.SendFileAsync ("TINMTTOS.png", null, false, null);

            return;
        }

        [Command ("opensource")]
        public async Task OpensourceAsync () {
            await ReplyAsync ("This bot is open sourced and permissively licensed. You can find all source code in the github repo at https://github.com/kbmacneal/timebot");

            return;
        }

        [Command ("vote")]
        public async Task vote (string faction, int question, int selection) {

            vote vote = new vote ();

            if (Context.Guild.Roles.Where (e => e.Name == faction).FirstOrDefault () == null) {
                await ReplyAsync ("Faction invalid.");
                return;
            }

            vote.name = Context.User.Username;
            vote.discriminator = Convert.ToUInt64 (Context.User.Discriminator);
            vote.selection = selection;
            vote.vote_id = question;
            vote.faction_name = faction;
            vote.faction_id = Context.Guild.Roles.Where (e => e.Name == faction).Select (e => e.Id).FirstOrDefault ();

            Boolean can_vote = validate_vote (Context.User, vote);
            if (!can_vote) {
                await ReplyAsync ("User not authorized to cast a vote for this faction.");
                return;
            }

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
                results.Add ("The tally for option " + option.ToString () + " is: " + votes.Where (e => e.selection == option).ToList ().Count ().ToString ());
                results.Add ("The factions who voted for this option are:");
                results.Add (String.Join (", ", votes.Select (e => e.faction_name)));
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

        [Command ("synth")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task SynthAsync (SocketUser user) {
            SocketGuildUser usr = Context.Guild.GetUser (user.Id);

            string name = usr.Nickname.ToString () == "" ? usr.Username : usr.Nickname;

            int synth_id = Program.rand.Next (0, Int32.MaxValue - 5);

            string new_nick = "A Synth, ID " + synth_id.ToString ();

            await usr.ModifyAsync (e => e.Nickname = new_nick);

            await ReplyAsync ("I knew it! " + name + " is a synth!");
        }

        [Command ("tag")]
        public async Task TagAsync (params string[] collection) {
            string tag_name = String.Join (" ", collection);

            TextInfo UsaTextInfo = new CultureInfo ("en - US", false).TextInfo;
            tag_name = UsaTextInfo.ToTitleCase (tag_name);

            if (Program.tags.FirstOrDefault (e => e.Name == tag_name) == null) {
                await ReplyAsync ("Invalid tag selection.");
                return;
            }

            Embed emb = Helper.ObjToEmbed (Program.tags.FirstOrDefault (e => e.Name == tag_name), "Name");

            await ReplyAsync ("", false, emb, null);
        }

        [Command ("asset")]
        public async Task AssetAsync (params string[] collection) {
            string asset_name = String.Join (" ", collection);

            TextInfo UsaTextInfo = new CultureInfo ("en - US", false).TextInfo;
            asset_name = UsaTextInfo.ToTitleCase (asset_name);

            if (Program.assets.FirstOrDefault (e => e.Name == asset_name) == null) {
                await ReplyAsync ("Invalid asset selection.");
                return;
            }

            Embed emb = Helper.ObjToEmbed (Program.assets.FirstOrDefault (e => e.Name == asset_name), "Name");

            await ReplyAsync ("", false, emb, null);
        }

        [Command ("badbot")]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task BadbotAsync () {
            Dictionary<int, string> results = new Dictionary<int, string> ();

            results.Add (1, "BANNED!");
            results.Add (2, "You come over here and say that to my face.");
            results.Add (3, "...");
            results.Add (4, "Eh? What?");
            results.Add (5, "Who said that?");
            results.Add (6, "```The details of my life are quite inconsequential... very well, where do I begin? My father was a relentlessly self-improving boulangerie owner from Belgium with low grade narcolepsy and a penchant for buggery. My mother was a fifteen year old French prostitute named Chloe with webbed feet. My father would womanize, he would drink. He would make outrageous claims like he invented the question mark. Sometimes he would accuse chestnuts of being lazy. The sort of general malaise that only the genius possess and the insane lament. My childhood was typical. Summers in Rangoon, luge lessons. In the spring we'd make meat helmets. When I was insolent I was placed in a burlap bag and beaten with reeds- pretty standard really. At the age of twelve I received my first scribe. At the age of fourteen a Zoroastrian named Vilma ritualistically shaved my testicles. There really is nothing like a shorn scrotum... it's breathtaking- I highly suggest you try it.```");

            Random rnd = new Random ();

            int selection = rnd.Next (0, results.Keys.Max ()) + 1;

            await ReplyAsync (results.Where (e => e.Key == selection).First ().Value);
        }

        public int singleRoll (String roll) {
            int num_dice = 1;
            int result = 0;
            int di = roll.IndexOf ('d');
            if (di == -1) return Int32.Parse (roll);
            int diceSize = Int32.Parse (roll.Substring (di + 1)); //value of string after 'd'
            if (di != 0) num_dice = Int32.Parse (roll.Substring (0, di));

            for (int i = 0; i < num_dice; i++) {
                result += Program.rand.Next (0, diceSize) + 1;
            }
            return result;
        }

        [Command ("roll")]
        public async Task RollAsync (params string[] args) {
            string roll = string.Join ("", args).Replace (" ", "");
            List<int> dice_results = new List<int> ();

            DiceBag db = new DiceBag ();

            if (roll.Contains ('-') || roll.Contains ('+')) {
                char[] splits = new char[] { '+', '-' };
                string[] two_parts = roll.Split (splits);
                int mod = 0;
                if (roll.Contains ('-')) {
                    mod = -1 * Convert.ToInt32 (two_parts[1]);
                } else {
                    mod = Convert.ToInt32 (two_parts[1]);
                }
                string[] parameters = two_parts[0].Split ('d');
                uint num_dice = 1;
                int dice_sides = 2;

                if (parameters[0] != "") {
                    num_dice = Convert.ToUInt32 (parameters[0]);
                    dice_sides = Convert.ToInt32 (parameters[1]);
                } else {
                    dice_sides = Convert.ToInt32 (parameters[1]);
                }

                DiceBag.Dice dice = (DiceBag.Dice) System.Enum.Parse (typeof (DiceBag.Dice), dice_sides.ToString ());

                dice_results = db.RollQuantity (dice, num_dice);

                dice_results.Add (mod);
            } else {
                string[] parameters = roll.Split ('d');
                uint num_dice = 1;
                int dice_sides = 2;

                if (parameters[0] != "") {
                    num_dice = Convert.ToUInt32 (parameters[0]);
                    dice_sides = Convert.ToInt32 (parameters[1]);
                } else {
                    dice_sides = Convert.ToInt32 (parameters[1]);
                }

                DiceBag.Dice dice = (DiceBag.Dice) System.Enum.Parse (typeof (DiceBag.Dice), dice_sides.ToString ());

                dice_results = db.RollQuantity (dice, num_dice);

            }

            SocketGuildUser usr = Context.Guild.GetUser (Context.Message.Author.Id) as SocketGuildUser;

            string rtn_name = usr.Nickname == null ? usr.Username : usr.Nickname;

            await ReplyAsync (rtn_name + " rolled a " + dice_results.Sum () + " (" + string.Join (", ", dice_results) + ")");
        }

        [Command ("sector")]
        private async Task SectorAsync () {
            await ReplyAsync ("https://sectorswithoutnumber.com/sector/m11ZXBOt6xiJGo21EKio");
        }

        [Command("trilljoke")]
        private async Task TrilljokeAsync()
        {
            string msg = "What do you call a group of Trills?" + System.Environment.NewLine + "A Party" + System.Environment.NewLine + "👈😎👉";

            await ReplyAsync(msg,false,null,null);
        }

        [Command("xkcd")]
        private async Task XkcdAsync()
        {
            int comic_number = Program.rand.Next(0,(Program.latest_xkcd + 1));

            string baseurl = string.Concat ("https://xkcd.com/" + comic_number.ToString() + "/info.0.json");

            var client = new RestClient (baseurl);

            var request = new RestRequest (Method.GET);

            var response = client.Execute (request);

            if (!response.IsSuccessful) return;

            var content = JsonConvert.DeserializeObject<Classes.Xkcd.Comic>(response.Content);

            await ReplyAsync(content.Alt,false,null,null);

            await ReplyAsync(content.Img.ToString(),false,null,null);
        }

        [Command("xkcd")]
        private async Task XkcdAsync(int comic_number)
        {

            string baseurl = string.Concat ("https://xkcd.com/" + comic_number.ToString() + "/info.0.json");

            var client = new RestClient (baseurl);

            var request = new RestRequest (Method.GET);

            var response = client.Execute (request);

            if (!response.IsSuccessful) return;

            var content = JsonConvert.DeserializeObject<Classes.Xkcd.Comic>(response.Content);

            await ReplyAsync(content.Img.ToString(),false,null,null);
        }

        private Boolean validate_vote (SocketUser user, vote vote) {
            Nacho nacho = new Nacho ();

            if (nacho.get_rep (user.Username, Convert.ToUInt64 (user.Discriminator)).FirstOrDefault () == null) {
                return false;
            }

            Nacho.representative rep = nacho.get_rep (user.Username, Convert.ToUInt64 (user.Discriminator)).FirstOrDefault ();

            if (rep.faction_text != vote.faction_name) {
                return false;
            }

            return true;

        }

    }

}