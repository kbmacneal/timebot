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

    public class Meeting : ModuleBase<SocketCommandContext>
    {
        

        [Command("postmeeting")]
        [RequireBotPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task PostmeetingAsync(string title, string date, string time, string timezone)
        {
            timebot.Classes.Meeting.notice notice = new timebot.Classes.Meeting.notice ();

            SocketGuildUser user = (SocketGuildUser)Context.Message.Author;

            if (!(user.Roles.ToList().Select(e => e.Name).Contains("Representative"))) return;

            notice.text = Classes.Meeting.gen_text(title, date, time, timezone);

            if(notice.text == string.Empty)
            {
                await ReplyAsync("Datetime not in valid format.");
                return;
                
            }

            await Classes.Meeting.insert_notice(notice);

            ulong channel_id = 477209571509796866;

            ISocketMessageChannel chnl = Context.Guild.GetChannel(channel_id) as ISocketMessageChannel;

            await chnl.SendMessageAsync(notice.text);

            await ReplyAsync("Meeting proposal posted.");

        }

        [Command("acknowledge")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task AcknowledgeAsync(int id, string faction)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.Message.Author.Id);

            if(!(user.Roles.Select(e=>e.Name).Contains("Speaker")))return;

            Classes.Meeting.add_acknowledged(id, faction);

            Classes.Meeting.notice note = await Classes.Meeting.get_notice(id);

            ulong channel_id = 477209571509796866;

            ISocketMessageChannel chnl = Context.Guild.GetChannel(channel_id) as ISocketMessageChannel;

            SocketUserMessage message = chnl.GetMessagesAsync(Int32.MaxValue,CacheMode.AllowDownload,null).Flatten().GetAwaiter().GetResult().FirstOrDefault(e=>e.Content.Contains(note.title)) as SocketUserMessage;

            await message.ModifyAsync(e=>e.Content = note.text);


        }

        [Command("attend")]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task AttendAsync(int id, string faction)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.Message.Author.Id);

            if(!(user.Roles.Select(e=>e.Name).Contains("Speaker")))return;

            Classes.Meeting.add_attendee(id, faction);

            Classes.Meeting.notice note = await Classes.Meeting.get_notice(id);

            ulong channel_id = 477209571509796866;

            ISocketMessageChannel chnl = Context.Guild.GetChannel(channel_id) as ISocketMessageChannel;

            SocketUserMessage message = chnl.GetMessagesAsync(Int32.MaxValue,CacheMode.AllowDownload,null).Flatten().GetAwaiter().GetResult().FirstOrDefault(e=>e.Content.Contains(note.title)) as SocketUserMessage;

            await message.ModifyAsync(e=>e.Content = note.text);


        }
    }

}