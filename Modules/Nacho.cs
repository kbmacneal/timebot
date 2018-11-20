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

    public class NachoAdmin : ModuleBase<SocketCommandContext> {
        public string[] factions { get; } = {
            "House Crux",
            "House Vela",
            "House Fornax",
            "The Houses Minor",
            "The High Church of Messiah-as-Emperox",
            "Moderator"

        };

        [Command ("addrepresentative")]
        [Summary("Adds a user as the representative of a faction.")]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task AddrepresentativeAsync (string faction) {
            if (!(factions.Contains (faction))) {
                await ReplyAsync ("Invalid Request");
                return;
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser user = (SocketGuildUser) Context.User;

            if (roles.FirstOrDefault (e => e.Name == faction) == null) {
                await ReplyAsync ("Faction selection not valid");
                return;
            }

            SocketRole role = Context.Guild.Roles.FirstOrDefault (e => e.Name == faction);

            if (role == null) return;

            Nacho nacho = new Nacho ();

            Nacho.representative rep = new Nacho.representative ();

            rep.name = user.Username;
            rep.discriminator = Convert.ToUInt64 (user.Discriminator);
            rep.faction_id = role.Id;
            rep.faction_text = role.Name;

            if ((check_if_rep (rep)).GetAwaiter ().GetResult () == true) {
                await ReplyAsync ("You are already the representative for a faction. Run the tb!removerepresentative function then try again.");
                return;
            }

            if ((check_if_already_represented (rep.faction_text)).GetAwaiter ().GetResult () == true) {
                await ReplyAsync ("The faction selected already has a representative. Please contact the mods if you have further questions.");
                return;
            }

            await nacho.assign_representative (rep);

            await user.AddRoleAsync (roles.FirstOrDefault (e => e.Name == "Representative (Mods)"), null);

            string message = "You have been added as the representative for " + faction;

            await ReplyAsync (message);

            return;
        }

        [Command ("addrepresentative")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [Summary("Adds a user as the representative of a faction.")]
        public async Task AddrepresentativeAsync (SocketUser usr, string faction) {
            if (!(factions.Contains (faction))) {
                await ReplyAsync ("Invalid Request");
                return;
            }

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser user = (SocketGuildUser) usr;

            if (roles.FirstOrDefault (e => e.Name == faction) == null) {
                await ReplyAsync ("Faction selection not valid");
                return;
            }

            SocketRole role = Context.Guild.Roles.FirstOrDefault (e => e.Name == faction);

            if (role == null) return;

            Nacho nacho = new Nacho ();

            Nacho.representative rep = new Nacho.representative ();

            rep.name = user.Username;
            rep.discriminator = Convert.ToUInt64 (user.Discriminator);
            rep.faction_id = role.Id;
            rep.faction_text = role.Name;

            if ((check_if_rep (rep)).GetAwaiter ().GetResult () == true) {
                await ReplyAsync ("You are already the representative for a faction. Run the tb!removerepresentative function then try again.");
                return;
            }

            if ((check_if_already_represented (rep.faction_text)).GetAwaiter ().GetResult () == true) {
                await ReplyAsync ("The faction selected already has a representative. Please contact the mods if you have further questions.");
                return;
            }

            await nacho.assign_representative (rep);

            await user.AddRoleAsync (roles.FirstOrDefault (e => e.Name == "Representative (Mods)"), null);

            string message = "You have been added as the representative for " + faction;

            await ReplyAsync (message);

            return;
        }

        [Command ("removerepresentative")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [Summary("Removes a user as a faction representative.")]
        public async Task RemoverepresentativeAsync () {
            Nacho nacho = new Nacho ();

            await nacho.remove_rep (Context.Message.Author);

            await ((SocketGuildUser) Context.Message.Author).RemoveRoleAsync (Context.Guild.Roles.FirstOrDefault (e => e.Name == "Representative"), null);

            await ReplyAsync ("You have been removed as the representative");
            return;
        }

        [Command ("removerepresentative")]
        [Summary("Removes a user as a faction representative.")]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task RemoverepresentativeAsync (SocketUser user) {

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            SocketGuildUser usr = (SocketGuildUser) user;

            Nacho nacho = new Nacho ();

            await nacho.remove_rep (user);

            await usr.RemoveRoleAsync (roles.FirstOrDefault (e => e.Name == "Representative"), null);

            string message = "User has been removed as a Representative";

            await ReplyAsync (message);

            return;
        }

        public async Task<Boolean> check_if_rep (Nacho.representative rep) {

            Nacho nacho = new Nacho ();

            List<timebot.Classes.Nacho.representative> list = nacho.get_rep (rep.name, rep.discriminator).ToList ();

            if (list.Count > 0) {
                return true;
            } else {
                return false;
            }

        }

        public async Task<Boolean> check_if_already_represented (string faction) {
            Nacho nacho = new Nacho ();

            List<timebot.Classes.Nacho.representative> list = nacho.get_rep (faction).ToList ();

            if (list.Count > 0) {
                return true;
            } else {
                return false;
            }
        }

    }

}