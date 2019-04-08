using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Flurl;
using Flurl.Http;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using RestSharp;
using timebot.Classes;
using timebot.Classes.Assets;
using timebot.Classes.Utilities;
using timebot.Contexts;

namespace timebot.Modules.Commands
{
    public class command_help
    {
        public string name { get; set; }
        public string summary { get; set; }
        public bool admin_required { get; set; }
    }

    public class commands : ModuleBase<SocketCommandContext>
    {
        private static string[] cols = {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z",
        };

        private async Task SendPMAsync (string message, SocketUser user)
        {
            await user.SendMessageAsync (message);
        }

        [Command ("ping")]
        [Summary ("Test whether or no the bot is awake.")]
        public async Task PingAsync ()
        {
            await ReplyAsync ("Pong!");
        }

        [Command ("commands")]
        [Summary ("Pastes a link to the help page in the chat.")]
        public async Task CommandsAsync ()
        {
            List<string> rtn_message = new List<string> ();

            rtn_message.Add ("For complete instructions, go to https://highchurch.space/Bot");

            await ReplyAsync (String.Join (System.Environment.NewLine, rtn_message));
        }

        [Command ("addspeaker")]
        [Summary ("Adds the user as a speaker for meeting purposes.")]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task AddspeakerAsync (IGuildUser user)
        {
            Data.speaker spkr = Data.GuilduserToSpeaker (user);
            Data.insert_speaker (spkr);

            await ReplyAsync ("User been added as a speaker");
        }

        [Command ("changedefaults")]
        [Summary ("Changes the default speaking time, if needed.")]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task ChangedefaultsAsync (int minutes)
        {
            Data.reset_speaking_time (minutes);

            await ReplyAsync ("Speaking times have been reset");
        }

        // [Command ("starttimer")]
        // [Summary ("Starts the timer against a user.")]
        // [RequireUserPermission (GuildPermission.Administrator)]
        // public async Task StarttimerAsync (IGuildUser user)
        // {
        //     Data.speaker spkr = new Data.speaker ();

        //     if (!Data.is_speaker (user))
        //     {
        //         await AddspeakerAsync (user);

        //         spkr = Data.get_speakers ().First (s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator);

        //         spkr.start_time = DateTime.Now;

        //         string msg = String.Concat ("You are now the speaker. You have ", spkr.speaking_time_minutes, " minutes remaining");

        //         await user.SendMessageAsync (msg);

        //         timer tmr = new timer ();

        //         tmr.user = user;

        //         tmr.StartTimer (spkr.speaking_time_minutes * 60 * 1000);
        //     }
        //     else
        //     {
        //         spkr = Data.get_speakers ().First (s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator);

        //         spkr.start_time = DateTime.Now;

        //         string msg = String.Concat ("You are now the speaker. You have ", spkr.speaking_time_minutes, " minutes remaining");

        //         await user.SendMessageAsync (msg);

        //         timer tmr = new timer ();

        //         tmr.user = user;

        //         tmr.StartTimer (spkr.speaking_time_minutes * 60 * 1000);
        //     }
        // }

        [Command ("clearspeakers")]
        [Summary ("Resets the speaker list.")]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task ClearspeakersAsync ()
        {
            List<ulong> roles = Context.Guild.Roles.Where (e => e.Name == "Speaker" || e.Name == "Observer" || e.Name == "NACHO").Select (e => e.Id).ToList ();

            List<SocketGuildUser> users = Context.Guild.Users.ToList ();

            foreach (SocketGuildUser usr in users)
            {
                if (roles.Any (usr.Roles.Select (e => e.Id).Contains))
                {
                    foreach (ulong role in roles)
                    {
                        await usr.RemoveRoleAsync (Context.Guild.GetRole (role));
                    }
                }
            }

            await ReplyAsync ("Tags removed");
        }

        [Command ("clearchannel")]
        [Summary ("Clears the channel of chat messages.")]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task ClearchannelAsync (int count)
        {
            var messages = (this.Context.Channel.GetMessagesAsync (count)).Flatten ().ToList ().GetAwaiter ().GetResult ();

            RequestOptions opt = new RequestOptions ();

            opt.RetryMode = RetryMode.RetryRatelimit;

            foreach (var message in messages)
            {
                await message.DeleteAsync (opt);

                System.Threading.Thread.Sleep (2500);
            }

            // await this.Context.Channel.DeleteMessagesAsync(messages);
        }

        [Command ("cactus")]
        [Summary ("You should use this wherever possible.")]
        public async Task CactusAsync ()
        {
            GuildEmote cactus = Context.Guild.Emotes.Where (e => e.Name == "cactusemo").First ();

            await Context.Message.AddReactionAsync (cactus);

            string result = new StringBuilder ().Insert (0, String.Concat ("<:cactusemo:", cactus.Id.ToString (), ">"), 11).ToString ();

            await ReplyAsync (result);

            return;
        }

        [Command ("virtues")]
        [Summary ("Ten Blessings yall.")]
        public async Task VirtuesAsync ()
        {
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
        [Summary ("The only way to express approval.")]
        public async Task ChurchapprovedAsync ()
        {
            await Context.Channel.SendFileAsync ("Cactus_Pius_seal.png", "Officially Approved by the High Church", false, null);

            return;
        }

        [Command ("savior")]
        [Summary ("Answers that age-old question.")]
        public async Task SaviorAsync ()
        {
            await Context.Channel.SendFileAsync ("TINMTTOS.png", null, false, null);

            return;
        }

        [Command ("opensource")]
        [Summary ("Pastes a link to the github repo in the chat.")]
        public async Task OpensourceAsync ()
        {
            await ReplyAsync ("This bot is open sourced and permissively licensed. You can find all source code in the github repo at https://github.com/kbmacneal/timebot");

            return;
        }

        // [Command ("vote")]
        // [Summary ("Handles multifation voting.")]
        // public async Task vote (string faction, int question, int selection)
        // {
        //     vote vote = new vote ();

        //     if (Context.Guild.Roles.Where (e => e.Name == faction).FirstOrDefault () == null)
        //     {
        //         await ReplyAsync ("Faction invalid.");
        //         return;
        //     }

        //     vote.name = Context.User.Username;
        //     vote.discriminator = Convert.ToUInt64 (Context.User.Discriminator);
        //     vote.selection = selection;
        //     vote.vote_id = question;
        //     vote.faction_name = faction;
        //     vote.faction_id = Context.Guild.Roles.Where (e => e.Name == faction).Select (e => e.Id).FirstOrDefault ();

        //     Boolean can_vote = validate_vote (Context.User, vote);
        //     if (!can_vote)
        //     {
        //         await ReplyAsync ("User not authorized to cast a vote for this faction.");
        //         return;
        //     }

        //     factionvoting voter = new factionvoting ();
        //     await voter.add_vote (vote);

        //     await ReplyAsync ("Vote cast");
        // }

        // [Command ("tally")]
        // [Summary ("Tallies the votes.")]
        // public async Task tally (int question_id)
        // {
        //     List<string> results = new List<string> ();
        //     results.Add ("```");
        //     factionvoting voting = new factionvoting ();
        //     List<vote> votes = voting.return_tally (question_id);
        //     List<int> options = votes.Select (e => e.selection).Distinct ().ToList ();

        //     foreach (int option in options)
        //     {
        //         results.Add ("The tally for option " + option.ToString () + " is: " + votes.Where (e => e.selection == option).ToList ().Count ().ToString ());
        //         results.Add ("The factions who voted for this option are:");
        //         results.Add (String.Join (", ", votes.Select (e => e.faction_name)));
        //     }
        //     results.Add ("```");

        //     string rtn = string.Join (System.Environment.NewLine, results);

        //     await ReplyAsync (rtn);
        // }

        // [Command ("deletequestion")]
        // [Summary ("Removes a question from the database.")]
        // [RequireUserPermission (GuildPermission.Administrator)]
        // public async Task DeletequestionAsync (int question_id)
        // {
        //     factionvoting voting = new factionvoting ();

        //     await voting.delete_question (question_id);

        //     await ReplyAsync ("Votes for the question have been removed");
        // }

        [Command ("synth")]
        [Summary ("Determines whether or not someone is a synth.")]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task SynthAsync (SocketUser user)
        {
            SocketGuildUser usr = Context.Guild.GetUser (user.Id);

            string name = usr.Nickname.ToString () == "" ? usr.Username : usr.Nickname;

            int synth_id = Program.rand.Next (0, Int32.MaxValue - 5);

            string new_nick = "A Synth, ID " + synth_id.ToString ();

            await usr.ModifyAsync (e => e.Nickname = new_nick);

            await ReplyAsync ("I knew it! " + name + " is a synth!");
        }

        [Command ("tag")]
        [Summary ("Searches for a tag and displays the summary in chat.")]
        public async Task TagAsync (params string[] collection)
        {
            string tag_name = String.Join (" ", collection);

            if (Classes.Tags.Tag.GetTags ().FirstOrDefault (e => e.Name.ToUpperInvariant () == tag_name.ToUpperInvariant ()) == null)
            {
                await ReplyAsync ("Invalid tag selection.");
                return;
            }

            var tag = Classes.Tags.Tag.GetTags ().FirstOrDefault (e => e.Name.ToUpperInvariant () == tag_name.ToUpperInvariant ());

            Embed emb = Helper.ObjToEmbed (tag, "Name");

            await ReplyAsync ("", false, emb, null);
        }

        [Command ("asset")]
        [Summary ("Search for an asset and display it in chat.")]
        public async Task AssetAsync (params string[] collection)
        {
            string asset_name = String.Join (" ", collection);

            TextInfo UsaTextInfo = new CultureInfo ("en", false).TextInfo;
            asset_name = UsaTextInfo.ToTitleCase (asset_name);

            if (Classes.Assets.Asset.GetAssets ().FirstOrDefault (e => e.Name == asset_name) == null)
            {
                await ReplyAsync ("Invalid asset selection.");
                return;
            }

            Embed emb = Helper.ObjToEmbed (Classes.Assets.Asset.GetAssets ().FirstOrDefault (e => e.Name == asset_name), "Name");

            await ReplyAsync ("", false, emb, null);
        }

        [Command ("badbot")]
        [Summary ("At this point, I'm pretty sure the bot likes it.")]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task BadbotAsync ()
        {
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

        public int singleRoll (String roll)
        {
            int num_dice = 1;
            int result = 0;
            int di = roll.IndexOf ('d');
            if (di == -1) return Int32.Parse (roll);
            int diceSize = Int32.Parse (roll.Substring (di + 1)); //value of string after 'd'
            if (di != 0) num_dice = Int32.Parse (roll.Substring (0, di));

            for (int i = 0; i < num_dice; i++)
            {
                result += Program.rand.Next (0, diceSize) + 1;
            }
            return result;
        }

        [Command ("roll")]
        [Summary ("Rolls some dice.")]
        public async Task RollAsync (params string[] args)
        {
            string roll = string.Join ("", args).Replace (" ", "");
            List<int> dice_results = new List<int> ();

            DiceBag db = new DiceBag ();

            if (roll.Contains ('-') || roll.Contains ('+'))
            {
                char[] splits = new char[] { '+', '-' };
                string[] two_parts = roll.Split (splits);
                int mod = 0;
                if (roll.Contains ('-'))
                {
                    mod = -1 * Convert.ToInt32 (two_parts[1]);
                }
                else
                {
                    mod = Convert.ToInt32 (two_parts[1]);
                }
                string[] parameters = two_parts[0].Split ('d');
                uint num_dice = 1;
                int dice_sides = 2;

                if (parameters[0] != "")
                {
                    num_dice = Convert.ToUInt32 (parameters[0]);
                    dice_sides = Convert.ToInt32 (parameters[1]);
                }
                else
                {
                    dice_sides = Convert.ToInt32 (parameters[1]);
                }

                DiceBag.Dice dice = (DiceBag.Dice) System.Enum.Parse (typeof (DiceBag.Dice), dice_sides.ToString ());

                dice_results = db.RollQuantity (dice, num_dice);

                dice_results.Add (mod);
            }
            else
            {
                string[] parameters = roll.Split ('d');
                uint num_dice = 1;
                int dice_sides = 2;

                if (parameters[0] != "")
                {
                    num_dice = Convert.ToUInt32 (parameters[0]);
                    dice_sides = Convert.ToInt32 (parameters[1]);
                }
                else
                {
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
        [Summary ("Pastes the link to the sectorswithoutnumber page in chat.")]
        private async Task SectorAsync ()
        {
            await ReplyAsync ("https://sectorswithoutnumber.com/sector/m11ZXBOt6xiJGo21EKio");
        }

        [Command ("trilljoke")]
        [Summary ("Nuff said.")]
        private async Task TrilljokeAsync ()
        {
            string msg = "What do you call a group of Trills?" + System.Environment.NewLine + "A Party" + System.Environment.NewLine + "👈😎👉";

            await ReplyAsync (msg, false, null, null);
        }

        [Command ("xkcd")]
        [Summary ("Random web comic anyone?")]
        private async Task XkcdAsync ()
        {
            int comic_number = Program.rand.Next (0, (Program.latest_xkcd + 1));

            // string baseurl = string.Concat ("https://xkcd.com/" + comic_number.ToString () + "/info.0.json");

            // var client = new RestClient (baseurl);

            // var request = new RestRequest (Method.GET);

            // var response = client.Execute (request);

            // if (!response.IsSuccessful) return;

            var response = await "https://xkcd.com/"
                .AppendPathSegment (comic_number)
                .AppendPathSegment ("info.0.json")
                .GetAsync ()
                .ReceiveString ();

            var content = JsonConvert.DeserializeObject<Classes.Xkcd.Comic> (response);

            await ReplyAsync (content.Alt, false, null, null);

            await ReplyAsync (content.Img.ToString (), false, null, null);
        }

        [Command ("xkcd")]
        [Summary ("Random web comic anyone?")]
        private async Task XkcdAsync (int comic_number)
        {
            // string baseurl = string.Concat ("https://xkcd.com/" + comic_number.ToString () + "/info.0.json");

            // var client = new RestClient (baseurl);

            // var request = new RestRequest (Method.GET);

            // var response = client.Execute (request);

            // if (!response.IsSuccessful) return;

            var response = await "https://xkcd.com/"
                .AppendPathSegment (comic_number)
                .AppendPathSegment ("info.0.json")
                .GetAsync ()
                .ReceiveString ();

            var content = JsonConvert.DeserializeObject<Classes.Xkcd.Comic> (response);

            await ReplyAsync (content.Alt, false, null, null);

            await ReplyAsync (content.Img.ToString (), false, null, null);
        }

        public class commands_json
        {
            public string api_key { get; set; }
            public string json_text { get; set; }
        }

        [Command ("dumpcommands")]
        [Summary ("Updates the command help page.")]
        [RequireUserPermission (GuildPermission.Administrator)]
        private async Task DumpcommandsAsync ()
        {
            List<command_help> commands = new List<command_help> ();

            foreach (var command in Program._commands.Commands.ToList ())
            {
                command_help c = new command_help
                {
                    name = command.Name,
                    summary = command.Summary,
                    admin_required = command.Preconditions.Count () > 0
                };

                commands.Add (c);
            }

            var output = JsonConvert.SerializeObject (commands.OrderBy (e => e.name).Distinct (), Formatting.Indented);

            Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>> (System.IO.File.ReadAllText (Program.secrets_file));

            string key = secrets["api_key"];

            string baseurl = string.Concat ("https://highchurch.space/api/update_commands");
            // string baseurl = string.Concat ("http://localhost:5000/api/update_commands");

            var client = new RestClient (baseurl);

            var s = new commands_json ();
            s.api_key = key;
            s.json_text = output;

            // var response = await "http://api.mathjs.org/v4/"
            //     .AppendPathSegment ("api")
            //     .AppendPathSegment ("update_commands")
            //     .WithHeader ("Content-Type", "text/json")
            //     .PostJsonAsync (JsonConvert.SerializeObject (s));

            var request = new RestRequest (Method.POST);
            request.AddParameter ("text/json", JsonConvert.SerializeObject (s), ParameterType.RequestBody);

            request.AddHeader ("Content-Type", "text/json");

            var response = client.Execute (request);

            await ReplyAsync ("Commands Updated");
        }

        [Command ("summary")]
        [Summary ("Lets you know what a command is. I'm sensing a recursion here.")]
        public async Task SummaryAsync (params string[] commands)
        {
            string rtn = "";

            var command = Program._commands.Commands.FirstOrDefault (e => e.Name == String.Join (" ", commands));

            if (command == null)
            {
                rtn = "Command doesn't exist.";
            }
            else
            {
                rtn = command.Summary;
            }

            await ReplyAsync (rtn);
        }

        [Command ("turn")]
        [Summary ("Gives you a particular turn for a particular faction, straight from the faction tracker. tb!turn <turn #> <Faction without quotation marks>")]
        public async Task TurnAsync (int turn, params string[] faction)
        {
            string faction_name = String.Join (" ", faction);
            string rtn = "";
            ServiceAccountCredential credential;
            string[] Scopes = { SheetsService.Scope.Spreadsheets };
            string serviceAccountEmail = "timebot@timebot.iam.gserviceaccount.com";
            string jsonfile = "Timebot-fc612c6f90aa.json";
            using (Stream stream = new FileStream (@jsonfile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                credential = (ServiceAccountCredential)
                GoogleCredential.FromStream (stream).UnderlyingCredential;

                var initializer = new ServiceAccountCredential.Initializer (credential.Id)
                {
                    User = serviceAccountEmail,
                    Key = credential.Key,
                    Scopes = Scopes
                };
                credential = new ServiceAccountCredential (initializer);
            }

            var service = new SheetsService (new BaseClientService.Initializer ()
            {
                HttpClientInitializer = credential,
                    ApplicationName = "timebot",
            });

            // Define request parameters.
            String spreadsheetId = "1QR078QvO5Q8S9gbQDglRhYK1HV3tBd0111SmjoVV0jQ";
            String range = "\'Faction Turns\'!A2:A14";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get (spreadsheetId, range);

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            ValueRange response = request.Execute ();
            IList<IList<Object>> values = response.Values;
            int row_index = -1;
            if (values != null && values.Count > 0)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (values[i][0].ToString () == faction_name)
                    {
                        row_index = i + 2;
                        break;
                    }
                }

                if (row_index == -1)
                {
                    throw new KeyNotFoundException ();
                }
            }
            else
            {
                throw new KeyNotFoundException ();
            }

            string col_name = cols[turn + 1];

            range = "\'Faction Turns\'!" + col_name + row_index;

            SpreadsheetsResource.ValuesResource.GetRequest cell_request =
                service.Spreadsheets.Values.Get (spreadsheetId, range);

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            ValueRange cell_resp = cell_request.Execute ();
            if (cell_resp.Values == null)
            {
                rtn = "I'm sorry Dave, I'm afraid I can't do that.";
                await ReplyAsync (rtn);
                return;
            }
            IList<IList<Object>> cell_val = cell_resp.Values;

            if (values == null || values.Count == 0)
            {
                throw new KeyNotFoundException ();
            }

            if (cell_val[0][0] == null)
            {
                rtn = "I'm sorry Dave, I'm afraid I can't do that.";
                await ReplyAsync (rtn);
                return;
            }
            else
            {
                rtn = cell_val[0][0].ToString ();
            }

            if (rtn.ToCharArray ().Length >= 2000)
            {
                Queue<string> sender = new Queue<string> ();
                Queue<char> rtn_q = new Queue<char> ();

                rtn.ToCharArray ().ToList ().ForEach (e => rtn_q.Enqueue (e));

                while (rtn_q.Count > 0)
                {
                    int count = rtn_q.Count < 2000 ? rtn_q.Count : 2000;

                    string part = String.Join ("", rtn_q.Take (count));

                    rtn_q.ToList ().RemoveRange (0, count);

                    sender.Enqueue (part);
                }

                foreach (string part in sender)
                {
                    await ReplyAsync (part);
                }
            }
            else
            {
                await ReplyAsync (rtn);
            }
        }

        [Command ("assets")]
        [Summary ("Dumps a list of faction assets and their locations into the chat.")]
        public async Task AssetsAsync (params string[] faction)
        {
            string faction_name = String.Join (" ", faction);

            List<string> rtn = new List<string> ();

            ServiceAccountCredential credential;

            string[] Scopes = { SheetsService.Scope.Spreadsheets };

            string serviceAccountEmail = "timebot@timebot.iam.gserviceaccount.com";

            string jsonfile = "Timebot-fc612c6f90aa.json";

            using (Stream stream = new FileStream (@jsonfile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                credential = (ServiceAccountCredential)
                GoogleCredential.FromStream (stream).UnderlyingCredential;

                var initializer = new ServiceAccountCredential.Initializer (credential.Id)
                {
                    User = serviceAccountEmail,
                    Key = credential.Key,
                    Scopes = Scopes
                };
                credential = new ServiceAccountCredential (initializer);
            }

            var service = new SheetsService (new BaseClientService.Initializer ()
            {
                HttpClientInitializer = credential,
                    ApplicationName = "timebot",
            });

            // Define request parameters.
            String spreadsheetId = "1QR078QvO5Q8S9gbQDglRhYK1HV3tBd0111SmjoVV0jQ";

            String range = "AssetTracker!A2:L";

            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get (spreadsheetId, range);

            ValueRange response = request.Execute ();

            List<Classes.Assets.TrackerAsset> assets = new List<Classes.Assets.TrackerAsset> ();

            IList<IList<Object>> values = response.Values;

            int row_index = -1;

            if (values != null && values.Count > 0)
            {
                for (int i = 0; i < values.Count - 1; i++)
                {
                    if (values[i][0].ToString () == faction_name)
                    {
                        Classes.Assets.TrackerAsset asset = new Classes.Assets.TrackerAsset ()
                        {
                        Owner = values[i][0].ToString (),
                        Asset = values[i][1].ToString (),
                        Stealthed = values[i][3].ToString (),
                        Stat = values[i][4].ToString (),
                        HP = values[i][5].ToString (),
                        MaxHP = values[i][6].ToString (),
                        CombinedHP = values[i][5].ToString () + "/" + values[i][6].ToString (),
                        Type = values[i][7].ToString (),
                        Attack = values[i][8].ToString (),
                        Counter = values[i][9].ToString (),
                        Notes = values[i][10].ToString (),
                        Location = values[i][11].ToString ().Split ("/") [2].ToString ()
                        };
                        row_index = i;

                        assets.Add (asset);
                    }

                    if (values[i][11].ToString ().Contains (faction_name))
                    {
                        Classes.Assets.TrackerAsset asset = new Classes.Assets.TrackerAsset ()
                        {
                            Owner = values[i][0].ToString (),
                            Asset = values[i][1].ToString (),
                            Stealthed = values[i][3].ToString (),
                            Stat = values[i][4].ToString (),
                            HP = values[i][5].ToString (),
                            MaxHP = values[i][6].ToString (),
                            CombinedHP = values[i][5].ToString () + "/" + values[i][6].ToString (),
                            Type = values[i][7].ToString (),
                            Attack = values[i][8].ToString (),
                            Counter = values[i][9].ToString (),
                            Notes = values[i][10].ToString (),
                            Location = values[i][11].ToString ().Split ("/") [2].ToString ()
                        };
                        row_index = i;

                        assets.Add (asset);
                    }
                }

                if (row_index == -1)
                {
                    throw new KeyNotFoundException ();
                }
            }
            else
            {
                throw new KeyNotFoundException ();
            }

            await ReplyAsync ("Assets for: " + faction_name);

            var header = new string[6] { "Owner", "Name", "HP", "Attack Dice", "Counter Dice", "Location" };

            var table = Classes.TableParser.ToStringTable (assets.Select (asset => new { asset.Owner, asset.Asset, asset.CombinedHP, asset.Attack, asset.Counter, asset.Location }).OrderBy (e => e.Owner).ThenBy (e => e.Location).ThenBy (e => e.Asset), header, a => a.Owner, a => a.Asset, a => a.CombinedHP, a => a.Attack, a => a.Counter, a => a.Location);

            Helper.SplitToLines (table, 1994).ForEach (e => ReplyAsync ("```" + e + "```").GetAwaiter ().GetResult ());

            // rtn.Add (table);

            // rtn.Add(Classes.TableParser.PrintRow(header));
            // rtn.Add(Classes.TableParser.PrintLine());
            // rtn.Add("Name | HP | Max HP | Attack Dice | Counter Dice | Location");

            // assets.ForEach(asset=>rtn.Add(Classes.TableParser.PrintRow(new string[6]{asset.Asset, asset.HP, asset.MaxHP, asset.Attack, asset.Counter, asset.Location})));

            // foreach (var asset in assets)
            // {
            //     var adder = string.Join (" | ", new List<string> () { asset.Asset, asset.HP, asset.MaxHP, asset.Attack, asset.Counter, asset.Location });l

            //     rtn.Add (adder);
            // }
        }

        [Command ("turnfactions")]
        [Summary ("Prints a list of the factions in the tracker if you arent sure what to type.")]
        public async Task TurnFactionsAsync ()
        {
            ServiceAccountCredential credential;
            string[] Scopes = { SheetsService.Scope.Spreadsheets };
            string serviceAccountEmail = "timebot@timebot.iam.gserviceaccount.com";
            string jsonfile = "Timebot-fc612c6f90aa.json";
            using (Stream stream = new FileStream (@jsonfile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                credential = (ServiceAccountCredential)
                GoogleCredential.FromStream (stream).UnderlyingCredential;

                var initializer = new ServiceAccountCredential.Initializer (credential.Id)
                {
                    User = serviceAccountEmail,
                    Key = credential.Key,
                    Scopes = Scopes
                };
                credential = new ServiceAccountCredential (initializer);
            }

            var service = new SheetsService (new BaseClientService.Initializer ()
            {
                HttpClientInitializer = credential,
                    ApplicationName = "timebot",
            });

            // Define request parameters.
            String spreadsheetId = "1QR078QvO5Q8S9gbQDglRhYK1HV3tBd0111SmjoVV0jQ";
            String range = "\'Faction Turns\'!A2:A14";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get (spreadsheetId, range);

            ValueRange response = request.Execute ();
            IList<IList<Object>> values = response.Values;
            List<string> factions = new List<string> ();
            if (values != null && values.Count > 0)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    factions.Add (values[i][0].ToString ());
                }
            }
            else
            {
                throw new KeyNotFoundException ();
            }

            string rtn = String.Join (System.Environment.NewLine, factions);

            await ReplyAsync (rtn);
            return;
        }

        // private Boolean validate_vote (SocketUser user, vote vote)
        // {
        //     Nacho nacho = new Nacho ();

        //     if (nacho.get_rep (user.Username, Convert.ToUInt64 (user.Discriminator)).FirstOrDefault () == null)
        //     {
        //         return false;
        //     }

        //     Nacho.representative rep = nacho.get_rep (user.Username, Convert.ToUInt64 (user.Discriminator)).FirstOrDefault ();

        //     if (rep.faction_text != vote.faction_name)
        //     {
        //         return false;
        //     }

        //     return true;

        // }

        [Command ("josef")]
        [Summary ("A present for the Deathless.")]
        public async Task JosefAsync ()
        {
            await ReplyAsync ("We have a month, a month is not that long.");
        }

        [Command ("domaths")]
        [Summary ("For the geniuses among us")]
        public async Task DoMathsAsync (params string[] input)
        {
            var response = await "http://api.mathjs.org/v4/"
                .SetQueryParams (new { expr = input })
                .GetAsync ()
                .ReceiveString ();

            await ReplyAsync (response);
        }

        [Command ("wasiyy")]
        [Summary ("For the authentic Wasiyy experience.")]
        public async Task WasiyyAsync ()
        {
            var rtn = "*Wasiyy walks to the table with a gun in hand. He puts the gun on the table and looks at you.* \"This is a gun.\"";

            await ReplyAsync (rtn);
        }

        [Command ("timetillockin")]
        [Summary ("Gets the time til the faction turn orders lockin.")]
        public async Task TimetillockinAsync ()
        {
            var client = new RestClient ("https://private.highchurch.space/Home/GetTurnDateTime");

            var request = new RestRequest ();

            var response = client.Get (request);

            DateTime lockin_UTC = DateTime.Parse (response.Content.ToString ().Replace ("\"", ""), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime ();

            TimeSpan span = lockin_UTC - DateTime.UtcNow;

            await ReplyAsync (GetReadableTimespan (span));
        }

        [Command ("churchbullies")]
        [Summary ("Returns one of the many reasons the church is a bully.")]
        public async Task ChurchbulliesAsync ()
        {
            var bullies = new List<BullyReason> ();

            using (var context = new Context ())
            {
                bullies = context.BullyReasons.ToList ();
            }

            BullyReason reason = bullies.ElementAt (Program.rand.Next (bullies.Count));

            await ReplyAsync (reason.value);
        }

        [Command ("churchbullies")]
        [Summary ("Returns one of the many reasons the church is a bully.")]
        public async Task ChurchbulliesAsync (params string[] input)
        {
            using (var context = new Context ())
            {
                var bully = new BullyReason ();
                bully.value = string.Join (" ", input);

                context.BullyReasons.Add (bully);
                await context.SaveChangesAsync ();
            }

            await ReplyAsync ("Reason Added");
        }

        [Command ("goldenrule")]
        [Summary ("There are many rules in the church. This lists them for you.")]
        public async Task GoldenruleAsync ()
        {
            List<string> rtn = new List<string> ();
            rtn.Add ("```");
            rtn.Add ("Rule 1: Don't be a dick.");
            rtn.Add ("Rule 2: See rule one.");
            rtn.Add ("```");
            await ReplyAsync (string.Join (System.Environment.NewLine, rtn));
        }

        [Command ("calblames")]
        [Summary ("Returns the number of times Cal has been blamed.")]
        public async Task CalblamesAsync ()
        {
            var count = 0;

            using (var context = new Context ())
            {
                var assets = context.BlameCals.ToList ();

                count = assets.Count ();
            }

            var rtn = "Cal has been blamed " + count + " times.";

            await ReplyAsync (rtn);
        }

        [Command ("blamecal")]
        [Summary ("Blames Cal.")]
        public async Task BlamecalAsync ()
        {
            var count = 0;

            using (var context = new Context ())
            {
                context.BlameCals.Add (new BlameCal () { timestamp = DateTime.Now });

                await context.SaveChangesAsync ();

                count = context.BlameCals.Count ();
            }

            var rtn = "Cal has been blamed " + count + " times.";

            await ReplyAsync (rtn);
        }

        [Command ("wiki")]
        [Summary ("Searches the english Wikipedia for an article.")]
        public async Task WikiAsync (params string[] input)
        {
            string query = string.Join (" ", input);

            string rtn = "";
            //https://en.wikipedia.org/w/api.php?action=query&prop=extracts&format=json&exintro=&titles=Stack%20Overflow

            var baseurl = "https://en.wikipedia.org/w/api.php";

            var response = await baseurl
                .SetQueryParams (new { action = "query", prop = "extracts", format = "json", exintro = "", titles = query })
                .GetJsonAsync<IDictionary<String, Object>> ();

            var extract = "";
            var title = "";

            var obj1 = new object ();
            var obj2 = new KeyValuePair[]();

            if (response.TryGetValue ("query", out obj1))
            {
                if (obj1.First() == null)
                {
                    await ReplyAsync ("No Results.");
                    return;
                }
                else
                {

                    Embed emb = Helper.ObjToEmbed (new { title = title, extract = GetPlainTextFromHtml (extract).Substring (1990) + "..." }, "title");

                    await ReplyAsync ("", false, emb, null);
                }

            }

            await ReplyAsync ("No Results");
            return;

        }

        private string GetPlainTextFromHtml (string htmlString)
        {
            string htmlTagPattern = "<.*?>";
            var regexCss = new Regex ("(\\<script(.+?)\\)|(\\<style(.+?)\\)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            htmlString = regexCss.Replace (htmlString, string.Empty);
            htmlString = Regex.Replace (htmlString, htmlTagPattern, string.Empty);
            htmlString = Regex.Replace (htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            htmlString = htmlString.Replace (" ", string.Empty);

            return htmlString;
        }

        public string GetReadableTimespan (TimeSpan ts)
        {
            // formats and its cutoffs based on totalseconds
            var cutoff = new SortedList<long, string>
                { { 59, "{3:S}" },
                    { 60, "{2:M}" },
                    { 60 * 60 - 1, "{2:M}, {3:S}" },
                    { 60 * 60, "{1:H}" },
                    { 24 * 60 * 60 - 1, "{1:H}, {2:M}" },
                    { 24 * 60 * 60, "{0:D}" },
                    { Int64.MaxValue, "{0:D}, {1:H}" }
                };

            // find nearest best match
            var find = cutoff.Keys.ToList ()
                .BinarySearch ((long) ts.TotalSeconds);
            // negative values indicate a nearest match
            var near = find < 0 ? Math.Abs (find) - 1 : find;
            // use custom formatter to get the string
            return String.Format (
                new HMSFormatter (),
                cutoff[cutoff.Keys[near]],
                ts.Days,
                ts.Hours,
                ts.Minutes,
                ts.Seconds);
        }
    }

    // formatter for forms of
    // seconds/hours/day
    public class HMSFormatter : ICustomFormatter, IFormatProvider
    {
        // list of Formats, with a P customformat for pluralization
        private static Dictionary<string, string> timeformats = new Dictionary<string, string>
        { { "S", "{0:P:Seconds:Second}" },
            { "M", "{0:P:Minutes:Minute}" },
            { "H", "{0:P:Hours:Hour}" },
            { "D", "{0:P:Days:Day}" }
        };

        public string Format (string format, object arg, IFormatProvider formatProvider)
        {
            return String.Format (new PluralFormatter (), timeformats[format], arg);
        }

        public object GetFormat (Type formatType)
        {
            return formatType == typeof (ICustomFormatter) ? this : null;
        }
    }

    // formats a numeric value based on a format P:Plural:Singular
    public class PluralFormatter : ICustomFormatter, IFormatProvider
    {
        public string Format (string format, object arg, IFormatProvider formatProvider)
        {
            if (arg != null)
            {
                var parts = format.Split (':'); // ["P", "Plural", "Singular"]

                if (parts[0] == "P") // correct format?
                {
                    // which index postion to use
                    int partIndex = (arg.ToString () == "1") ? 2 : 1;
                    // pick string (safe guard for array bounds) and format
                    return String.Format ("{0} {1}", arg, (parts.Length > partIndex ? parts[partIndex] : ""));
                }
            }
            return String.Format (format, arg);
        }

        public object GetFormat (Type formatType)
        {
            return formatType == typeof (ICustomFormatter) ? this : null;
        }
    }
}