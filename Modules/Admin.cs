

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using timebot.Classes;
using timebot.Contexts;

namespace timebot.Modules.Commands
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("archivechannel")]
        [Summary("Dumps a json log of the channel into chat.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ArchivechannelAsync()
        {
            string date_archived = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

            IEnumerable<IMessage> archive = (Context.Channel.GetMessagesAsync(Int32.MaxValue).Flatten()).ToEnumerable();

            var query =
                from msg in archive
                select new { msg.Author.Username, msg.Author.Discriminator, msg.Content, msg.CreatedAt, msg.EditedTimestamp, msg.Id, msg.Source, msg.Timestamp, msg.Attachments };

            string serialized = JsonConvert.SerializeObject(query);

            string path = Context.Channel.Name + " " + date_archived;

            System.IO.File.WriteAllText(path + ".json", serialized);

            await ReplyAsync("Channel archived");

            await Context.Channel.SendMessageAsync("Here is the archived file.");
            await Context.Channel.SendFileAsync(path + ".json");
        }

        [Command("dumpserverchat")]
        [Summary("Gets the chat of every channel in the server in a separate json file and spits out the result")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DumpserverchatAsync()
        {
            if (!System.IO.Directory.Exists(Context.Guild.Name))
            {
                System.IO.Directory.CreateDirectory(Context.Guild.Name);
            }

            var path = Context.Guild.Name;

            foreach (var item in Context.Guild.TextChannels)
            {
                string date_archived = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

                // SocketTextChannel temp = Context.Guild.GetTextChannel(item.Id);
                // if(temp == null)continue;

                IEnumerable<IMessage> archive = item.GetMessagesAsync(Int32.MaxValue).Flatten().ToEnumerable();

                // IEnumerable<IMessage> archive = await Context.Channel.GetMessagesAsync(Int32.MaxValue).Flatten();

                var query =
                    from msg in archive
                    select new { msg.Author.Username, msg.Author.Discriminator, msg.Content, msg.CreatedAt, msg.EditedTimestamp, msg.Id, msg.Source, msg.Timestamp, msg.Attachments };

                string serialized = JsonConvert.SerializeObject(query, Formatting.Indented);

                string filepath = System.IO.Path.Join(path, item.Name + " " + date_archived + ".json");

                System.IO.File.WriteAllText(filepath, serialized);
            }

            System.IO.Compression.ZipFile.CreateFromDirectory(path, path + ".zip");
            await Context.Channel.SendMessageAsync("Here is the archived file.");
            await Context.Channel.SendFileAsync(path + ".zip");

            System.IO.Directory.Delete(path, true);
            System.IO.File.Delete(path + ".zip");
        }

        [Command("addservercommand")]
        [Summary("Adds a command to the list of commands allowed on the server.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddservercommandAsync(string command)
        {
            var server_id = Context.Guild.Id;

            using (var context = new Context())
            {
                var check = context.BotCommands.FirstOrDefault(e => e.serverid == server_id && e.commandname == command);

                if (check != null)
                {
                    await ReplyAsync("Command already added to this server.");
                    return;
                }

                if (!Program._commands.Commands.Select(e => e.Name).ToList().Contains(command))
                {
                    await ReplyAsync("Invalid command selected.");
                    return;
                }

                var cmds = context.BotCommands;

                await cmds.AddAsync(new botcommand()
                {
                    serverid = server_id,
                    commandname = command
                });

                await context.SaveChangesAsync();
            }

            await ReplyAsync("Command added.", false, null, null);
        }

        [Command("listservercommand")]
        [Summary("Lists commands allowed on the server.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListservercommandAsync()
        {
            var server_id = Context.Guild.Id;

            List<string> commands = new List<string>();

            using (var context = new Context())
            {
                commands = await context.BotCommands.ToAsyncEnumerable().Where(e => e.serverid == server_id).Select(e => e.commandname).OrderBy(e => e).ToListAsync();
            }

            await ReplyAsync(string.Join(System.Environment.NewLine, commands), false, null, null);
        }

        [Command("cleanservercommands")]
        [Summary("Cleans the list of server commands.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CleanservercommandsAsync()
        {
            using (var context = new Context())
            {
                foreach (var item in context.BotCommands)
                {
                    if (!Program._commands.Commands.Select(e => e.Name).Contains(item.commandname))
                    {
                        context.BotCommands.Remove(item);
                    }
                }

                await context.SaveChangesAsync();
            }

            await ReplyAsync("Cleaned.", false, null, null);
        }

        [Command("freddysays")]
        [Summary("Makes freddy say something in chat.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task FreddysaysAsync(ISocketMessageChannel channel, params string[] message)
        {
            string msg = String.Join(" ", message);

            await channel.TriggerTypingAsync();

            Thread.Sleep(2000);

            await channel.SendMessageAsync(msg, false, null, null);
        }
    }
}