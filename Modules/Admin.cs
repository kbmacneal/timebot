using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using RestSharp;
using timebot.Classes;
using NodaTime;

namespace timebot.Modules.Commands
{

    public class infractions
    {
        public string name { get; set; }
        public string faction { get; set; }
    }
    public class Admin : ModuleBase<SocketCommandContext>
    {
        private static readonly string[] optional_tags = {
            "Speaker",
            "Observer",
            "War Room",
            "Houses Major",
            "Houses Minor",
            "Churches",
            "Moderator",
            "Representative (Mods)",
            "Bots",
            "Corporates",
            "NACHO",
            "TrilliantMeeting"
        };

        private static readonly Dictionary<string, string> blasters = new Dictionary<string, string>() { { "The High Church of Messiah-as-Emperox", "messiah" }
        };

        private static Boolean security_check(string faction, ulong id)
        {
            Boolean rtn = false;

            Classes.SwnbotResponse response = Classes.SwnbotResponseGet.GetResponse(id).GetAwaiter().GetResult();

            if (response == null) { rtn = true; }
            else
            {
                List<string> member_of = response.UserRoles.Select(e => e.FactionName).ToList();

                // if (member_of.Contains (faction)) rtn = true;

                if (member_of.Where(e => String.Compare(faction, e, true) == 0).Count() > 0) rtn = true;
            }

            return rtn;

        }

        private static async Task<List<string>> faction_check(ulong id)
        {
            List<string> rtn = null;

            Classes.SwnbotResponse response = Classes.SwnbotResponseGet.GetResponse(id).GetAwaiter().GetResult();

            if (response == null) { rtn = null; }
            else
            {

                List<string> member_of = response.UserRoles.Select(e => e.FactionName).ToList();

                rtn = member_of;
            }

            return rtn;

        }

        [Command("sendfactionblast")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Sends a message to every member of a faction that the bot can see.")]
        public async Task SendfactionblastAsync(string faction, string message)
        {
            if (!(blasters.Keys.ToList().Contains(faction)))
            {
                await ReplyAsync("Your faction is not configured to receive bot blasts. Consult for rep.");

                return;
            }

            var users = Classes.FactionCount.FactionCountGet.GetCount(blasters[faction]).Members;

            foreach (var user in users)
            {
                RequestOptions opt = new RequestOptions();
                opt.RetryMode = RetryMode.RetryRatelimit;
                var send = Context.Client.GetUser(Convert.ToUInt64(user.User.Id));

                if (send != null) await send.SendMessageAsync(message, false, null, opt);
            }

            await ReplyAsync("Messages Sent");
        }

        public class stats
        {
            public List<faction_stat> membershipstring { get; set; }
            public string api_key { get; set; }
        }
        public class faction_stat
        {
            public string name {get;set;}
            public int count{get;set;}
        }

        [Command("getfactioncount")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Gets the realtime count of all members of a faction from the SWNBot API.")]
        public async Task GetfactioncountAsync()
        {
            List<Classes.Faction> official_factions = Classes.Factions.get_factions().apiFactions.ToList();

            List<string> rtn = new List<string>();

            Dictionary<string, string> holder = new Dictionary<string, string>();

            official_factions.ForEach(e => holder.Add(e.FactionName, Classes.FactionCount.FactionCountGet.GetCount(e.FactionShortName).Members.Count().ToString()));

            rtn.Add("Here are the counts of active members for each faction");
            rtn.Add("---------------");
            holder.AsEnumerable().ToList().ForEach(e => rtn.Add(string.Concat(e.Key, ": ", e.Value)));

            Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(Program.secrets_file));

            string key = secrets["api_key"];

            List<faction_stat> sender = new List<faction_stat>();

            holder.ToList().ForEach(e=>sender.Add(new faction_stat(){name = e.Key, count = Int32.Parse(e.Value)}));

            stats s = new stats
            {
                membershipstring = sender.OrderByDescending(e=>e.count).ToList(),
                api_key = key
            };

            string baseurl = string.Concat("https://highchurch.space/api/UpdateMemCount");
            // string baseurl = string.Concat ("http://localhost:5000/api/UpdateMemCount");

            var client = new RestClient(baseurl);

            var request = new RestRequest(Method.POST);
            request.AddParameter("text/json", JsonConvert.SerializeObject(s), ParameterType.RequestBody);

            request.AddHeader("Content-Type", "text/json");

            var response = client.Execute(request);

            await ReplyAsync(string.Join(System.Environment.NewLine, rtn));
        }

        [Command("monthlychanges")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("A command that runs both the cleanfaclists command and the addtorightfaction commands one after the other")]
        public async Task MonthlychangesAsync()
        {

            await CleanfaclistsAsync();
            await Addtorightfaction();
        }

        [Command("addtorightfaction")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Adds a user to the faction they are a part of in the main server.")]
        public async Task Addtorightfaction()
        {
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            List<infractions> additions = new List<infractions>();

            List<SocketGuildUser> users = Context.Guild.Users.ToList();

            RequestOptions opt = new RequestOptions();
            opt.RetryMode = RetryMode.RetryRatelimit;

            foreach (var user in users)
            {
                if (user.IsBot) continue;

                List<string> member_of = faction_check(user.Id).GetAwaiter().GetResult();

                if (member_of == null) continue;

                foreach (var item in member_of)
                {
                    if (official_factions.Contains(item))
                    {
                        var role = Context.Guild.Roles.FirstOrDefault(e => e.Name == item);
                        if (role == null) continue;
                        if (!user.Roles.Select(e => e.Name).Contains(role.Name))
                        {
                            await user.AddRoleAsync(role, opt);
                            additions.Add(new Modules.Commands.infractions { name = user.Username, faction = role.Name });
                        }
                    }
                }

            }

            await ReplyAsync("users added to a role" + System.Environment.NewLine + additions.ToStringTable(new[] { "Name", "Faction" }, a => a.name, a => a.faction));
        }

        [Command("addtorightfaction")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Adds a user to the faction they are a part of in the main server.")]
        public async Task Addtorightfaction(SocketGuildUser user)
        {
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            List<infractions> additions = new List<infractions>();

            RequestOptions opt = new RequestOptions();
            opt.RetryMode = RetryMode.RetryRatelimit;

            if (user.IsBot) return;

            List<string> member_of = faction_check(user.Id).GetAwaiter().GetResult();

            if (member_of == null) return;

            foreach (var item in member_of)
            {
                if (official_factions.Contains(item))
                {
                    var role = Context.Guild.Roles.FirstOrDefault(e => e.Name == item);
                    if (role == null) continue;
                    if (!user.Roles.Select(e => e.Name).Contains(role.Name))
                    {
                        await user.AddRoleAsync(role, opt);
                        additions.Add(new Modules.Commands.infractions { name = user.Username, faction = role.Name });
                    }
                }
            }

            await ReplyAsync("users added to a role" + System.Environment.NewLine + additions.ToStringTable(new[] { "Name", "Faction" }, a => a.name, a => a.faction));
        }

        [Command("cleanfaclists")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Can be run either against everyone on a server or against a specific user (at the user as a parameter to use the latter). Will check and make sure the faction roles match with the main FV server.")]
        public async Task CleanfaclistsAsync()
        {
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            List<infractions> infractions = new List<infractions>();

            List<SocketGuildUser> users = Context.Guild.Users.ToList();

            foreach (var user in users)
            {
                if (user.IsBot) continue;
                foreach (var role in user.Roles)
                {
                    if (!official_factions.Contains(role.Name)) continue;

                    if (!security_check(role.Name, user.Id))
                    {
                        infractions.Add(new Modules.Commands.infractions { name = user.Username, faction = role.Name });
                        await user.RemoveRoleAsync(role, null);
                    }
                }
            }

            await ReplyAsync("users removed from a role" + System.Environment.NewLine + infractions.ToStringTable(new[] { "Name", "Faction" }, a => a.name, a => a.faction));
        }

        [Command("cleanfaclists")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Can be run either against everyone on a server or against a specific user (at the user as a parameter to use the latter). Will check and make sure the faction roles match with the main FV server.")]
        public async Task CleanfaclistsAsync(SocketUser user)
        {
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            List<infractions> infractions = new List<infractions>();

            if (user.IsBot) return;

            foreach (var role in ((SocketGuildUser)user).Roles)
            {
                if (!official_factions.Contains(role.Name)) continue;

                if (!security_check(role.Name, user.Id))
                {
                    infractions.Add(new Modules.Commands.infractions { name = user.Username, faction = role.Name });
                    await ((SocketGuildUser)user).RemoveRoleAsync(role, null);
                }
            }

            await ReplyAsync("users removed from a role" + System.Environment.NewLine + infractions.ToStringTable(new[] { "Name", "Faction" }, a => a.name, a => a.faction));
        }

        [Command("listfaction")]
        [Summary("Returns the list of all factions available to the addfaction command.")]
        public async Task ListfactionAsync()
        {
            List<SocketRole> roles = Context.Guild.Roles.ToList();

            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            string rtn_message = String.Join(System.Environment.NewLine, roles.Where(e => official_factions.Contains(e.Name)).OrderBy(e => e.Name));

            await ReplyAsync(rtn_message);
        }

        [Command("addfaction")]
        [Summary("Adds the sender to the selected faction. Administrators can at an individual then specify the faction and the bot will add the user to the faction. This is the preferred way to add, since the bot with check with the FV server to make sure that the user is a part of that faction.")]
        public async Task AddfactionAsync(params string[] args)
        {

            string faction = string.Join(" ", args);
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            // if (!official_factions.Any (faction.Contains) && !optional_tags.Any (faction.Contains)) {
            //     await ReplyAsync ("Invalid Request");
            //     return;
            // }

            if (official_factions.Where(e => String.Compare(faction, e, true) == 0).Count() == 0 && optional_tags.Where(e => String.Compare(faction, e, true) == 0).Count() != 0)
            {
                await ReplyAsync("Invalid Request");
                return;
            }

            if (!optional_tags.Contains(faction))
            {
                if (!security_check(faction, Context.Message.Author.Id))
                {
                    await ReplyAsync("You are not a member of this faction. Please select the faction you are a part of on the main Far Verona Discord.");
                    return;
                }
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            SocketGuildUser user = (SocketGuildUser)Context.User;

            if (roles.FirstOrDefault(e => String.Compare(faction, e.Name, true) == 0) == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await user.AddRoleAsync(roles.FirstOrDefault(e => String.Compare(faction, e.Name, true) == 0), null);

            await ReplyAsync("Role Added");
            return;
        }

        [Command("addfaction")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Adds a user to the selected faction.")]
        public async Task AddfactionAsync(SocketUser user, params string[] args)
        {
            string faction = string.Join(" ", args);
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            // if (!official_factions.Any (faction.Contains) && !optional_tags.Any (faction.Contains)) {
            //     await ReplyAsync ("Invalid Request");
            //     return;
            // }

            if (official_factions.Where(e => String.Compare(faction, e, true) == 0).Count() == 0 && optional_tags.Where(e => String.Compare(faction, e, true) == 0).Count() != 0)
            {
                await ReplyAsync("Invalid Request");
                return;
            }

            if (!optional_tags.Contains(faction))
            {
                if (!security_check(faction, user.Id))
                {
                    await ReplyAsync("Target user not a member of this faction. Please select the faction they are a part of on the main Far Verona Discord.");
                    return;
                }
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            SocketGuildUser usr = (SocketGuildUser)user;

            if (roles.FirstOrDefault(e => String.Compare(faction, e.Name, true) == 0) == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await usr.AddRoleAsync(roles.FirstOrDefault(e => String.Compare(faction, e.Name, true) == 0), null);

            await ReplyAsync("Role Added");
            return;
        }

        [Command("removefaction")]
        [Summary("Exactly like the addfaction command, but removes. Has the same at user overload.")]
        public async Task RemovefactionAsync(params string[] args)
        {
            string faction = string.Join(" ", args);
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            // if (!official_factions.Any (faction.Contains) && !optional_tags.Any (faction.Contains)) {
            //     await ReplyAsync ("Invalid Request");
            //     return;
            // }

            if (official_factions.Where(e => String.Compare(faction, e, true) == 0).Count() == 0 && optional_tags.Where(e => String.Compare(faction, e, true) == 0).Count() != 0)
            {
                await ReplyAsync("Invalid Request");
                return;
            }

            if (!optional_tags.Contains(faction))
            {
                if (!security_check(faction, Context.Message.Author.Id))
                {
                    await ReplyAsync("You are not a member of this faction. Please select the faction you are a part of on the main Far Verona Discord.");
                    return;
                }
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            SocketGuildUser user = (SocketGuildUser)Context.User;

            if (roles.FirstOrDefault(e => String.Compare(faction, e.Name, true) == 0) == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await user.RemoveRoleAsync(roles.FirstOrDefault(e => String.Compare(faction, e.Name, true) == 0), null);

            await ReplyAsync("Role Removed");
            return;
        }

        [Command("removefaction")]
        [Summary("Exactly like the addfaction command, but removes. Has the same at user overload.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemovefactionAsync(SocketUser user, params string[] args)
        {
            string faction = string.Join(" ", args);
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            // if (!official_factions.Any (faction.Contains) && !optional_tags.Any (faction.Contains)) {
            //     await ReplyAsync ("Invalid Request");
            //     return;
            // }

            if (official_factions.Where(e => String.Compare(faction, e, true) == 0).Count() == 0 && optional_tags.Where(e => String.Compare(faction, e, true) == 0).Count() != 0)
            {
                await ReplyAsync("Invalid Request");
                return;
            }

            if (!optional_tags.Contains(faction))
            {
                if (!security_check(faction, user.Id))
                {
                    await ReplyAsync("Target user not a member of this faction. Please select the faction they are a part of on the main Far Verona Discord.");
                    return;
                }
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            SocketGuildUser usr = (SocketGuildUser)user;

            if (roles.FirstOrDefault(e => String.Compare(faction, e.Name, true) == 0) == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await usr.AddRoleAsync(roles.FirstOrDefault(e => String.Compare(faction, e.Name, true) == 0), null);

            await ReplyAsync("Role Removed");
            return;
        }

        [Command("removeentirefaction")]
        [Summary("Kicks and entire faction from the server")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveentirefactionAsync(params string[] args)
        {

            string faction = string.Join(" ", args);

            SocketRole role = Context.Guild.Roles.FirstOrDefault(e => e.Name == faction);

            if (role == null)
            {
                await ReplyAsync("Selection invalid");
                return;
            }

            RequestOptions opt = new RequestOptions
            {
                RetryMode = RetryMode.RetryRatelimit
            };

            List<SocketGuildUser> users = Context.Guild.Users.Where(e => e.Roles.Contains(role)).ToList();

            foreach (var user in users)
            {
                await user.KickAsync(null, opt);
            }

            await ReplyAsync("Users Removed");
            return;
        }

        [Command("tracker")]
        [Summary("Pastes the link to the faction tracker in chat.")]
        public async Task TrackerAsync()
        {
            await ReplyAsync("https://docs.google.com/spreadsheets/d/1QR078QvO5Q8S9gbQDglRhYK1HV3tBd0111SmjoVV0jQ/edit#gid=859451630");
        }

        [Command("rulings")]
        [Summary("Pastes the link to the rulings doc in chat.")]
        public async Task RulingsAsync()
        {
            await ReplyAsync("https://docs.google.com/document/d/1I34PlRnkl5Pzq9av9xWGXwY2Tzp7f5O9LQdlj0O0drc/edit");
        }

        [Command("archivechannel")]
        [Summary("Dumps a json log of the channel into chat.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ArchivechannelAsync()
        {
            string date_archived = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

            IEnumerable<IMessage> archive =  (Context.Channel.GetMessagesAsync(Int32.MaxValue).Flatten()).ToEnumerable();

            var query =
                from msg in archive
                select new { msg.Author.Username, msg.Author.Discriminator, msg.Content, msg.CreatedAt, msg.EditedTimestamp, msg.Id, msg.Source, msg.Timestamp, msg.Attachments };

            string serialized = JsonConvert.SerializeObject(query);

            string path = Context.Channel.Name + " " + date_archived;

            System.IO.File.WriteAllText(path + ".json", serialized);

            await ReplyAsync("Channel archived");

            await Context.Channel.SendMessageAsync("Here is the archived file.");
            await Context.Channel.SendFileAsync(path + ".json");
        }

        [Command("dumpserverchat")]
        [Summary("Gets the chat of every channel in the server in a separate json file and spits out the result")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DumpserverchatAsync()
        {
            if (!System.IO.Directory.Exists(Context.Guild.Name))
            {
                System.IO.Directory.CreateDirectory(Context.Guild.Name);
            }

            var path = Context.Guild.Name;

            foreach (var item in Context.Guild.TextChannels)
            {
                string date_archived = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

                // SocketTextChannel temp = Context.Guild.GetTextChannel(item.Id);
                // if(temp == null)continue;

                IEnumerable<IMessage> archive = item.GetMessagesAsync(Int32.MaxValue).Flatten().ToEnumerable();

                // IEnumerable<IMessage> archive = await Context.Channel.GetMessagesAsync(Int32.MaxValue).Flatten();

                var query =
                    from msg in archive
                    select new { msg.Author.Username, msg.Author.Discriminator, msg.Content, msg.CreatedAt, msg.EditedTimestamp, msg.Id, msg.Source, msg.Timestamp, msg.Attachments };

                string serialized = JsonConvert.SerializeObject(query);

                string filepath = System.IO.Path.Join(path, item.Name + " " + date_archived + ".json");

                System.IO.File.WriteAllText(filepath, serialized);
            }

            System.IO.Compression.ZipFile.CreateFromDirectory(path, path + ".zip");
            await Context.Channel.SendMessageAsync("Here is the archived file.");
            await Context.Channel.SendFileAsync(path + ".zip");

            System.IO.Directory.Delete(path, true);
            System.IO.File.Delete(path + ".zip");
        }

        [Command("startnewyearscountdown")]
        [Summary("Starts the around-the-world new years countdown")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task StartnewyearscountdownAsync()
        {
            var event_list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Times>>(System.IO.File.ReadAllText("new_years.json"));

            int int_test = 0;

            foreach (var time in event_list)
            {
                Instant now = SystemClock.Instance.GetCurrentInstant();

                ZonedDateTime now_zoned = new ZonedDateTime(now, DateTimeZoneProviders.Tzdb.GetSystemDefault());

                NodaTime.ZonedDateTime dt = new NodaTime.ZonedDateTime(Instant.FromDateTimeOffset(new DateTimeOffset(DateTime.Parse(time.CentralStandardTime))), NodaTime.DateTimeZone.ForOffset(NodaTime.Offset.FromHours(-6)), NodaTime.CalendarSystem.Gregorian);

                Duration interval = dt - now_zoned;

                long long_interval = Convert.ToInt64(interval.TotalMilliseconds);

                if (!(Int32.TryParse(long_interval.ToString(), out int_test)))
                {
                    await ReplyAsync("Cannot schedule now, number of milliseconds until timer conclusion too large.");
                    return;
                }
            }

            var paste = await Classes.new_years.schedule_times(Context);

            await ReplyAsync(String.Join(System.Environment.NewLine, paste), false, null, null);
        }

    }

}