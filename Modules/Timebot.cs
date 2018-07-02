using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Threading;

namespace timebot.Modules.Default {

    public class commands : ModuleBase<SocketCommandContext> {

        public async Task SendPMAsync (string message, SocketUser user) {
            await user.SendMessageAsync (message);
        }

        public async Task PingAsync (SocketUser user) {
            await user.SendMessageAsync ("Pong!");
        }

        public async Task StarttimerAsync (SocketUser user)
        {
            
        }

        //template
        // [Command ("")]
        // public async Task NameAsync (SocketUser user) {
        //     await user.SendMessageAsync ("Text");
        // }

    }

}