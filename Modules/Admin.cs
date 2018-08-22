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

        private Dictionary<string,List<string>> gen_access_lists()
        {
            Dictionary<string,List<string>> rtn = new Dictionary<string, List<string>>();


            //New Room
            rtn.Add(
                "480767825368186911", new List<string>()
                {
                    "House Vagrant",
                    "14 Red Dogs Triad",
                    "ACRE",
                    "Church of Humanity Repentant",
                    "High Church of the Messiah-as-Emperox",
                    "House Aquila",
                    "House Crux",
                    "House Eridanus",
                    "House Fornax",
                    "House Lyra",
                    "House Pyxis",
                    "House Reticulum",
                    "House Serpens",
                    "House Triangulum",
                    "House Vela",
                    "PRISM",
                    "The Deathless",
                    "The Trilliant Ring",
                    "Unified People's Collective",
                    "War Room",
                    "Speaker",
                    "Observer"
                }
            );

            //IFDS
            rtn.Add(
                "465538179978756096", new List<string>()
                {
                    "\\\"House\\\" Vagrant",
                    "14 Red Dogs Triad",
                    "ACRE",
                    "Church of Humanity Repentant",
                    "High Church of the Messiah-as-Emperox",
                    "House Aquila",
                    "House Crux",
                    "House Eridanus",
                    "House Fornax",
                    "House Lyra",
                    "House Pyxis",
                    "House Reticulum",
                    "House Serpens",
                    "House Triangulum",
                    "House Vela",
                    "PRISM",
                    "The Deathless",
                    "The Trilliant Ring",
                    "Unified People's Collective",
                    "Speaker",
                    "Observer"
                }
            );

            //Meeting Room 1
            rtn.Add(
                "476147072526188546", new List<string>()
                {
                    "\\\"House\\\" Vagrant",
                    "14 Red Dogs Triad",
                    "ACRE",
                    "Church of Humanity Repentant",
                    "High Church of the Messiah-as-Emperox",
                    "House Aquila",
                    "House Crux",
                    "House Eridanus",
                    "House Fornax",
                    "House Lyra",
                    "House Pyxis",
                    "House Reticulum",
                    "House Serpens",
                    "House Triangulum",
                    "House Vela",
                    "PRISM",
                    "The Deathless",
                    "The Trilliant Ring",
                    "Unified People's Collective",
                    "Speaker",
                    "Observer"
                }
            );

            //Church Diplo Server
            rtn.Add(
                "435921918152146945", new List<string>()
                {
                    "\\\"House\\\" Vagrant",
                    "14 Red Dogs Triad",
                    "ACRE",
                    "Church of Humanity Repentant",
                    "High Church of the Messiah-as-Emperox",
                    "House Aquila",
                    "House Crux",
                    "House Eridanus",
                    "House Fornax",
                    "House Lyra",
                    "House Pyxis",
                    "House Reticulum",
                    "House Serpens",
                    "House Triangulum",
                    "House Vela",
                    "PRISM",
                    "The Deathless",
                    "The Trilliant Ring",
                    "Unified People's Collective"
                }
            );

            return rtn;
        }

        [Command ("listfaction")]
        public async Task ListfactionAsync () {
            List<string> valid_requests = gen_access_lists().FirstOrDefault(e=>e.Key == Context.Guild.Id.ToString()).Value;

            List<SocketRole> roles = Context.Guild.Roles.ToList ();

            string rtn_message = String.Join (System.Environment.NewLine, roles.Where (e => valid_requests.Contains (e.Name)).OrderBy (e => e.Name));

            await ReplyAsync (rtn_message);
        }

        [Command ("addfaction")]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task AddfactionAsync (string faction) {
            List<string> valid_requests = gen_access_lists().FirstOrDefault(e=>e.Key == Context.Guild.Id.ToString()).Value;

            if (!valid_requests.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
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
        public async Task AddfactionAsync (SocketUser user, string faction) {
            List<string> valid_requests = gen_access_lists().FirstOrDefault(e=>e.Key == Context.Guild.Id.ToString()).Value;

            if (!valid_requests.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
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
            List<string> valid_requests = gen_access_lists().FirstOrDefault(e=>e.Key == Context.Guild.Id.ToString()).Value;

            if (!valid_requests.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
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
            List<string> valid_requests = gen_access_lists().FirstOrDefault(e=>e.Key == Context.Guild.Id.ToString()).Value;

            if (!valid_requests.Any (faction.Contains)) {
                await ReplyAsync ("Invalid Request");
                return;
            }

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

        [Command ("stopbot")]
        [RequireBotPermission (GuildPermission.Administrator)]
        [RequireUserPermission (GuildPermission.Administrator)]
        public async Task StopbotAsync () {;
            await ReplyAsync ("The bot is shutting down.");
            Context.Client.LogoutAsync ().GetAwaiter ().GetResult ();
            Context.Client.StopAsync ().GetAwaiter ().GetResult ();
            Context.Client.Dispose ();
        }

        [Command("tracker")]
        public async Task TrackerAsync()
        {
            await ReplyAsync("https://docs.google.com/spreadsheets/d/1QR078QvO5Q8S9gbQDglRhYK1HV3tBd0111SmjoVV0jQ/edit#gid=859451630");}

        [Command("rulings")]
        public async Task RulingsAsync()
        {
            await ReplyAsync("https://docs.google.com/document/d/1I34PlRnkl5Pzq9av9xWGXwY2Tzp7f5O9LQdlj0O0drc/edit");
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