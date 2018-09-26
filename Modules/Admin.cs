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

namespace timebot.Modules.Commands {
    public class Admin : ModuleBase<SocketCommandContext> {
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

        private static readonly Dictionary<string, string> blasters = new Dictionary<string, string> () { { "The High Church of Messiah-as-Emperox", "messiah" }
        };

        private static Boolean security_check (string faction, ulong id) {
            Boolean rtn = false;

            Classes.SwnbotResponse response = Classes.SwnbotResponseGet.GetResponse (id).GetAwaiter ().GetResult ();

            if (response == null) { rtn = true; } else {
                List<string> member_of = response.UserRoles.Select (e => e.RoleName).ToList ();

                if (member_of.Contains (faction)) rtn = true;
            }

            return rtn;

        }

        private static async Task<List<string>> faction_check (ulong id) {
            List<string> rtn = null;

            Classes.SwnbotResponse response = Classes.SwnbotResponseGet.GetResponse (id).GetAwaiter ().GetResult ();

            if (response == null) { rtn = null; } else {

                List<string> member_of = response.UserRoles.Select (e => e.RoleName).ToList ();

                rtn = member_of;
            }

            return rtn;

        }

        [Command ("sendfactionblast")]
        [RequireUserPermission (GuildPermission.Administrator)]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task SendfactionblastAsync (string faction, string message) {
            if (!(blasters.Keys.ToList ().Contains (faction))) {
                await ReplyAsync ("Your faction is not configured to receive bot blasts. Consult for rep.");

                return;
            }

            var users = Classes.FactionCount.FactionCountGet.GetCount (blasters[faction]).Members;

            foreach (var user in users) {
                RequestOptions opt = new RequestOptions ();
                opt.RetryMode = RetryMode.RetryRatelimit;
                var send = Context.Client.GetUser (Convert.ToUInt64 (user.User.Id));

                if (send != null) await send.SendMessageAsync (message, false, null, opt);
            }

            await ReplyAsync ("Messages Sent");
        }

        [Command ("getfactioncount")]
        [RequireUserPermission (GuildPermission.Administrator)]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task GetfactioncountAsync () {
            List<Classes.Faction> official_factions = Classes.Factions.get_factions ().apiFactions.ToList ();

            List<string> rtn = new List<string> ();

            Dictionary<string, string> holder = new Dictionary<string, string> ();

            official_factions.ForEach (e => holder.Add (e.FactionName, Classes.FactionCount.FactionCountGet.GetCount (e.FactionShortName).Members.Count ().ToString ()));

            rtn.Add ("Here are the counts of active members for each faction");
            rtn.Add ("---------------");
            holder.AsEnumerable ().ToList ().ForEach (e => rtn.Add (string.Concat (e.Key, ": ", e.Value)));

            await ReplyAsync (string.Join (System.Environment.NewLine, rtn));
        }

        [Command ("addtorightfaction")]
        [RequireUserPermission (GuildPermission.Administrator)]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task Addtorightfaction () {
            List<string> official_factions = Classes.Factions.get_factions ().apiFactions.ToList ().Select (e => e.FactionName).ToList ();

            List<string> additions = new List<string> ();

            List<SocketGuildUser> users = Context.Guild.Users.ToList ();

            RequestOptions opt = new RequestOptions ();
            opt.RetryMode = RetryMode.RetryRatelimit;

            foreach (var user in users) {
                if (user.IsBot) continue;

                List<string> member_of = faction_check (user.Id).GetAwaiter ().GetResult ();

                if (member_of == null) continue;

                foreach (var item in member_of) {
                    if (official_factions.Contains (item)) {
                        var role = Context.Guild.Roles.FirstOrDefault (e => e.Name == item);
                        if (!user.Roles.Select (e => e.Name).Contains (role.Name)) {
                            await user.AddRoleAsync (role, opt);
                            additions.Add (string.Concat (user.Username, "|", item));
                        }
                    }
                }

            }

            await ReplyAsync ("users added to a role" + System.Environment.NewLine + string.Join (System.Environment.NewLine, additions));
        }

        [Command ("cleanfaclists")]
        [RequireUserPermission (GuildPermission.Administrator)]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task CleanfaclistsAsync () {
            List<string> official_factions = Classes.Factions.get_factions ().apiFactions.ToList ().Select (e => e.FactionName).ToList ();

            List<string> infractions = new List<string> ();

            List<SocketGuildUser> users = Context.Guild.Users.ToList ();

            foreach (var user in users) {
                if (user.IsBot) continue;
                foreach (var role in user.Roles) {
                    if (!official_factions.Contains (role.Name)) continue;

                    if (!security_check (role.Name, user.Id)) {
                        infractions.Add (String.Concat (user.Username, "|", role.Name));
                        await user.RemoveRoleAsync (role, null);
                    }
                }
            }

            await ReplyAsync ("users removed from a role" + System.Environment.NewLine + string.Join (System.Environment.NewLine, infractions));
        }

        [Command ("cleanfaclists")]
        [RequireUserPermission (GuildPermission.Administrator)]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task CleanfaclistsAsync (SocketUser user) {
            List<string> official_factions = Classes.Factions.get_factions ().apiFactions.ToList ().Select (e => e.FactionName).ToList ();

            List<string> infractions = new List<string> ();

            if (user.IsBot) return;

            foreach (var role in ((SocketGuildUser) user).Roles) {
                if (!official_factions.Contains (role.Name)) continue;

                if (!security_check (role.Name, user.Id)) {
                    infractions.Add (String.Concat (user.Username, "|", role.Name));
                    await ((SocketGuildUser) user).RemoveRoleAsync (role, null);
                }
            }

            await ReplyAsync ("users removed from a role" + System.Environment.NewLine + string.Join (System.Environment.NewLine, infractions));
        }

        [Command ("listfaction")]
        public async Task ListfactionAsync () {
            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            List<string> official_factions = Classes.Factions.get_factions ().apiFactions.ToList ().Select (e => e.FactionName).ToList ();

            string rtn_message = String.Join (System.Environment.NewLine, roles.Where (e => official_factions.Contains (e.Name)).OrderBy (e => e.Name));

            await ReplyAsync (rtn_message);
        }

        [Command ("addfaction")]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task AddfactionAsync (params string[] args) {

            string faction = string.Join(" ", args);
            List<string> official_factions = Classes.Factions.get_factions ().apiFactions.ToList ().Select (e => e.FactionName).ToList ();

            if (!official_factions.Any (faction.Contains) && !optional_tags.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
            }

            if (!optional_tags.Contains (faction)) {
                if (!security_check (faction, Context.Message.Author.Id)) {
                    await ReplyAsync ("You are not a member of this faction. Please select the faction you are a part of on the main Far Verona Discord.");
                    return;
                }
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser user = (SocketGuildUser) Context.User;

            if (roles.FirstOrDefault (e => e.Name == faction) == null) {
                await ReplyAsync ("Faction selection not valid");
                return;
            }

            await user.AddRoleAsync (roles.FirstOrDefault (e => e.Name == faction), null);

            await ReplyAsync ("Role Added");
            return;
        }

        [Command ("addfaction")]
        [RequireUserPermission (GuildPermission.Administrator)]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task AddfactionAsync (SocketUser user, params string[] args) {
            string faction = string.Join(" ", args);
            List<string> official_factions = Classes.Factions.get_factions ().apiFactions.ToList ().Select (e => e.FactionName).ToList ();

            if (!official_factions.Any (faction.Contains) && !optional_tags.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
            }

            if (!optional_tags.Contains (faction)) {
                if (!security_check (faction, user.Id)) {
                    await ReplyAsync ("Target user not a member of this faction. Please select the faction they are a part of on the main Far Verona Discord.");
                    return;
                }
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser usr = (SocketGuildUser) user;

            if (roles.FirstOrDefault (e => e.Name == faction) == null) {
                await ReplyAsync ("Faction selection not valid");
                return;
            }

            await usr.AddRoleAsync (roles.FirstOrDefault (e => e.Name == faction), null);

            await ReplyAsync ("Role Added");
            return;
        }

        [Command ("removefaction")]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task RemovefactionAsync (string faction) {
            List<string> official_factions = Classes.Factions.get_factions ().apiFactions.ToList ().Select (e => e.FactionName).ToList ();

            if (!official_factions.Any (faction.Contains) && !optional_tags.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
            }

            if (!optional_tags.Contains (faction)) {
                if (!security_check (faction, Context.Message.Author.Id)) {
                    await ReplyAsync ("You are not a member of this faction. Please select the faction you are a part of on the main Far Verona Discord.");
                    return;
                }
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser user = (SocketGuildUser) Context.User;

            if (roles.FirstOrDefault (e => e.Name == faction) == null) {
                await ReplyAsync ("Faction selection not valid");
                return;
            }

            await user.RemoveRoleAsync (roles.FirstOrDefault (e => e.Name == faction), null);

            await ReplyAsync ("Role Removed");
            return;
        }

        [Command ("removefaction")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task RemovefactionAsync (SocketUser user, string faction) {
            List<string> official_factions = Classes.Factions.get_factions ().apiFactions.ToList ().Select (e => e.FactionName).ToList ();

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser usr = (SocketGuildUser) user;

            if (roles.FirstOrDefault (e => e.Name == faction) == null) {
                await ReplyAsync ("Faction selection not valid");
                return;
            }

            await usr.RemoveRoleAsync (roles.FirstOrDefault (e => e.Name == faction), null);

            await ReplyAsync ("Role Removed");
            return;
        }

        [Command ("removeentirefaction")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task RemoveentirefactionAsync (params string[] args) {

            string faction = string.Join(" ",args);
            
            SocketRole role = Context.Guild.Roles.FirstOrDefault(e=>e.Name == faction);

            if(role == null)
            {
                await ReplyAsync("Selection invalid");
                return;
            }

            RequestOptions opt = new RequestOptions{
                RetryMode = RetryMode.RetryRatelimit
            };

            List<SocketGuildUser> users = Context.Guild.Users.Where(e=>e.Roles.Contains(role)).ToList();

            users.ForEach(e=>e.KickAsync(null,opt));

            await ReplyAsync ("Users Removed");
            return;
        }

        [Command ("tracker")]
        public async Task TrackerAsync () {
            await ReplyAsync ("https://docs.google.com/spreadsheets/d/1QR078QvO5Q8S9gbQDglRhYK1HV3tBd0111SmjoVV0jQ/edit#gid=859451630");
        }

        [Command ("rulings")]
        public async Task RulingsAsync () {
            await ReplyAsync ("https://docs.google.com/document/d/1I34PlRnkl5Pzq9av9xWGXwY2Tzp7f5O9LQdlj0O0drc/edit");
        }

        [Command ("archivechannel")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task ArchivechannelAsync () {
            string date_archived = DateTime.Now.ToString ("yyyy-dd-M--HH-mm-ss");

            IEnumerable<IMessage> archive = await Context.Channel.GetMessagesAsync (Int32.MaxValue).Flatten ();

            var query =
                from msg in archive
            select new { msg.Author.Username, msg.Author.Discriminator, msg.Content, msg.CreatedAt, msg.EditedTimestamp, msg.Id, msg.Source, msg.Timestamp, msg.Attachments };

            string serialized = JsonConvert.SerializeObject (query);

            string path = Context.Channel.Name + " " + date_archived;

            System.IO.File.WriteAllText (path + ".json", serialized);

            await ReplyAsync ("Channel archived");

            await Context.Channel.SendMessageAsync ("Here is the archived file.");
            await Context.Channel.SendFileAsync (path + ".json");
        }

    }

}