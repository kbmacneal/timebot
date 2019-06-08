using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
using MoreLinq;
using Newtonsoft.Json;
using Npgsql;
using RestSharp;
using timebot.Classes;
using timebot.Classes.Utilities;
using timebot.Contexts;

namespace timebot.Modules.Commands
{
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

        private async Task SendPMAsync(string message, SocketUser user)
        {
            await user.SendMessageAsync(message);
        }

        [Command("ping")]
        [Summary("Test whether or no the bot is awake.")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!");
        }

        [Command("commands")]
        [Summary("Pastes a link to the help page in the chat.")]
        public async Task CommandsAsync()
        {
            List<string> rtn_message = new List<string>();

            rtn_message.Add("For complete instructions, go to https://highchurch.space/Bot");

            await ReplyAsync(String.Join(System.Environment.NewLine, rtn_message));
        }

        [Command("addspeaker")]
        [Summary("Adds the user as a speaker for meeting purposes.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddspeakerAsync(IGuildUser user)
        {
            Data.speaker spkr = Data.GuilduserToSpeaker(user);
            Data.insert_speaker(spkr);

            await ReplyAsync("User been added as a speaker");
        }

        [Command("changedefaults")]
        [Summary("Changes the default speaking time, if needed.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ChangedefaultsAsync(int minutes)
        {
            Data.reset_speaking_time(minutes);

            await ReplyAsync("Speaking times have been reset");
        }

        [Command("clearspeakers")]
        [Summary("Resets the speaker list.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearspeakersAsync()
        {
            List<ulong> roles = Context.Guild.Roles.Where(e => e.Name == "Speaker" || e.Name == "Observer" || e.Name == "NACHO").Select(e => e.Id).ToList();

            List<SocketGuildUser> users = Context.Guild.Users.ToList();

            foreach (SocketGuildUser usr in users)
            {
                if (roles.Any(usr.Roles.Select(e => e.Id).Contains))
                {
                    foreach (ulong role in roles)
                    {
                        await usr.RemoveRoleAsync(Context.Guild.GetRole(role));
                    }
                }
            }

            await ReplyAsync("Tags removed");
        }

        [Command("clearchannel")]
        [Summary("Clears the channel of chat messages.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearchannelAsync(int count)
        {
            var messages = (this.Context.Channel.GetMessagesAsync(count)).Flatten().ToList().GetAwaiter().GetResult();

            RequestOptions opt = new RequestOptions();

            opt.RetryMode = RetryMode.RetryRatelimit;

            foreach (var message in messages)
            {
                await message.DeleteAsync(opt);

                System.Threading.Thread.Sleep(2500);
            }

            // await this.Context.Channel.DeleteMessagesAsync(messages);
        }

        [Command("cactus")]
        [Summary("You should use this wherever possible.")]
        public async Task CactusAsync()
        {
            GuildEmote cactus = Context.Guild.Emotes.Where(e => e.Name == "cactusemo").First();

            await Context.Message.AddReactionAsync(cactus);

            string result = new StringBuilder().Insert(0, String.Concat("<:cactusemo:", cactus.Id.ToString(), ">"), 11).ToString();

            await ReplyAsync(result);

            return;
        }

        [Command("virtues")]
        [Summary("Ten Blessings yall.")]
        public async Task VirtuesAsync()
        {
            List<string> rtner = new List<string>();

            rtner.Add("```The First Virtue is Faith. Recitation: “Faith above all. We must trust God and their chosen Emperor to guide us.” Faith is exemplified by daily prayer, regular attendance of Church ceremonies, dutiful tithing, and pilgrimage to holy sites.```");
            rtner.Add("```The Second Virtue is Propriety, which flows from Faith. Recitation: “We must be obedient to tradition, ceremony, courtesy and station.” Propriety is exemplified by respectful loyalty to righteous authority and cultural norms, along with unfailing intolerance for all heretics and heathens. Custom, dress, and technology all must adhere to Propriety.```");
            rtner.Add("```The Third Virtue is Justice, which flows from Propriety. Recitation: “We must reward those who behave rightly and punish those who do not.” Justice is exemplified by its unflinching enforcement, and we must correct our own failings before looking to those of others.```");
            rtner.Add("```The Fourth Virtue is Fortitude, which reinforces Justice. Recitation: “We must patiently endure the challenges laid upon us and follow the rightful path despite them.” Fortitude is exemplified by steadfast courage and endurance in the face of adversity, particularly in avoiding all cringing or complaint when upon holy ground or fulfilling holy duties.```");
            rtner.Add("```The Fifth Virtue is Wisdom, which accompanies Fortitude. Recitation: “We must strive to see the world in its truth and shape it according to God’s will.” Wisdom is exemplified by perceived the flawed world as it is, but never losing sight of what it should be.  Daily reflection upon the sacred texts and their application to our lives is essential to Wisdom.```");
            await ReplyAsync(string.Join(System.Environment.NewLine, rtner), false, null, null);

            rtner = null;
            rtner = new List<string>();

            rtner.Add("```The Sixth Virtue is Temperance, which flows from Wisdom. Recitation: “We must show prudent moderation and diligent control over our desires.” Temperance is exemplified by self-restraint in all facets, particularly regular fasting and avoidance of intoxicants.```");
            rtner.Add("```The Seventh Virtue is Diligence, which reinforces Temperance.  Recitation: “We must be ever persistent and expend all effort and attention in keeping ourselves and others to the true path.” Diligence is exemplified by constant, tireless vigilance against temptation and treachery in all aspects of life.```");
            rtner.Add("```The Eighth Virtue is Charity, which echoes Justice. Recitation: “We must show compassion to those worthy of God’s mercy.” Charity is exemplified by philanthropic acts and outreach to faithful sufferers.```");
            rtner.Add("```The Ninth Virtue is Integrity, which echoes Propriety. Recitation: “We must honor our oaths and uphold the truth.” Integrity is exemplified by unfailing honesty and the exposure of deviants, as well as regular confession of our failings.```");
            rtner.Add("```The Tenth Virtue is Hope, which echoes Faith. Recitation: “We must never despair, no matter how dark the hour, as God shines their light upon us.”  Hope is exemplified by the conquest of despair, symbolized most prominently by the restriction of mourning to a designated period, as well as the teaching of the Virtues to the ignorant.```");

            await ReplyAsync(string.Join(System.Environment.NewLine, rtner), false, null, null);

            return;
        }

        [Command("churchapproved")]
        [Summary("The only way to express approval.")]
        public async Task ChurchapprovedAsync()
        {
            await Context.Channel.SendFileAsync("Cactus_Pius_seal.png", "Officially Approved by the High Church", false, null);

            return;
        }

        [Command("savior")]
        [Summary("Answers that age-old question.")]
        public async Task SaviorAsync()
        {
            await Context.Channel.SendFileAsync("TINMTTOS.png", null, false, null);

            return;
        }

        [Command("opensource")]
        [Summary("Pastes a link to the github repo in the chat.")]
        public async Task OpensourceAsync()
        {
            await ReplyAsync("This bot is open sourced and permissively licensed. You can find all source code in the github repo at https://github.com/kbmacneal/timebot");

            return;
        }

        [Command("synth")]
        [Summary("Determines whether or not someone is a synth.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SynthAsync(SocketUser user)
        {
            SocketGuildUser usr = Context.Guild.GetUser(user.Id);

            string name = usr.Nickname.ToString() == "" ? usr.Username : usr.Nickname;

            int synth_id = Program.rand.Next(0, Int32.MaxValue - 5);

            string new_nick = "A Synth, ID " + synth_id.ToString();

            await usr.ModifyAsync(e => e.Nickname = new_nick);

            await ReplyAsync("I knew it! " + name + " is a synth!");
        }

        [Command("tag")]
        [Summary("Searches for a tag and displays the summary in chat.")]
        public async Task TagAsync(params string[] collection)
        {
            string tag_name = String.Join(" ", collection);

            if (Classes.Tags.Tag.GetTags().FirstOrDefault(e => e.Name.ToUpperInvariant() == tag_name.ToUpperInvariant()) == null)
            {
                await ReplyAsync("Invalid tag selection.");
                return;
            }

            var tag = Classes.Tags.Tag.GetTags().FirstOrDefault(e => e.Name.ToUpperInvariant() == tag_name.ToUpperInvariant());

            Embed emb = Helper.ObjToEmbed(tag, "Name");

            await ReplyAsync("", false, emb, null);
        }

        [Command("asset")]
        [Summary("Search for an asset and display it in chat.")]
        public async Task AssetAsync(params string[] collection)
        {
            string asset_name = String.Join(" ", collection);

            TextInfo UsaTextInfo = new CultureInfo("en", false).TextInfo;
            asset_name = UsaTextInfo.ToTitleCase(asset_name);

            if (Classes.Assets.Asset.GetAssets().FirstOrDefault(e => e.Name == asset_name) == null)
            {
                await ReplyAsync("Invalid asset selection.");
                return;
            }

            Embed emb = Helper.ObjToEmbed(Classes.Assets.Asset.GetAssets().FirstOrDefault(e => e.Name == asset_name), "Name");

            await ReplyAsync("", false, emb, null);
        }

        [Command("badbot")]
        [Summary("At this point, I'm pretty sure the bot likes it.")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task BadbotAsync()
        {
            Dictionary<int, string> results = new Dictionary<int, string>();

            results.Add(1, "BANNED!");
            results.Add(2, "You come over here and say that to my face.");
            results.Add(3, "...");
            results.Add(4, "Eh? What?");
            results.Add(5, "Who said that?");
            results.Add(6, "```The details of my life are quite inconsequential... very well, where do I begin? My father was a relentlessly self-improving boulangerie owner from Belgium with low grade narcolepsy and a penchant for buggery. My mother was a fifteen year old French prostitute named Chloe with webbed feet. My father would womanize, he would drink. He would make outrageous claims like he invented the question mark. Sometimes he would accuse chestnuts of being lazy. The sort of general malaise that only the genius possess and the insane lament. My childhood was typical. Summers in Rangoon, luge lessons. In the spring we'd make meat helmets. When I was insolent I was placed in a burlap bag and beaten with reeds- pretty standard really. At the age of twelve I received my first scribe. At the age of fourteen a Zoroastrian named Vilma ritualistically shaved my testicles. There really is nothing like a shorn scrotum... it's breathtaking- I highly suggest you try it.```");

            Random rnd = new Random();

            int selection = rnd.Next(0, results.Keys.Max()) + 1;

            await ReplyAsync(results.Where(e => e.Key == selection).First().Value);
        }

        public int singleRoll(String roll)
        {
            int num_dice = 1;
            int result = 0;
            int di = roll.IndexOf('d');
            if (di == -1) return Int32.Parse(roll);
            int diceSize = Int32.Parse(roll.Substring(di + 1)); //value of string after 'd'
            if (di != 0) num_dice = Int32.Parse(roll.Substring(0, di));

            for (int i = 0; i < num_dice; i++)
            {
                result += Program.rand.Next(0, diceSize) + 1;
            }
            return result;
        }

        [Command("roll")]
        [Summary("Rolls some dice.")]
        public async Task RollAsync(params string[] args)
        {
            string roll = string.Join("", args).Replace(" ", "");
            List<int> dice_results = new List<int>();

            DiceBag db = new DiceBag();

            if (roll.Contains('-') || roll.Contains('+'))
            {
                char[] splits = new char[] { '+', '-' };
                string[] two_parts = roll.Split(splits);
                int mod = 0;
                if (roll.Contains('-'))
                {
                    mod = -1 * Convert.ToInt32(two_parts[1]);
                }
                else
                {
                    mod = Convert.ToInt32(two_parts[1]);
                }
                string[] parameters = two_parts[0].Split('d');
                uint num_dice = 1;
                int dice_sides = 2;

                if (parameters[0] != "")
                {
                    num_dice = Convert.ToUInt32(parameters[0]);
                    dice_sides = Convert.ToInt32(parameters[1]);
                }
                else
                {
                    dice_sides = Convert.ToInt32(parameters[1]);
                }

                DiceBag.Dice dice = (DiceBag.Dice)System.Enum.Parse(typeof(DiceBag.Dice), dice_sides.ToString());

                dice_results = db.RollQuantity(dice, num_dice);

                dice_results.Add(mod);
            }
            else
            {
                string[] parameters = roll.Split('d');
                uint num_dice = 1;
                int dice_sides = 2;

                if (parameters[0] != "")
                {
                    num_dice = Convert.ToUInt32(parameters[0]);
                    dice_sides = Convert.ToInt32(parameters[1]);
                }
                else
                {
                    dice_sides = Convert.ToInt32(parameters[1]);
                }

                DiceBag.Dice dice = (DiceBag.Dice)System.Enum.Parse(typeof(DiceBag.Dice), dice_sides.ToString());

                dice_results = db.RollQuantity(dice, num_dice);
            }

            SocketGuildUser usr = Context.Guild.GetUser(Context.Message.Author.Id) as SocketGuildUser;

            string rtn_name = usr.Nickname == null ? usr.Username : usr.Nickname;

            await ReplyAsync(rtn_name + " rolled a " + dice_results.Sum() + " (" + string.Join(", ", dice_results) + ")");
        }

        [Command("sector")]
        [Summary("Pastes the link to the sectorswithoutnumber page in chat.")]
        private async Task SectorAsync()
        {
            await ReplyAsync("https://sectorswithoutnumber.com/sector/m11ZXBOt6xiJGo21EKio");
        }

        [Command("trilljoke")]
        [Summary("Nuff said.")]
        private async Task TrilljokeAsync()
        {
            string msg = "What do you call a group of Trills?" + System.Environment.NewLine + "A Party" + System.Environment.NewLine + "👈😎👉";

            await ReplyAsync(msg, false, null, null);
        }

        [Command("xkcd")]
        [Summary("Random web comic anyone?")]
        private async Task XkcdAsync()
        {
            int comic_number = Program.rand.Next(0, (Program.latest_xkcd + 1));

            var response = await "https://xkcd.com/"
                .AppendPathSegment(comic_number)
                .AppendPathSegment("info.0.json")
                .GetAsync()
                .ReceiveString();

            var content = JsonConvert.DeserializeObject<Classes.Xkcd.Comic>(response);

            await ReplyAsync(content.Alt, false, null, null);

            await ReplyAsync(content.Img.ToString(), false, null, null);
        }

        [Command("xkcd")]
        [Summary("Random web comic anyone?")]
        private async Task XkcdAsync(int comic_number)
        {
            var response = await "https://xkcd.com/"
                .AppendPathSegment(comic_number)
                .AppendPathSegment("info.0.json")
                .GetAsync()
                .ReceiveString();

            var content = JsonConvert.DeserializeObject<Classes.Xkcd.Comic>(response);

            await ReplyAsync(content.Alt, false, null, null);

            await ReplyAsync(content.Img.ToString(), false, null, null);
        }

        public class commands_json
        {
            public string api_key { get; set; }
            public List<command> commands { get; set; }
        }

        public class command
        {
            public string name { get; set; }
            public string summary { get; set; }
            public bool admin_required { get; set; }
        }

        [Command("dumpcommands")]
        [Summary("Updates the command help page.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        private async Task DumpcommandsAsync()
        {
            List<command> commands = new List<command>();

            foreach (var command in Program._commands.Commands.ToList())
            {
                command c = new command
                {
                    name = command.Name,
                    summary = command.Summary,
                    admin_required = command.Preconditions.Count() > 0
                };

                commands.Add(c);
            }

            Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(Program.secrets_file));

            using (Npgsql.NpgsqlConnection conn = new NpgsqlConnection(JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(Program.secrets_file))["connection_string"]))
            {
                conn.Open();

                // Insert some data
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM commands;";
                    cmd.ExecuteNonQuery();
                }

                foreach (var item in commands.DistinctBy(e => e.name.ToString() + e.summary.ToString()).OrderBy(e => e.name).ThenBy(e => e.admin_required))
                {
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "INSERT INTO commands (name,summary,admin_required) VALUES (@n,@s,@a)";
                        cmd.Parameters.AddWithValue("n", item.name);
                        cmd.Parameters.AddWithValue("s", item.summary);
                        cmd.Parameters.AddWithValue("a", item.admin_required);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            await ReplyAsync("Commands Updated");
        }

        [Command("summary")]
        [Summary("Lets you know what a command is. I'm sensing a recursion here.")]
        public async Task SummaryAsync(params string[] commands)
        {
            string rtn = "";

            var command = Program._commands.Commands.FirstOrDefault(e => e.Name == String.Join(" ", commands));

            if (command == null)
            {
                rtn = "Command doesn't exist.";
            }
            else
            {
                rtn = command.Summary;
            }

            await ReplyAsync(rtn);
        }

        [Command("turn")]
        [Summary("Gives you a particular turn for a particular faction, straight from the faction tracker. tb!turn <turn #> <Faction without quotation marks>")]
        public async Task TurnAsync(int turn, params string[] faction)
        {
            string faction_name = String.Join(" ", faction);
            string rtn = "";
            ServiceAccountCredential credential;
            string[] Scopes = { SheetsService.Scope.Spreadsheets };
            string serviceAccountEmail = "timebot@timebot.iam.gserviceaccount.com";
            string jsonfile = "Timebot-fc612c6f90aa.json";
            using (Stream stream = new FileStream(@jsonfile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                credential = (ServiceAccountCredential)
                GoogleCredential.FromStream(stream).UnderlyingCredential;

                var initializer = new ServiceAccountCredential.Initializer(credential.Id)
                {
                    User = serviceAccountEmail,
                    Key = credential.Key,
                    Scopes = Scopes
                };
                credential = new ServiceAccountCredential(initializer);
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "timebot",
            });

            // Define request parameters.
            String spreadsheetId = "1QR078QvO5Q8S9gbQDglRhYK1HV3tBd0111SmjoVV0jQ";
            String range = "\'Faction Turns\'!A2:A14";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            int row_index = -1;
            if (values != null && values.Count > 0)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (values[i][0].ToString() == faction_name)
                    {
                        row_index = i + 2;
                        break;
                    }
                }

                if (row_index == -1)
                {
                    throw new KeyNotFoundException();
                }
            }
            else
            {
                throw new KeyNotFoundException();
            }

            string col_name = cols[turn + 1];

            range = "\'Faction Turns\'!" + col_name + row_index;

            SpreadsheetsResource.ValuesResource.GetRequest cell_request =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            ValueRange cell_resp = cell_request.Execute();
            if (cell_resp.Values == null)
            {
                rtn = "I'm sorry Dave, I'm afraid I can't do that.";
                await ReplyAsync(rtn);
                return;
            }
            IList<IList<Object>> cell_val = cell_resp.Values;

            if (values == null || values.Count == 0)
            {
                throw new KeyNotFoundException();
            }

            if (cell_val[0][0] == null)
            {
                rtn = "I'm sorry Dave, I'm afraid I can't do that.";
                await ReplyAsync(rtn);
                return;
            }
            else
            {
                rtn = cell_val[0][0].ToString();
            }

            if (rtn.ToCharArray().Length >= 2000)
            {
                Queue<string> sender = new Queue<string>();
                Queue<char> rtn_q = new Queue<char>();

                rtn.ToCharArray().ToList().ForEach(e => rtn_q.Enqueue(e));

                while (rtn_q.Count > 0)
                {
                    int count = rtn_q.Count < 2000 ? rtn_q.Count : 2000;

                    string part = String.Join("", rtn_q.Take(count));

                    rtn_q.ToList().RemoveRange(0, count);

                    sender.Enqueue(part);
                }

                foreach (string part in sender)
                {
                    await ReplyAsync(part);
                }
            }
            else
            {
                await ReplyAsync(rtn);
            }
        }

        [Command("assets")]
        [Summary("Dumps a list of faction assets and their locations into the chat.")]
        public async Task AssetsAsync(params string[] faction)
        {
            string faction_name = String.Join(" ", faction);

            List<string> rtn = new List<string>();

            ServiceAccountCredential credential;

            string[] Scopes = { SheetsService.Scope.Spreadsheets };

            string serviceAccountEmail = "timebot@timebot.iam.gserviceaccount.com";

            string jsonfile = "Timebot-fc612c6f90aa.json";

            using (Stream stream = new FileStream(@jsonfile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                credential = (ServiceAccountCredential)
                GoogleCredential.FromStream(stream).UnderlyingCredential;

                var initializer = new ServiceAccountCredential.Initializer(credential.Id)
                {
                    User = serviceAccountEmail,
                    Key = credential.Key,
                    Scopes = Scopes
                };
                credential = new ServiceAccountCredential(initializer);
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "timebot",
            });

            // Define request parameters.
            String spreadsheetId = "1QR078QvO5Q8S9gbQDglRhYK1HV3tBd0111SmjoVV0jQ";

            String range = "AssetTracker!A2:L";

            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = request.Execute();

            List<Classes.Assets.TrackerAsset> assets = new List<Classes.Assets.TrackerAsset>();

            IList<IList<Object>> values = response.Values;

            int row_index = -1;

            if (values != null && values.Count > 0)
            {
                for (int i = 0; i < values.Count - 1; i++)
                {
                    if (values[i][0].ToString() == faction_name)
                    {
                        Classes.Assets.TrackerAsset asset = new Classes.Assets.TrackerAsset()
                        {
                            Owner = values[i][0].ToString(),
                            Asset = values[i][1].ToString(),
                            Stealthed = values[i][3].ToString(),
                            Stat = values[i][4].ToString(),
                            HP = values[i][5].ToString(),
                            MaxHP = values[i][6].ToString(),
                            CombinedHP = values[i][5].ToString() + "/" + values[i][6].ToString(),
                            Type = values[i][7].ToString(),
                            Attack = values[i][8].ToString(),
                            Counter = values[i][9].ToString(),
                            Notes = values[i][10].ToString(),
                            Location = values[i][11].ToString().Split("/")[2].ToString()
                        };
                        row_index = i;

                        assets.Add(asset);
                    }

                    if (values[i][11].ToString().Contains(faction_name))
                    {
                        Classes.Assets.TrackerAsset asset = new Classes.Assets.TrackerAsset()
                        {
                            Owner = values[i][0].ToString(),
                            Asset = values[i][1].ToString(),
                            Stealthed = values[i][3].ToString(),
                            Stat = values[i][4].ToString(),
                            HP = values[i][5].ToString(),
                            MaxHP = values[i][6].ToString(),
                            CombinedHP = values[i][5].ToString() + "/" + values[i][6].ToString(),
                            Type = values[i][7].ToString(),
                            Attack = values[i][8].ToString(),
                            Counter = values[i][9].ToString(),
                            Notes = values[i][10].ToString(),
                            Location = values[i][11].ToString().Split("/")[2].ToString()
                        };
                        row_index = i;

                        assets.Add(asset);
                    }
                }

                if (row_index == -1)
                {
                    throw new KeyNotFoundException();
                }
            }
            else
            {
                throw new KeyNotFoundException();
            }

            await ReplyAsync("Assets for: " + faction_name);

            var header = new string[6] { "Owner", "Name", "HP", "Attack Dice", "Counter Dice", "Location" };

            var table = Classes.TableParser.ToStringTable(assets.Select(asset => new { asset.Owner, asset.Asset, asset.CombinedHP, asset.Attack, asset.Counter, asset.Location }).OrderBy(e => e.Owner).ThenBy(e => e.Location).ThenBy(e => e.Asset), header, a => a.Owner.Length > 15 ? a.Owner.Substring(0, 15) + "..." : a.Owner, a => a.Asset, a => a.CombinedHP, a => a.Attack, a => a.Counter, a => a.Location);

            Helper.SplitToLines(table, 1994).ForEach(e => ReplyAsync("```" + e + "```").GetAwaiter().GetResult());
        }

        [Command("turnfactions")]
        [Summary("Prints a list of the factions in the tracker if you arent sure what to type.")]
        public async Task TurnFactionsAsync()
        {
            ServiceAccountCredential credential;
            string[] Scopes = { SheetsService.Scope.Spreadsheets };
            string serviceAccountEmail = "timebot@timebot.iam.gserviceaccount.com";
            string jsonfile = "Timebot-fc612c6f90aa.json";
            using (Stream stream = new FileStream(@jsonfile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                credential = (ServiceAccountCredential)
                GoogleCredential.FromStream(stream).UnderlyingCredential;

                var initializer = new ServiceAccountCredential.Initializer(credential.Id)
                {
                    User = serviceAccountEmail,
                    Key = credential.Key,
                    Scopes = Scopes
                };
                credential = new ServiceAccountCredential(initializer);
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "timebot",
            });

            // Define request parameters.
            String spreadsheetId = "1QR078QvO5Q8S9gbQDglRhYK1HV3tBd0111SmjoVV0jQ";
            String range = "\'Faction Turns\'!A2:A";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            List<string> factions = new List<string>();
            if (values != null && values.Count > 0)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    factions.Add(values[i][0].ToString());
                }
            }
            else
            {
                throw new KeyNotFoundException();
            }

            string rtn = String.Join(System.Environment.NewLine, factions);

            await ReplyAsync(rtn);
            return;
        }

        [Command("josef")]
        [Summary("A present for the Deathless.")]
        public async Task JosefAsync()
        {
            await ReplyAsync("We have a month, a month is not that long.");
        }

        [Command("domaths")]
        [Summary("For the geniuses among us")]
        public async Task DoMathsAsync(params string[] input)
        {
            var response = await "http://api.mathjs.org/v4/"
                .SetQueryParams(new { expr = input })
                .GetAsync()
                .ReceiveString();

            await ReplyAsync(response);
        }

        [Command("wasiyy")]
        [Summary("For the authentic Wasiyy experience.")]
        public async Task WasiyyAsync()
        {
            var rtn = "*Wasiyy walks to the table with a gun in hand. He puts the gun on the table and looks at you.* \"This is a gun.\"";

            await ReplyAsync(rtn);
        }

        [Command("timetillockin")]
        [Summary("Gets the time til the faction turn orders lockin.")]
        public async Task TimetillockinAsync()
        {
            string lockin_string = await "https://private.highchurch.space"
                .AppendPathSegment("Home")
                .AppendPathSegment("GetLockinDateTime")
                .GetStringAsync();

            DateTime lockin_time = DateTime.Parse(lockin_string.Replace("\"", ""), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();

            TimeSpan span = lockin_time - DateTime.UtcNow;

            string rtn = GetReadableTimespan(span);

            if (rtn.StartsWith('-'))
            {
                await ReplyAsync("0 days");
            }
            else
            {
                await ReplyAsync(rtn);
            }
        }

        [Command("timetilturn")]
        [Summary("Gets the time til the faction turn.")]
        public async Task TimetilturnAsync()
        {
            string lockin_string = await "https://private.highchurch.space"
                .AppendPathSegment("Home")
                .AppendPathSegment("GetTurnDateTime")
                .GetStringAsync();

            DateTime lockin_time = DateTime.Parse(lockin_string.Replace("\"", ""), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();

            TimeSpan span = lockin_time - DateTime.UtcNow;

            string rtn = GetReadableTimespan(span);

            if (rtn.StartsWith('-'))
            {
                await ReplyAsync("0 days");
            }
            else
            {
                await ReplyAsync(rtn);
            }
        }

        [Command("churchbullies")]
        [Summary("Returns one of the many reasons the church is a bully.")]
        public async Task ChurchbulliesAsync()
        {
            var bullies = new List<BullyReason>();

            using (var context = new Context())
            {
                bullies = context.BullyReasons.ToList();
            }

            BullyReason reason = bullies.ElementAt(Program.rand.Next(bullies.Count));

            await ReplyAsync(reason.value);
        }

        [Command("churchbullies")]
        [Summary("Returns one of the many reasons the church is a bully.")]
        public async Task ChurchbulliesAsync(params string[] input)
        {
            using (var context = new Context())
            {
                var bully = new BullyReason();
                bully.value = string.Join(" ", input);

                context.BullyReasons.Add(bully);
                await context.SaveChangesAsync();
            }

            await ReplyAsync("Reason Added");
        }

        [Command("goldenrule")]
        [Summary("There are many rules in the church. This lists them for you.")]
        public async Task GoldenruleAsync()
        {
            List<string> rtn = new List<string>();
            rtn.Add("```");
            rtn.Add("Rule 1: Don't be a dick.");
            rtn.Add("Rule 2: See rule one.");
            rtn.Add("```");
            await ReplyAsync(string.Join(System.Environment.NewLine, rtn));
        }

        [Command("blamecal")]
        [Summary("Blames Cal.")]
        public async Task BlamecalAsync()
        {
            var count = 0;

            using (var context = new Context())
            {
                count = context.BlameCals.Count() + 1;

                if (count - 1 > 3201)
                {
                    await ReplyAsync("Cal has been blamed enough don't you think?");
                    return;
                }

                await context.BlameCals.AddAsync(new BlameCal() { timestamp = DateTime.Now });

                await context.SaveChangesAsync();
            }

            var rtn = "Cal has been blamed " + count + " times.";

            var rtn_2 = "Only " + (3201 - count).ToString() + " blames to go!";

            var rtn_3 = "Gaius has acquired " + Math.Floor((count / 533.5)).ToString() + " Infinity Stones!";

            await ReplyAsync(rtn + System.Environment.NewLine + rtn_2 + System.Environment.NewLine + rtn_3);
        }

        [Command("wiki")]
        [Summary("Searches the english Wikipedia for an article.")]
        public async Task WikiAsync(params string[] input)
        {
            string query = string.Join(" ", input);

            var baseurl = "https://en.wikipedia.org/w/api.php";

            var response = await baseurl
                .SetQueryParams(new { action = "query", prop = "extracts", format = "json", exintro = "", titles = query })
                .GetStringAsync();

            var deserial = Wiki.FromJson(response);

            if (deserial.Query.Pages.First().Key == "-1")
            {
                await ReplyAsync("No Results.");
                return;
            }
            else
            {
                var info = await baseurl
                    .SetQueryParams(new { action = "query", prop = "info", format = "json", inprop = "url", pageids = deserial.Query.Pages.First().Key })
                    .GetStringAsync();

                var infodeserial = Info.FromJson(info);

                //https://en.wikipedia.org/w/api.php?action=query&prop=info&format=json&pageids=15573

                var title = deserial.Query.Pages.First().Value.Title;
                var link = infodeserial.Query.Pages.First().Value.URL;
                var content = Helper.GetPlainTextFromHtml(deserial.Query.Pages.First().Value.Extract.Substring(0, 1000)) + "...";

                Embed emb = Helper.ObjToEmbed(new { title = title, summary = content, url = link }, "title");

                await ReplyAsync("", false, emb, null);
                return;
            }
        }

        [Command("thanos")]
        [Summary("Determines your place in Thanos's balance.")]
        public async Task ThanosAsync(string role)
        {
            string rtn = "";

            if (role != "Dusted" && role != "Survivor")
            {
                rtn = "There is only Dusted or Survivor in the cosmic balance.";
                await ReplyAsync(rtn);
                return;
            }

            using (var context = new Context())
            {
                if (context.Thanos.FirstOrDefault(e => e.playerID == Context.User.Id) != null)
                {
                    await ReplyAsync("Your place has already been determined, mortal.");
                }
                else
                {
                    var bal = new Thanos()
                    {
                        playerID = Context.User.Id,
                        role_choice = role
                    };

                    await context.Thanos.AddAsync(bal);

                    await context.SaveChangesAsync();

                    await ReplyAsync("Your place has been determined, mortal.");
                }
            }

            return;
        }

        [Command("valid")]
        [Summary("Figures out if someone is valid.")]
        public async Task ValidAsync(IGuildUser user)
        {
            var role = Context.Guild.Roles.FirstOrDefault(e => e.Name == "valid");
            if (role == null)
            {
                await ReplyAsync("No valid role available.");
                return;
            }

            var mention = user.Mention;

            await user.AddRoleAsync(role);

            await ReplyAsync(mention + " is valid.");
            return;
        }

        [Command("snap")]
        [Summary("Makes the server perfectly balanced.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SnapAsync()
        {
            await ReplyAsync("The Balancing has begun, puny mortals.");
            await EnactBalance(Context);
        }

        public async Task EnactBalance(SocketCommandContext c)
        {
            var users = c.Guild.Users.Where(e => !e.Roles.Select(f => f.Name).Contains("Dusted")).Where(e => !e.Roles.Select(f => f.Name).Contains("Survivor")).ToList<SocketGuildUser>();

            var _opt = new RequestOptions()
            {
                RetryMode = RetryMode.RetryRatelimit
            };

            var Thani = new List<Thanos>();

            using (var context = new Context())
            {
                Thani = context.Thanos.ToList<Thanos>();
            }

            foreach (var mortal in Thani)
            {
                var user = c.Guild.GetUser((ulong)(mortal.playerID));

                await user.AddRoleAsync(c.Guild.Roles.FirstOrDefault(e => e.Name == mortal.role_choice), _opt);
            }

            foreach (var mortal in users.Where(e => !Thani.Select(f => f.playerID).Contains(e.Id)))
            {
                var selection = Program.rand.Next(0, 2);

                switch (selection)
                {
                    case 0:
                        var dusted = c.Guild.GetUser((ulong)(mortal.Id));

                        await dusted.AddRoleAsync(c.Guild.Roles.FirstOrDefault(e => e.Name == "Dusted"), _opt);
                        break;

                    case 1:
                        var survivor = c.Guild.GetUser((ulong)(mortal.Id));

                        await survivor.AddRoleAsync(c.Guild.Roles.FirstOrDefault(e => e.Name == "Survivor"), _opt);
                        break;

                    default:
                        var def = c.Guild.GetUser((ulong)(mortal.Id));

                        await def.AddRoleAsync(c.Guild.Roles.FirstOrDefault(e => e.Name == "Dusted"), _opt);
                        break;
                }
            }
        }

        public string GetReadableTimespan(TimeSpan ts)
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
            var find = cutoff.Keys.ToList()
                .BinarySearch((long)ts.TotalSeconds);
            // negative values indicate a nearest match
            var near = find < 0 ? Math.Abs(find) - 1 : find;
            // use custom formatter to get the string
            return String.Format(
                new HMSFormatter(),
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

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return String.Format(new PluralFormatter(), timeformats[format], arg);
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }

    // formats a numeric value based on a format P:Plural:Singular
    public class PluralFormatter : ICustomFormatter, IFormatProvider
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg != null)
            {
                var parts = format.Split(':'); // ["P", "Plural", "Singular"]

                if (parts[0] == "P") // correct format?
                {
                    // which index postion to use
                    int partIndex = (arg.ToString() == "1") ? 2 : 1;
                    // pick string (safe guard for array bounds) and format
                    return String.Format("{0} {1}", arg, (parts.Length > partIndex ? parts[partIndex] : ""));
                }
            }
            return String.Format(format, arg);
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }
}