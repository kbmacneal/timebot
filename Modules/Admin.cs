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

namespace timebot.Modules.Commands {

    public class Admin : ModuleBase<SocketCommandContext> {

        public string[] bad_requests { get; } = {
            "@everyone",
            "Moderator",
            "Timebot",
            "Bots",
            "admin",
            "church_member",
            "inquisitor",
            "churchbot",
            "new role",
            "OOC a human don't forget it",
            "pilgrim",
            "Representative",
            "shame",
            "NACHO",
            "Locked"

        };

        [Command ("listfaction")]
        public async Task ListfactionAsync () {
            List<string> bad_requests = this.bad_requests.ToList ();

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            string rtn_message = String.Join (System.Environment.NewLine, roles.Where (e => !bad_requests.Contains (e.Name)).OrderBy (e => e.Name));

            await ReplyAsync (rtn_message);
        }

        [Command ("addfaction")]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task AddfactionAsync (string faction) {
            List<string> bad_requests = this.bad_requests.ToList ();

            if (bad_requests.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser user = (SocketGuildUser) Context.User;

            if (roles.Where (e => e.Name == faction).FirstOrDefault () == null) {
                await ReplyAsync ("Faction selection not valid");
                return;
            }

            await user.AddRoleAsync (roles.Where (e => e.Name == faction).FirstOrDefault (), null);

            await ReplyAsync ("Role Added");
            return;
        }

        [Command ("addfaction")]
        [RequireUserPermission (GuildPermission.Administrator)]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task AddfactionAsync (SocketUser user, string faction) {
            List<string> bad_requests = this.bad_requests.ToList ();

            if (bad_requests.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser usr = (SocketGuildUser)user;

            if (roles.Where (e => e.Name == faction).FirstOrDefault () == null) {
                await ReplyAsync ("Faction selection not valid");
                return;
            }

            await usr.AddRoleAsync (roles.Where (e => e.Name == faction).FirstOrDefault (), null);

            await ReplyAsync ("Role Added");
            return;
        }

        [Command ("removefaction")]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task RemovefactionAsync (string faction) {
            List<string> bad_requests = this.bad_requests.ToList ();

            if (bad_requests.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser user = (SocketGuildUser) Context.User;

            if (roles.Where (e => e.Name == faction).FirstOrDefault () == null) {
                await ReplyAsync ("Faction selection not valid");
                return;
            }

            await user.RemoveRoleAsync (roles.Where (e => e.Name == faction).FirstOrDefault (), null);

            await ReplyAsync ("Role Removed");
            return;
        }

        [Command ("removefaction")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task RemovefactionAsync (SocketUser user, string faction) {
            List<string> bad_requests = this.bad_requests.ToList ();

            if (bad_requests.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser usr = (SocketGuildUser)user;

            if (roles.Where (e => e.Name == faction).FirstOrDefault () == null) {
                await ReplyAsync ("Faction selection not valid");
                return;
            }

            await usr.RemoveRoleAsync (roles.Where (e => e.Name == faction).FirstOrDefault (), null);

            await ReplyAsync ("Role Removed");
            return;
        }

        [Command ("stopbot")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task StopbotAsync () {;
            await ReplyAsync ("The bot is shutting down.");
            Context.Client.LogoutAsync ().GetAwaiter ().GetResult ();
            Context.Client.StopAsync ().GetAwaiter ().GetResult ();
            Context.Client.Dispose ();
        }

        [Command ("archivechannel")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
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

            await Context.User.SendMessageAsync("Here is the archived file.");
            await Context.Channel.SendFileAsync(path);
        }

    }

}