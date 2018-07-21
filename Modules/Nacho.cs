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

    public class NachoAdmin : ModuleBase<SocketCommandContext>
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
            "pilgrim",
            "Representative"

        };

        [Command("addrepresentative")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task AddrepresentativeAsync(string faction)
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

            SocketRole role = Context.Guild.Roles.Where(e=>e.Name == faction).FirstOrDefault();

            if(role == null)return;

            Nacho nacho = new Nacho();

            Nacho.representative rep = new Nacho.representative();

            rep.name = user.Username;
            rep.discriminator = Convert.ToUInt64(user.Discriminator);
            rep.faction_id = role.Id;
            rep.faction_text = role.Name;

            if((check_if_rep(rep)).GetAwaiter().GetResult() == true)
            {
                await ReplyAsync("You are already the representative for a faction. Run the tb!removerepresentative function then try again.");
                return;
            }

            if((check_if_already_represented(rep.faction_text)).GetAwaiter().GetResult() == true)
            {
                await ReplyAsync("The faction selected already has a representative. Please contact the mods if you have further questions.");
                return;
            }

            await nacho.assign_representative(rep);
            
            await user.AddRoleAsync(roles.Where(e => e.Name == "Representative").FirstOrDefault(), null);
            
            string message = "You have been added as the representative for " + faction;

            await ReplyAsync(message);
            
            return;
        }

        [Command("removerepresentative")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task RemoverepresentativeAsync(string faction)
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

            SocketRole role = Context.Guild.Roles.Where(e=>e.Name == faction).FirstOrDefault();

            if(role == null)return;

            Nacho nacho = new Nacho();

            Nacho.representative rep = new Nacho.representative();

            rep.name = user.Username;
            rep.discriminator = Convert.ToUInt64(user.Discriminator);
            rep.faction_id = role.Id;
            rep.faction_text = role.Name;

            await nacho.remove_rep(rep);

            await user.RemoveRoleAsync(roles.Where(e => e.Name == "Representative").FirstOrDefault(), null);
            
            string message = "You have been removed as the representative for " + faction;

            await ReplyAsync(message);
            
            return;
        }


        public async Task<Boolean> check_if_rep(Nacho.representative rep)
        {
            
            Nacho nacho = new Nacho();

            List<timebot.Classes.Nacho.representative> list = nacho.get_rep(rep.name, rep.discriminator).ToList();

            if(list.Count > 0)
            {
                return true;
            }
            else{
                return false;
            }


        }

        public async Task<Boolean> check_if_already_represented(string faction)
        {
            Nacho nacho = new Nacho();

            List<timebot.Classes.Nacho.representative> list = nacho.get_rep(faction).ToList();

            if(list.Count > 0)
            {
                return true;
            }
            else{
                return false;
            }
        }

    }

}