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
using timebot.Classes.Utilities;
using System.Text.RegularExpressions;

namespace timebot.Modules.Commands
{

    public class commands : ModuleBase<SocketCommandContext>
    {

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

            rtn_message.Add("For complete instructions, go to https://highchurch.space/Bot");

            await ReplyAsync(String.Join(System.Environment.NewLine, rtn_message));
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

                spkr = Data.get_speakers().First(s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator);

                spkr.start_time = DateTime.Now;

                string msg = String.Concat("You are now the speaker. You have ", spkr.speaking_time_minutes, " minutes remaining");

                await user.SendMessageAsync(msg);

                timer tmr = new timer();

                tmr.user = user;

                tmr.StartTimer(spkr.speaking_time_minutes * 60 * 1000);
            }
            else
            {
                spkr = Data.get_speakers().First(s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator);

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
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearchannelAsync(int count)
        {
            var messages = await this.Context.Channel.GetMessagesAsync(count).Flatten();

            RequestOptions opt = new RequestOptions();

            opt.RetryMode = RetryMode.RetryRatelimit;

            foreach (var message in messages)
            {
                await message.DeleteAsync(opt);

                System.Threading.Thread.Sleep(2500);
            }

            // await this.Context.Channel.DeleteMessagesAsync(messages);

        }

        [Command("clearchannel")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearchannelAsync()
        {
            var messages = await this.Context.Channel.GetMessagesAsync(Int32.MaxValue).Flatten();

            await Context.Channel.DeleteMessagesAsync(messages);

            // await this.Context.Channel.DeleteMessagesAsync(messages);

        }

        [Command("cactus")]
        public async Task CactusAsync()
        {
            GuildEmote cactus = Context.Guild.Emotes.Where(e => e.Name == "cactusemo").First();

            await Context.Message.AddReactionAsync(cactus);

            string result = new StringBuilder().Insert(0, String.Concat("<:cactusemo:", cactus.Id.ToString(), ">"), 11).ToString();

            await ReplyAsync(result);

            return;
        }

        [Command("churchapproved")]
        public async Task ChurchapprovedAsync()
        {
            await Context.Channel.SendFileAsync("Cactus_Pius_seal.png", "Officially Approved by the High Church", false, null);

            return;
        }

        [Command("savior")]
        public async Task SaviorAsync()
        {
            await Context.Channel.SendFileAsync("TINMTTOS.png", null, false, null);

            return;
        }

        [Command("opensource")]
        public async Task OpensourceAsync()
        {
            await ReplyAsync("This bot is open sourced and permissively licensed. You can find all source code in the github repo at https://github.com/kbmacneal/timebot");

            return;
        }

        [Command("vote")]
        public async Task vote(string faction, int question, int selection)
        {

            vote vote = new vote();

            if (Context.Guild.Roles.Where(e => e.Name == faction).FirstOrDefault() == null)
            {
                await ReplyAsync("Faction invalid.");
                return;
            }

            vote.name = Context.User.Username;
            vote.discriminator = Convert.ToUInt64(Context.User.Discriminator);
            vote.selection = selection;
            vote.vote_id = question;
            vote.faction_name = faction;
            vote.faction_id = Context.Guild.Roles.Where(e => e.Name == faction).Select(e => e.Id).FirstOrDefault();

            Boolean can_vote = validate_vote(Context.User, vote);
            if (!can_vote)
            {
                await ReplyAsync("User not authorized to cast a vote for this faction.");
                return;
            }

            factionvoting voter = new factionvoting();
            await voter.add_vote(vote);

            await ReplyAsync("Vote cast");
        }

        [Command("tally")]
        public async Task tally(int question_id)
        {
            List<string> results = new List<string>();
            results.Add("```");
            factionvoting voting = new factionvoting();
            List<vote> votes = voting.return_tally(question_id);
            List<int> options = votes.Select(e => e.selection).Distinct().ToList();

            foreach (int option in options)
            {
                results.Add("The tally for option " + option.ToString() + " is: " + votes.Where(e => e.selection == option).ToList().Count().ToString());
                results.Add("The factions who voted for this option are:");
                results.Add(String.Join(", ", votes.Select(e => e.faction_name)));
            }
            results.Add("```");

            string rtn = string.Join(System.Environment.NewLine, results);

            await ReplyAsync(rtn);
        }

        [Command("deletequestion")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeletequestionAsync(int question_id)
        {
            factionvoting voting = new factionvoting();

            await voting.delete_question(question_id);

            await ReplyAsync("Votes for the question have been removed");
        }

        [Command("synth")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SynthAsync(SocketUser user)
        {
            SocketGuildUser usr = Context.Guild.GetUser(user.Id);

            string name = usr.Nickname.ToString() == "" ? usr.Username : usr.Nickname;

            int synth_id = Program.rand.Next(0, Int32.MaxValue - 5);

            string new_nick = "A Synth, ID " + synth_id.ToString();
            
            await usr.ModifyAsync(e=> e.Nickname = new_nick);

            await ReplyAsync("I knew it! " + name + " is a synth!");
        }

        [Command("asset")]
        public async Task AssetAsync(params string[] collection)
        {
            string asset_name = String.Join(" ", collection);

            if(Program.assets.FirstOrDefault(e=>e.Name == asset_name) == null)
            {
                await ReplyAsync("Invalid asset selection.");
                return;
            }

            Embed emb = Helper.ObjToEmbed(Program.assets.FirstOrDefault(e=>e.Name == asset_name),"Name");

            await ReplyAsync("",false,emb,null);
        }

        [Command("badbot")]
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

            int selection = rnd.Next(0, results.Keys.Max())+1;

            await ReplyAsync(results.Where(e => e.Key == selection).First().Value);
        }

        public int singleRoll(String roll)
        {
            int num_dice = 1;
            int result = 0;
            int di = roll.IndexOf('d');
            if (di == -1) return Int32.Parse(roll);
            int diceSize = Int32.Parse(roll.Substring(di + 1)); //value of string after 'd'
            if(di!=0)num_dice = Int32.Parse(roll.Substring(0,di));

            for (int i = 0; i < num_dice; i++)
            {
               result += Program.rand.Next(0,diceSize)+1; 
            }
            return result;
        }

        [Command("roll")]
        public async Task RollAsync(params string[] args)
        {
            string roll = string.Join("", args).Replace(" ", "");
            List<int> dice_results = new List<int>();

            DiceBag db = new DiceBag();

            if(roll.Contains('-') || roll.Contains('+'))
            {
                char[] splits = new char[] { '+', '-' };
                string[] two_parts = roll.Split(splits);
                int mod = 0;
                if(roll.Contains('-'))
                {
                    mod = -1* Convert.ToInt32(two_parts[1]);
                }
                else{
                    mod = Convert.ToInt32(two_parts[1]);
                }
                string[] parameters = two_parts[0].Split('d');
                uint num_dice = 1;
                int dice_sides = 2;

                if(parameters[0] != "")
                {
                    num_dice = Convert.ToUInt32(parameters[0]);
                    dice_sides = Convert.ToInt32(parameters[1]);
                }
                else{
                    dice_sides = Convert.ToInt32(parameters[1]);
                }

                DiceBag.Dice dice = (DiceBag.Dice)System.Enum.Parse(typeof(DiceBag.Dice), dice_sides.ToString());

                dice_results = db.RollQuantity( dice , num_dice );

                dice_results.Add(mod);
            }
            else{
                string[] parameters = roll.Split('d');
                uint num_dice = 1;
                int dice_sides = 2;

                if(parameters[0] != "")
                {
                    num_dice = Convert.ToUInt32(parameters[0]);
                    dice_sides = Convert.ToInt32(parameters[1]);
                }
                else{
                    dice_sides = Convert.ToInt32(parameters[1]);
                }

                DiceBag.Dice dice = (DiceBag.Dice)System.Enum.Parse(typeof(DiceBag.Dice), dice_sides.ToString());

                dice_results = db.RollQuantity( dice , num_dice );
                
            }
            

            SocketGuildUser usr = Context.Guild.GetUser(Context.Message.Author.Id) as SocketGuildUser;

            string rtn_name = usr.Nickname == null ? usr.Username : usr.Nickname;

            await ReplyAsync(rtn_name + " rolled a " + dice_results.Sum() + " (" + string.Join(", ",dice_results) + ")");
        }

        [Command("sector")]
        private async Task SectorAsync()
        {
            await ReplyAsync("https://sectorswithoutnumber.com/sector/m11ZXBOt6xiJGo21EKio");
        }

        private Boolean validate_vote(SocketUser user, vote vote)
        {
            Nacho nacho = new Nacho();

            if (nacho.get_rep(user.Username, Convert.ToUInt64(user.Discriminator)).FirstOrDefault() == null)
            {
                return false;
            }

            Nacho.representative rep = nacho.get_rep(user.Username, Convert.ToUInt64(user.Discriminator)).FirstOrDefault();

            if (rep.faction_text != vote.faction_name)
            {
                return false;
            }

            return true;

        }

    }

}