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
        [RequireBotPermission (GuildPermission.Administrator)]
        public async Task ProposeAsync (String text) {

            ulong channel_id = 476521329122869259;

            var channel = Context.Guild.GetChannel (channel_id) as ISocketMessageChannel;
            if (channel == null) return;

            Attachment attach = Context.Message.Attachments.FirstOrDefault ();

            text = text.Insert(0,"```");
            text = text += "```";

            if (attach != null) {
                using (WebClient wc = new WebClient ()) {
                    wc.DownloadFileAsync (new System.Uri (attach.Url),
                        attach.Filename);                        
                }

                await channel.SendFileAsync(attach.Filename,text,false,null);
            }
            else{
                await channel.SendMessageAsync(text,false,null,null);
            }

        }

    }
}