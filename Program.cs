using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using timebot.Classes;

namespace timebot
{
    internal class Program
    {

        private static void Main(string[] args) => new Program().RunBotAsync(args[0].ToString()).GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        public static readonly string[] prefixes = {
            "tb!"
        };

        public async Task RunBotAsync(string botToken)
        {

            //initialize the default admin

            if (Data.get_users().Where(s => s.Name == "BowmoreK").Count() == 0)
            {
                Data.user usr = new Data.user();
                usr.Name = "BowmoreK";
                usr.Discriminator = "9327";
                usr.admin = true;

                Data.insert_user(usr);
            }

            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection().AddSingleton(_client).AddSingleton(_commands).BuildServiceProvider();

            //event subscriptions
            _client.Log += Log;

            await RegisterCommandAsync();

            await _client.LoginAsync(TokenType.Bot, botToken);

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
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            //prevent infinite loops where the bot is talking to itself or other bots
            if (message is null || message.Author.IsBot)
            {
                return;
            }

            if(message.Content.Contains('“') || message.Content.Contains('”'))
            {
                string msg = message.Content.Replace('“','"').Replace('”','"');
                await message.ModifyAsync(e=>e.Content = msg);
            }

            string msg_prefix = message.Content.ToString().Substring(0, 3);

            //if the prefix is in the list of valid prefixes, continue
            if (prefixes.Any(msg_prefix.Contains))
            {
                //log that we have a command sent
                string logmessage = String.Concat(message.Author, " sent command ", message.Content);

                await Log(new LogMessage(LogSeverity.Info, "VERBOSE", logmessage));

                int argPosition = 0;
                if (message.HasStringPrefix("tb!", ref argPosition) || message.HasMentionPrefix(_client.CurrentUser, ref argPosition))
                {
                    var context = new SocketCommandContext(_client, message);

                    string server_id = context.Guild.Id.ToString();

                    if (!(check_command(server_id, message.Content.Replace("tb!", ""))))
                    {
                        Console.WriteLine(message.Content + " not allowed on this server.");
                        return;
                    }

                    var result = await _commands.ExecuteAsync(context, argPosition, _services);
                    if (!result.IsSuccess)
                    {
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
            Boolean rtn = false;
            string cmd = "";

            if (command.Contains(" "))
            {
                cmd = command.Split(" ")[0];
            }
            else
            {
                cmd = command;
            }


            List<string> commands_available = generate_server_command_list().Where(e => e.Key == server_id).Select(e => e.Value).FirstOrDefault().ToList();

            rtn = commands_available.Contains(cmd) ? true : false;

            return rtn;
        }

        public Dictionary<string, List<string>> generate_server_command_list()
        {
            Dictionary<string, List<string>> rtn = new Dictionary<string, List<string>>();

            //Inter-faction discussion server
            rtn.Add("465538179978756096", new List<string>(){
                "ping",
                "commands",
                "changedefaults",
                "setbotusername",
                "starttimer",
                "listfaction",
                "addfaction",
                "playbingo",
                "clearspeakers",
                "clearchannel",
                "removefaction",
                "addrepresentative",
                "removerepresentative",
                "vote",
                "tally",
                "deletequestion",
                "setcolors",
                "cactus",
                "badbot"
            });

            //main diplo server
            rtn.Add("435921918152146945", new List<string>(){
                "ping",
                "commands",
                "setbotusername",
                "stopbot",
                "listfaction",
                "addfaction",
                "playbingo",
                "clearchannel",
                "removefaction",
                "setcolors",
                "cactus",
                "badbot"
            });

            return rtn;
        }
    }
}
