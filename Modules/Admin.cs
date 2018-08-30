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
using timebot.Classes;
using RestSharp;

namespace timebot.Modules.Commands
{
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

        private static Boolean security_check(string faction, ulong id)
        {
            Boolean rtn = false;

            Classes.SwnbotResponse response = Classes.SwnbotResponseGet.GetResponse(id);

            List<string> member_of = response.UserRoles.Select(e => e.RoleName).ToList();

            if (member_of.Contains(faction)) rtn = true;

            return rtn;

        }

        [Command("getfactioncount")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task GetfactioncountAsync()
        {
            List<Classes.Faction> official_factions = Classes.Factions.get_factions().apiFactions.ToList();

            List<string> rtn = new List<string>();

            Dictionary<string,string> holder = new Dictionary<string,string>();

            official_factions.ForEach(e=>holder.Add(e.FactionName,Classes.FactionCount.FactionCountGet.GetCount(e.FactionShortName).Members.Count().ToString()));

            rtn.Add("Here are the counts of active members for each faction");
            rtn.Add("---------------");
            holder.AsEnumerable().ToList().ForEach(e=>rtn.Add(string.Concat(e.Key, ": ",e.Value)));
            
            await ReplyAsync(string.Join(System.Environment.NewLine,rtn));
        }

        [Command("cleanfaclists")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task CleanfaclistsAsync()
        {
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            List<string> infractions = new List<string>();

            List<SocketGuildUser> users = Context.Guild.Users.ToList();

            foreach (var user in users)
            {
                if (user.IsBot) continue;
                foreach (var role in user.Roles)
                {
                    if (!official_factions.Contains(role.Name)) continue;

                    if (!security_check(role.Name, user.Id))
                    {
                        infractions.Add(user.Nickname);
                        await user.RemoveRoleAsync(role, null);
                    }
                }
            }

            await ReplyAsync("users removed from a role" + System.Environment.NewLine + string.Join(System.Environment.NewLine, infractions));
        }

        [Command("listfaction")]
        public async Task ListfactionAsync()
        {
            List<SocketRole> roles = Context.Guild.Roles.ToList();

            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            string rtn_message = String.Join(System.Environment.NewLine, roles.Where(e => official_factions.Contains(e.Name)).OrderBy(e => e.Name));

            await ReplyAsync(rtn_message);
        }

        [Command("addfaction")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task AddfactionAsync(string faction)
        {
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            if (!official_factions.Any(faction.Contains) && !optional_tags.Any(faction.Contains))
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

            if (roles.FirstOrDefault(e => e.Name == faction) == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await user.AddRoleAsync(roles.FirstOrDefault(e => e.Name == faction), null);

            await ReplyAsync("Role Added");
            return;
        }

        [Command("addfaction")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task AddfactionAsync(SocketUser user, string faction)
        {
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            if (!official_factions.Any(faction.Contains) && !optional_tags.Any(faction.Contains))
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

            if (roles.FirstOrDefault(e => e.Name == faction) == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await usr.AddRoleAsync(roles.FirstOrDefault(e => e.Name == faction), null);

            await ReplyAsync("Role Added");
            return;
        }

        [Command("removefaction")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task RemovefactionAsync(string faction)
        {
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            if (!official_factions.Any(faction.Contains) && !optional_tags.Any(faction.Contains))
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

            if (roles.FirstOrDefault(e => e.Name == faction) == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await user.RemoveRoleAsync(roles.FirstOrDefault(e => e.Name == faction), null);

            await ReplyAsync("Role Removed");
            return;
        }

        [Command("removefaction")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemovefactionAsync(SocketUser user, string faction)
        {
            List<string> official_factions = Classes.Factions.get_factions().apiFactions.ToList().Select(e => e.FactionName).ToList();

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            SocketGuildUser usr = (SocketGuildUser)user;

            if (roles.FirstOrDefault(e => e.Name == faction) == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await usr.RemoveRoleAsync(roles.FirstOrDefault(e => e.Name == faction), null);

            await ReplyAsync("Role Removed");
            return;
        }

        [Command("tracker")]
        public async Task TrackerAsync()
        {
            await ReplyAsync("https://docs.google.com/spreadsheets/d/1QR078QvO5Q8S9gbQDglRhYK1HV3tBd0111SmjoVV0jQ/edit#gid=859451630");
        }

        [Command("rulings")]
        public async Task RulingsAsync()
        {
            await ReplyAsync("https://docs.google.com/document/d/1I34PlRnkl5Pzq9av9xWGXwY2Tzp7f5O9LQdlj0O0drc/edit");
        }

        [Command("archivechannel")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ArchivechannelAsync()
        {
            string date_archived = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

            IEnumerable<IMessage> archive = await Context.Channel.GetMessagesAsync(Int32.MaxValue).Flatten();

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

    }

}