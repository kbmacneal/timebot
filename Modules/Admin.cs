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

        [Command("listfaction")]
        public async Task ListfactionAsync()
        {
            List<string> bad_requests = new List<string>();

            bad_requests.Add("everyone");
            bad_requests.Add("Houses Minor");
            bad_requests.Add("Houses Major");
            bad_requests.Add("Speaker");
            bad_requests.Add("Observer");
            bad_requests.Add("Moderator");

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            string rtn_message = String.Join(System.Environment.NewLine, roles.Where(e=>!bad_requests.Contains(e.Name)));

            await ReplyAsync(rtn_message);
        }

        [Command("addfaction")]
        public async Task AddfactionAsync(string faction)
        {
            List<string> bad_requests = new List<string>();

            bad_requests.Add("everyone");
            bad_requests.Add("Houses Minor");
            bad_requests.Add("Houses Major");
            bad_requests.Add("Speaker");
            bad_requests.Add("Observer");
            bad_requests.Add("Moderator");

            if(bad_requests.Any(faction.Contains))
            {
                await ReplyAsync("Invalid Request");
                return;
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            SocketGuildUser user = (SocketGuildUser)Context.User;

            if(roles.Where(e=>e.Name == faction).FirstOrDefault() == null)
            {
                await ReplyAsync("Faction selection not valid");
                return;
            }

            await user.AddRoleAsync(roles.Where(e=>e.Name == faction).FirstOrDefault(), null);

            await ReplyAsync("Role Added");
            return;
        }

    }

}