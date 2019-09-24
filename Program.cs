using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace timebot
{
    internal class Program
    {
        private static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public static Random rand = new Random();

        public static Dictionary<ulong, Classes.HoldEm> HoldEm = new Dictionary<ulong, Classes.HoldEm>();

        public static DiscordSocketClient _client = new DiscordSocketClient();
        public static CommandService _commands = new CommandService();
        public static IServiceProvider _services = new ServiceCollection().AddSingleton(_client).AddSingleton(_commands).BuildServiceProvider();
        public static string secrets_file = "timebot.json";
        public static int latest_xkcd = Classes.Xkcd.Comic.get_latest_xkcd();

        public static readonly string[] prefixes = {
            "tb!"
        };

        public async Task RunBotAsync()
        {

            Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(secrets_file));

            //event subscriptions
            _client.Log += Log;

            await RegisterCommandAsync();

            await _client.LoginAsync(TokenType.Bot, secrets["bot_code"]);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            //prevent infinite loops where the bot is talking to itself or other bots
            if (message is null || message.Author.IsBot)
            {
                return;
            }

            if (message.Content.Contains('“') || message.Content.Contains('”'))
            {
                string msg = message.Content.Replace('“', '"').Replace('”', '"');
                await message.ModifyAsync(e => e.Content = msg);
            }

            if (message.Content.Length < 3 || !message.Content.Contains("!")) return;

            string msg_prefix = message.Content.ToString().Substring(0, 3);

            //if the prefix is in the list of valid prefixes, continue
            if (prefixes.Where(e => String.Equals(e, msg_prefix, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                //log that we have a command sent
                string logmessage = String.Concat(message.Author, " sent command ", message.Content);

                await Log(new LogMessage(LogSeverity.Info, "VERBOSE", logmessage));

                int argPosition = 0;
                if (message.HasStringPrefix(msg_prefix, ref argPosition) || message.HasMentionPrefix(_client.CurrentUser, ref argPosition))
                {
                    var context = new SocketCommandContext(_client, message);

                    string server_id = context.Guild.Id.ToString();

                    if (!(check_command(server_id, message.Content)))
                    {
                        // await context.Channel.SendMessageAsync(message.Content + " not allowed on this server.");
                        await context.Channel.SendMessageAsync("No.");
                        await Log(new LogMessage(LogSeverity.Info, "VERBOSE", message.Content + " is an invalid command."));
                        return;
                    }

                    var result = await _commands.ExecuteAsync(context, argPosition, _services);
                    if (!result.IsSuccess)
                    {
                        var channel = context.Guild.Channels.FirstOrDefault(e => e.Name == "bot-commands") as ISocketMessageChannel;

                        if (channel != null) await channel.SendMessageAsync(result.ErrorReason);

                        Console.WriteLine(result.ErrorReason);
                    }
                }
            }
        }

        public async Task SendPMAsync(string message, SocketUser user)
        {
            string logmessage = String.Concat(user.Username, " was sent a message");

            await Log(new LogMessage(LogSeverity.Info, "VERBOSE", logmessage));

            await user.SendMessageAsync(message);
        }

        public Boolean check_command(string server_id, string command)
        {
            string cmd = "";

            if (command.Contains(" "))
            {
                cmd = command.Split(" ")[0];
            }
            else
            {
                cmd = command;
            }

            if (cmd.Contains("!"))
            {
                cmd = cmd.Split("!")[1];
            }

            if (cmd == "addservercommand")
            {
                return true;
            }

            using (var con = new Contexts.Context())
            {
                var tester = con.BotCommands.FirstOrDefault(e => e.serverid.ToString() == server_id && e.commandname == cmd);

                if (tester != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}