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

        private static string Bash(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }

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

            // var guild = _client.GetGuild(435921918152146945);
            // var user = guild.GetUser(_client.CurrentUser.Id);
            // await user.ModifyAsync(x => {
            //     x.Nickname = "Arch Lector Frederick";
            // });

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

            string msg_prefix = message.Content.ToString().Substring(0, 3);

            //if the prefix is in the list of valid prefixes, continue
            if (prefixes.Any(msg_prefix.Contains))
            {
                //log that we have a command sent
                string logmessage = String.Concat(message.Author, " sent command ", message.Content);

                await Log(new LogMessage(LogSeverity.Info, "VERBOSE", logmessage));

                //useful variables to hold the message content and the command that was sent for later
                string fullcommand = message.Content;

                string command = fullcommand.Replace(msg_prefix, "");

                SocketGuildUser user = (message.Author as SocketGuildUser);

                //check if the user is authorized to send the message that they did, using the prefix binding SortedDictionary above
                bool isAuthorized = CheckAuthorization(user);

                if (!isAuthorized)
                {
                    //if not authorized, drop out before any message is processed 
                    await SendPMAsync("You are not authorized to control the timebot. Send a message to an administrator to request access.", message.Author);

                    return;
                }

                timebot.Modules.Default.commands cmds = new timebot.Modules.Default.commands();

                if (fullcommand.StartsWith("tb!ping"))
                {
                    await cmds.PingAsync(user);
                }

                if (fullcommand.StartsWith("tb!changedefaults"))
                {
                    string[] split = fullcommand.Split("changedefaults");

                    int newminutes = Convert.ToInt32(split[1]);

                    await cmds.changedefaults(user, newminutes);
                }

                if (fullcommand.StartsWith("tb!starttimer"))
                {
                    string[] split = fullcommand.Split("starttimer");

                    string username = split[1].Split("#")[0];
                    string disc = split[1].Split("#")[1];


                    SocketUser usr = _client.GetUser(username, disc);

                    List<SocketUser> admins = new List<SocketUser>();

                    List<Data.user> admns = Data.get_users().Where(s => s.admin == true).ToList();

                    foreach (Data.user admn in admns)
                    {
                        admins.Add(_client.GetUser(admn.Name, admn.Discriminator));
                    }


                    await cmds.StarttimerAsync(usr, admins);
                }

                if (fullcommand.StartsWith("tb!addspeaker"))
                {
                    string[] split = fullcommand.Split("addspeaker");

                    string username = split[1].Split("#")[0];
                    string disc = split[1].Split("#")[1];


                    SocketUser usr = _client.GetUser(username, disc);


                    await cmds.AddspeakerAsync(usr);
                }
                if (fullcommand.StartsWith("tb!commands"))
                {
                    await cmds.CommandsAsync(user);
                }
            }

        }

        public async Task SendPMAsync(string message, SocketUser user)
        {
            string logmessage = String.Concat(user.Username, " was sent a message");

            await Log(new LogMessage(LogSeverity.Info, "VERBOSE", logmessage));

            await user.SendMessageAsync(message);
        }

        private bool CheckAuthorization(SocketGuildUser user)
        {
            timebot.Classes.Data.user usr = new timebot.Classes.Data.user();

            usr.Name = user.Username;
            usr.Discriminator = user.Discriminator;


            return timebot.Classes.Data.is_user_authorized(usr);
        }

        //helper method for manipulating strings to be in the right format for generic method section above
        public static string FirstCharToUpper(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }




    }
}
