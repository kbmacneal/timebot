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

namespace timebot.Modules.Commands {

    public class Setup : ModuleBase<SocketCommandContext> {
        private Tuple<string, string>[] Factions = new Tuple<string, string>[] {
            Tuple.Create ("14 Red Dogs Triad", "#AD4641"),
            Tuple.Create ("ACRE", "#915A2D"),
            Tuple.Create ("Church of Humanity Repentant", "#227F97"),
            Tuple.Create ("High Church of the Messiah-as-Emperox", "#F1C40F"),
            Tuple.Create ("House Aquila", "#C2A77A"),
            Tuple.Create ("House Crux", "#7851A9"),
            Tuple.Create ("House Eridanus", "#070000"),
            Tuple.Create ("House Fornax", "#C27C0E"),
            Tuple.Create ("House Lyra", "#853C67"),
            Tuple.Create ("House Pyxis", "#E3A041"),
            Tuple.Create ("House Reticulum", "#B00000"),
            Tuple.Create ("House Serpens", "#009115"),
            Tuple.Create ("House Triangulum", "#7DB6FF"),
            Tuple.Create ("House Vela", "#1B75BC"),
            Tuple.Create ("PRISM", "#AB99B6"),
            Tuple.Create ("The Trilliant Ring", "#BBBBBB"),
            Tuple.Create ("The Deathless", "#8F5C5C"),
            Tuple.Create ("Unified People's Collective", "#89B951"),
            Tuple.Create ("\"House\" Vagrant", "#2F4CCA")
        };

        [Command ("initializeserver")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task InitializeserverAsync () {
            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            foreach (Tuple<string, string> faction in Factions) {
                if (!(roles.Select (e => e.Name).Contains (faction.Item1))) {
                    await Context.Guild.CreateRoleAsync (faction.Item1, null, null, false, null);
                }
            }

            await ReplyAsync ("Server initialized");

            await SetcolorsAsync ();

        }

        [Command ("setcolors")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task SetcolorsAsync () {

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            System.Drawing.ColorConverter converter = new System.Drawing.ColorConverter ();

            foreach (Tuple<string, string> Faction in Factions) {
                if (roles.Select (e => e.Name).Contains (Faction.Item1)) {
                    System.Drawing.Color colorhex = (System.Drawing.Color) converter.ConvertFromString (Faction.Item2);

                    await roles.Where (e => e.Name == Faction.Item1).FirstOrDefault ().ModifyAsync (r => r.Color = new Discord.Color (colorhex.R, colorhex.G, colorhex.B)).ConfigureAwait (false);
                }

            }

            await ReplyAsync ("Faction colors normalized.");
        }

        [Command ("setbotusername")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task SetBotUserName () {
            var guild = Context.Client.GetGuild (Context.Guild.Id);

            var user = guild.GetUser (Context.Client.CurrentUser.Id);

            await user.ModifyAsync (e => e.Nickname = "Arch Lector Frederick of Timebot", null);

            await Context.Client.SetStatusAsync (UserStatus.Online);

            await Context.Client.SetGameAsync ("World Domination", null, StreamType.NotStreaming);

            await ReplyAsync ("Username changed");
        }

    }

}