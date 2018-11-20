using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using timebot.Classes;

namespace timebot.Modules.Commands {

    public class Proposals : ModuleBase<SocketCommandContext> {

        [Command ("propose")]
        [Summary("Propose a topic for a meeting to the other representatives.")]
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task ProposeAsync (String text) {

            ulong channel_id = 476521329122869259;

            Nacho nacho = new Nacho ();

            List<SocketRole> roles = Context.Guild.GetUser (Context.Message.Author.Id).Roles.ToList ();

            SocketRole rep_role = Context.Guild.GetRole (Context.Guild.Roles.FirstOrDefault (e => e.Name == "Representative").Id);

            if (!roles.Contains (rep_role)) return;

            DateTime stamp = DateTime.Now.ToUniversalTime ();

            var channel = Context.Guild.GetChannel (channel_id) as ISocketMessageChannel;
            if (channel == null) return;

            Nacho.representative rep = nacho.get_rep (Context.User.Username, Convert.ToUInt64 (Context.User.Discriminator)).FirstOrDefault ();

            Attachment attach = Context.Message.Attachments.FirstOrDefault ();

            string nickname = Context.Guild.GetUser (Context.Message.Author.Id).Nickname;

            text = text.Insert (0, "```");
            text = text += "```";
            text = stamp.ToString () + System.Environment.NewLine + "Proposal by: " + nickname + System.Environment.NewLine + "Representing Faction: " + rep.faction_text + System.Environment.NewLine + text;

            if (attach != null) {
                using (WebClient wc = new WebClient ()) {
                    wc.DownloadFile (new System.Uri (attach.Url),
                        attach.Filename);
                }

                await channel.SendFileAsync (attach.Filename, text, false, null);
            } else {
                await channel.SendMessageAsync (text, false, null, null);
            }

        }

    }
}