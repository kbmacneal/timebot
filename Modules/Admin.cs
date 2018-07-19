using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;
using Newtonsoft.Json;
using System.Threading;
using timebot.Classes;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace timebot.Modules.Commands
{

    public class Admin : ModuleBase<SocketCommandContext>
    {

        public string[] bad_requests { get; } =
        {
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
            "pilgrim"

        };

        [Command("listfaction")]
        public async Task ListfactionAsync()
        {
            List<string> bad_requests = this.bad_requests.ToList();

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            string rtn_message = String.Join(System.Environment.NewLine, roles.Where(e => !bad_requests.Contains(e.Name)).OrderBy(e => e.Name));

            await ReplyAsync(rtn_message);
        }

        [Command("addfaction")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task AddfactionAsync(string faction)
        {
            List<string> bad_requests = this.bad_requests.ToList();

            if (bad_requests.Any(faction.Contains))
            {
                await ReplyAsync("Invalid Request");
                return;
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            SocketGuildUser user = (SocketGuildUser)Context.User;

            if (roles.Where(e => e.Name == faction).FirstOrDefault() == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await user.AddRoleAsync(roles.Where(e => e.Name == faction).FirstOrDefault(), null);

            await ReplyAsync("Role Added");
            return;
        }

        [Command("removefaction")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemovefactionAsync(string faction)
        {
            List<string> bad_requests = this.bad_requests.ToList();

            if (bad_requests.Any(faction.Contains))
            {
                await ReplyAsync("Invalid Request");
                return;
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            SocketGuildUser user = (SocketGuildUser)Context.User;

            if (roles.Where(e => e.Name == faction).FirstOrDefault() == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await user.RemoveRoleAsync(roles.Where(e => e.Name == faction).FirstOrDefault(), null);

            await ReplyAsync("Role Removed");
            return;
        }

    }

}