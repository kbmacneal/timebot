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
        public async Task PostmeetingAsync(string title, string datetime)
        {
            timebot.Classes.Meeting.notice notice = new timebot.Classes.Meeting.notice ();

            SocketGuildUser user = (SocketGuildUser)Context.Message.Author;

            if (!(user.Roles.ToList().Select(e => e.Name).Contains("Representative"))) return;

            notice.text = Classes.Meeting.gen_text(title, datetime);

            if(notice.text == string.Empty)
            {
                await ReplyAsync("Datetime not in valid format.");
                return;

            }

            await Classes.Meeting.insert_notice(notice);

            Classes.Meeting.notice rtn = Classes.Meeting.get_notice().GetAwaiter().GetResult().OrderByDescending(e=>e.ID).First();

            rtn.text = rtn.text.Insert(0,"ID: " + rtn.ID.ToString() + System.Environment.NewLine);
            Classes.Meeting.update_text(rtn.ID, rtn.text);

            ulong channel_id = 477209571509796866;

            ISocketMessageChannel chnl = Context.Guild.GetChannel(channel_id) as ISocketMessageChannel;

            await chnl.SendMessageAsync(rtn.text);

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

            IMessage message = chnl.GetMessagesAsync(Int32.MaxValue).Flatten().GetAwaiter().GetResult().ToList().FirstOrDefault(e=>e.Content.Split(System.Environment.NewLine)[0] == new string("ID: " + note.ID.ToString()));

            await message.DeleteAsync();

            await chnl.SendMessageAsync(note.text,false,null,null);

            await ReplyAsync("Meeting acknowledged.");


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

            IMessage message = chnl.GetMessagesAsync(Int32.MaxValue).Flatten().GetAwaiter().GetResult().ToList().FirstOrDefault(e=>e.Content.Split(System.Environment.NewLine)[0] == new string("ID: " + note.ID.ToString()));

            await message.DeleteAsync();

            await chnl.SendMessageAsync(note.text,false,null,null);

            await ReplyAsync("Meeting attendance acknowledged.");
        }
    }

}