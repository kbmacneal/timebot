using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace timebot.Modules.Commands
{
    public class Setup : ModuleBase<SocketCommandContext>
    {
        private Tuple<string, string>[] Factions = new Tuple<string, string>[] {
            Tuple.Create ("SERAPH", "#227F97"),
            Tuple.Create ("The High Church of Messiah-as-Emperox", "#F1C40F"),
            Tuple.Create ("House Aquila", "#C2A77A"),
            Tuple.Create ("House Crux", "#7851A9"),
            Tuple.Create ("House Eridanus", "#070000"),
            Tuple.Create ("House Fornax", "#C27C0E"),
            Tuple.Create ("House Lyra", "#853C67"),
            Tuple.Create ("House Pyxis", "#E3A041"),
            Tuple.Create ("House Reticulum", "#B00000"),
            Tuple.Create ("House Serpens", "#009115"),
            Tuple.Create ("House Triangulum", "#7DB6FF"),
            Tuple.Create ("The Prism Network", "#AB99B6"),
            Tuple.Create ("The Trilliant Ring", "#FFDFF00"),
            Tuple.Create ("The Deathless", "#8F5C5C"),
            Tuple.Create ("Confederation of the Upright Vagrant", "#2F4CCA"),
            Tuple.Create ("Democratic Mandate", "#147162")
        };

        [Command("setcolors")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Initializes the factions with their correct, official colors.")]
        public async Task SetcolorsAsync()
        {
            SocketRole role = null;

            List<SocketRole> roles = Context.Guild.Roles.ToList();

            System.Drawing.ColorConverter converter = new System.Drawing.ColorConverter();

            foreach (Tuple<string, string> Faction in Factions)
            {
                System.Drawing.Color colorhex = (System.Drawing.Color)converter.ConvertFromString(Faction.Item2);

                if (roles.Select(e => e.Name).Contains(Faction.Item1))
                {
                    await roles.Where(e => e.Name == Faction.Item1).FirstOrDefault().ModifyAsync(r => r.Color = new Discord.Color(colorhex.R, colorhex.G, colorhex.B)).ConfigureAwait(false);

                    role = Context.Guild.GetRole(roles.FirstOrDefault(e => e.Name == Faction.Item1).Id);
                }
                else
                {
                    ulong id = (await Context.Guild.CreateRoleAsync(Faction.Item1, null, new Discord.Color(colorhex.R, colorhex.G, colorhex.B), false, null)).Id;

                    role = Context.Guild.GetRole(id);
                }
            }

            await ReplyAsync("Faction colors normalized.");
        }

        private async Task setup_channels_Async()
        {
            List<string> facs = new List<string>();
            Factions.ToList().ForEach(e => facs.Add(e.Item1));

            foreach (string fac in facs)
            {
                var role = Context.Guild.Roles.Where(e => e.Name == fac);
                var channel = await Context.Guild.CreateTextChannelAsync(fac, null);

                IRole everyone = Context.Guild.EveryoneRole as IRole;

                OverwritePermissions everyone_perms = new OverwritePermissions();

                everyone_perms.Modify(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, null, null, null, null, PermValue.Deny, null, PermValue.Deny, PermValue.Deny);

                OverwritePermissions roleperms = new OverwritePermissions();

                roleperms.Modify(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, null, null, null, null, PermValue.Deny, null, PermValue.Deny, PermValue.Deny);

                await channel.AddPermissionOverwriteAsync(everyone, everyone_perms, null);
                // await channel.AddPermissionOverwriteAsync(role,roleperms,null);
            }
        }
    }
}