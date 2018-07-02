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

        private static void Main(string[] args) => new Program().RunBotAsync(args[0]).GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        public static readonly string[] prefixes = {
            "tb!"
        };

        public async Task RunBotAsync(string botToken)
        {

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
                bool isAuthorized = CheckAuthorization(user, msg_prefix);

                if (!isAuthorized)
                {
                    //if not authorized, drop out before any message is processed 
                    await SendPMAsync("You are not authorized to send messages for this Faction.", message.Author);

                    return;
                }

                //figure out which module we're looking at
                if (fullcommand.ToString().Contains("votefor") || fullcommand.ToString().Contains("votetally") || fullcommand.ToString().Contains("listquestions"))
                {
                    //process a vote request
                    voting.voting vt = new churchbot.voting.voting();
                    List<string> returns = await vt.ProcessVote(message, msg_prefix);
                    foreach (string rtn in returns)
                    {
                        await SendPMAsync(rtn, message.Author);
                    }
                }
                else if (fullcommand.ToString().Contains("addquestion"))
                {
                    //process a new poll
                    churchbot.voting.voting vt = new churchbot.voting.voting();
                    List<int> ids = new List<int>();
                    string path = String.Concat("votes/" + msg_prefix.Replace("!", ""), "/");
                    //make sure the directory exists before we call any other methods
                    if (!(Directory.Exists(path))) Directory.CreateDirectory(path);

                    string[] files = System.IO.Directory.GetFiles(path);
                    int id = 0;

                    foreach (string file in files)
                    {
                        ids.Add(Convert.ToInt32(file.Replace(path, "").Replace(".json", "")));
                    }

                    if (ids.Count == 0)
                    {
                        id = 1;
                    }
                    else
                    {
                        id = ids.Max() + 1;
                    }

                    List<string> returns = await vt.AddQuestion(id, msg_prefix);

                    await SendPMAsync(returns[0], message.Author);

                }
                else
                {
                    //here is where we handle commands that are not involved in voting, that just return messages.
                    switch (msg_prefix)
                    {
                        case "cb!":
                            Modules.HighChurch.commands cmd = new Modules.HighChurch.commands();
                            Type type = cmd.GetType();
                            MethodInfo methodInfo = type.GetMethod(FirstCharToUpper(command) + "Async");
                            List<object> list = new List<object>();
                            list.Add(message.Author);
                            methodInfo.Invoke(cmd, list.ToArray());
                            break;
                        case "!":
                            Modules.Default.commands cmd2 = new Modules.Default.commands();
                            Type type2 = cmd2.GetType();
                            MethodInfo methodInfo2 = type2.GetMethod(FirstCharToUpper(command) + "Async");
                            List<object> list2 = new List<object>();
                            list2.Add(message.Author);
                            methodInfo2.Invoke(cmd2, list2.ToArray());
                            break;
                        default:
                            await SendPMAsync("There are no commands associated with this faction. Please consult an admin", message.Author);
                            break;
                    }
                }
            }
        }

        public async Task SendPMAsync(string message, SocketUser user)
        {
            string logmessage = String.Concat(user.Username, " was sent a messge");

            await Log(new LogMessage(LogSeverity.Info, "VERBOSE", logmessage));

            await user.SendMessageAsync(message);
        }

        private bool CheckAuthorization(SocketGuildUser user, string prefix)
        {
            

            return isAuthorized;
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
