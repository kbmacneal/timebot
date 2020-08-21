using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Flurl;
using Flurl.Http;
using MoreLinq;
using Newtonsoft.Json;
using Npgsql;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using timebot.Classes;
using timebot.Classes.Assets;
using timebot.Classes.Extensions;
using timebot.Classes.Utilities;
using timebot.Contexts;

namespace timebot.Modules.Commands
{
    public class commands : ModuleBase<SocketCommandContext>
    {
        private static string[] insults = new string[]
        {
            "A most notable coward, an infinite and endless liar, an hourly promise breaker, the owner of no one good quality.",
            "Away, you starvelling, you elf-skin, you dried neat’s-tongue, bull’s-pizzle, you stock-fish!",
            "Away, you three-inch fool!",
            "Come, come, you froward and unable worms!",
            "Go, prick thy face, and over-red thy fear, Thou lily-liver’d boy.",
            "His wit’s as thick as a Tewkesbury mustard.",
            "I am pigeon-liver’d and lack gall.",
            "I am sick when I do look on thee.",
            "I must tell you friendly in your ear, sell when you can, you are not for all markets.",
            "If thou wilt needs marry, marry a fool; for wise men know well enough what monsters you make of them.",
            "I’ll beat thee, but I would infect my hands.",
            "I scorn you, scurvy companion.",
            "Methink’st thou art a general offence and every man should beat thee.",
            "More of your conversation would infect my brain.",
            "My wife’s a hobby horse!",
            "Peace, ye fat guts!",
            "Poisonous bunch-backed toad!",
            "The rankest compound of villainous smell that ever offended nostril",
            "The tartness of his face sours ripe grapes.",
            "There’s no more faith in thee than in a stewed prune.",
            "Thine forward voice, now, is to speak well of thine friend; thine backward voice is to utter foul speeches and to detract.",
            "That trunk of humours, that bolting-hutch of beastliness, that swollen parcel of dropsies, that huge bombard of sack, that stuffed cloak-bag of guts, that roasted Manningtree ox with pudding in his belly, that reverend vice, that grey Iniquity, that father ruffian, that vanity in years?",
            "Thine face is not worth sunburning.",
            "This woman’s an easy glove, my lord, she goes off and on at pleasure.",
            "Thou art a boil, a plague sore",
            "Was the Duke a flesh-monger, a fool and a coward?",
            "Thou art as fat as butter.",
            "Here is the babe, as loathsome as a toad.",
            "Like the toad; ugly and venomous.",
            "Thou art unfit for any place but hell.",
            "Thou cream faced loon",
            "Thou clay-brained guts, thou knotty-pated fool, thou whoreson obscene greasy tallow-catch!",
            "Thou damned and luxurious mountain goat.",
            "Thou elvish-mark’d, abortive, rooting hog!",
            "Thou leathern-jerkin, crystal-button, knot-pated, agatering, puke-stocking, caddis-garter, smooth-tongue, Spanish pouch!",
            "Thou lump of foul deformity",
            "That poisonous bunch-back’d toad!",
            "Thou sodden-witted lord! Thou hast no more brain than I have in mine elbows.",
            "Thou subtle, perjur’d, false, disloyal man!",
            "Thou whoreson zed , thou unnecessary letter!",
            "Thy sin’s not accidental, but a trade.",
            "Thy tongue outvenoms all the worms of Nile.",
            "Would thou wert clean enough to spit upon.",
            "Would thou wouldst burst!",
            "You poor, base, rascally, cheating lack-linen mate!",
            "You are as a candle, the better burnt out.",
            "You scullion! You rampallian! You fustilarian! I’ll tickle your catastrophe!",
            "You starvelling, you eel-skin, you dried neat’s-tongue, you bull’s-pizzle, you stock-fish–O for breath to utter what is like thee!-you tailor’s-yard, you sheath, you bow-case, you vile standing tuck!",
            "Your brain is as dry as the remainder biscuit after voyage.",
            "Virginity breeds mites, much like a cheese.",
            "Villain, I have done thy mother"
        };

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

        public class nextevent
        {
            [JsonProperty("name")]
            public string name { get; set; }

            [JsonProperty("time")]
            public string time { get; set; }

            [JsonProperty("countdown")]
            public string countdown { get; set; }
        }

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
            // await ReplyAsync ("For complete instructions, go to https://highchurch.space/Bot");

            using (var context = new Context())
            {
                var valid_commands = context.BotCommands.Where(e => e.serverid == Context.Guild.Id);

                var command_list = Program._commands.Commands.Where(e => valid_commands.Select(f => f.commandname).Contains(e.Name)).DistinctBy(e => e.Name);

                var header = new string[2] { "Command", "Summary" };

                var table = Classes.TableParser.ToStringTable(command_list.Select(command => new { command.Name, command.Summary }).OrderBy(e => e.Name).ThenBy(e => e.Summary), header, a => a.Name, a => a.Summary.Length > 30 ? a.Summary.Substring(0, 30) + "..." : a.Summary);

                Helper.SplitToLines(table, 1994).ForEach(e => ReplyAsync("```" + e + "```").GetAwaiter().GetResult());
            }
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

        [Command("opensource")]
        [Summary("Pastes a link to the github repo in the chat.")]
        public async Task OpensourceAsync()
        {
            await ReplyAsync("This bot is open sourced and permissively licensed. You can find all source code in the github repo at https://github.com/kbmacneal/timebot");

            return;
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

            var shakespeare = insults.GetRandom(1).First();

            results.Add(1, "BANNED!");
            results.Add(2, "You come over here and say that to my face.");
            results.Add(3, "As Bill Shakespeare would say: \"" + shakespeare + "\"");
            results.Add(4, "Eh? What?");
            results.Add(5, "Who said that?");
            results.Add(6, "```The details of my life are quite inconsequential... very well, where do I begin? My father was a relentlessly self-improving boulangerie owner from Belgium with low grade narcolepsy and a penchant for buggery. My mother was a fifteen year old French prostitute named Chloe with webbed feet. My father would womanize, he would drink. He would make outrageous claims like he invented the question mark. Sometimes he would accuse chestnuts of being lazy. The sort of general malaise that only the genius possess and the insane lament. My childhood was typical. Summers in Rangoon, luge lessons. In the spring we'd make meat helmets. When I was insolent I was placed in a burlap bag and beaten with reeds- pretty standard really. At the age of twelve I received my first scribe. At the age of fourteen a Zoroastrian named Vilma ritualistically shaved my testicles. There really is nothing like a shorn scrotum... it's breathtaking- I highly suggest you try it.```");

            Random rnd = new Random();

            int selection = rnd.Next(0, results.Keys.Max()) + 1;

            await ReplyAsync(results.Where(e => e.Key == selection).First().Value);
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

        [Command("blamecal")]
        [Summary("Blames Cal.")]
        public async Task BlamecalAsync()
        {
            var count = 0;

            using (var context = new Context())
            {
                count = context.BlameCals.Count() + 1;

                if (count - 1 >= 3201)
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

        [Command("randomasset")]
        [Summary("Generates a completely random asset.")]
        public async Task RandomassetAsync()
        {
            var assets = Asset.GetAssets();

            var rtn = new Asset()
            {
                ID = 0,
                Name = assets.ElementAt(Program.rand.Next(0, assets.Count)).Name,
                HP = assets.ElementAt(Program.rand.Next(0, assets.Count)).HP,
                Attack = assets.ElementAt(Program.rand.Next(0, assets.Count)).Attack,
                Counterattack = assets.ElementAt(Program.rand.Next(0, assets.Count)).Counterattack,
                Description = assets.ElementAt(Program.rand.Next(0, assets.Count)).Description,
                Type = assets.ElementAt(Program.rand.Next(0, assets.Count)).Type,
                Tier = assets.ElementAt(Program.rand.Next(0, assets.Count)).Tier,
                TechLevel = assets.ElementAt(Program.rand.Next(0, assets.Count)).TechLevel,
                Cost = assets.ElementAt(Program.rand.Next(0, assets.Count)).Cost,
                AssetType = assets.ElementAt(Program.rand.Next(0, assets.Count)).AssetType
            };

            Embed emb = Helper.ObjToEmbed(rtn, "Name");

            await ReplyAsync("", false, emb, null);
        }

        [Command("sayno")]
        [Summary("Pastes the word 'No' into chat.")]
        public async Task SaynoAsync()
        {
            await ReplyAsync("Yes.");
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