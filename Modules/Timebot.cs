using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Threading;
using timebot.Classes;
using System.Linq;

namespace timebot.Modules.Default
{

    public class commands : ModuleBase<SocketCommandContext>
    {

        public async Task SendPMAsync(string message, SocketUser user)
        {
            await user.SendMessageAsync(message);
        }

        public async Task PingAsync(SocketUser user)
        {
            await user.SendMessageAsync("Pong!");
        }

        public async Task CommandsAsync(SocketUser user)
        {

            await user.SendMessageAsync(String.Concat("```Here are the commands available" + System.Environment.NewLine +
                "tb!ping : Make sure the bot is alive" + System.Environment.NewLine +
                "tb!changedefaults#: Change the default speaking time" + System.Environment.NewLine +
                "tb!starttimerName#Disc: start a timer for a specific person" + System.Environment.NewLine + "tb!addspeakerName#Disc: adds a speaker to the list" + System.Environment.NewLine + "```"));
        }

        public async Task AddspeakerAsync(SocketUser user)
        {
            Data.speaker spkr = new Data.speaker();
            spkr.user.Name = user.Username;
            spkr.user.ID = user.Discriminator;
            spkr.user.admin = false;
            spkr.speaking_time_minutes = Data.get_speaking_time();

            Data.insert_user(spkr.user);
            Data.insert_speaker(spkr);

            await user.SendMessageAsync("You have been added as a speaker");
        }

        public async Task changedefaults(SocketUser user, int minutes)
        {
            Data.reset_speaking_time(minutes);

            await user.SendMessageAsync("Speaking times have been reset");
        }

        public async Task StarttimerAsync(SocketUser user)
        {

            Data.speaker spkr = new Data.speaker();
            spkr.user = Data.get_users().Where(s => s.Name == user.Username && s.ID == user.Discriminator).FirstOrDefault();

            if (Data.get_users().Where(s => s.Name == user.Username && s.ID == user.Discriminator).Count() > 1)
            {
                Thread.Sleep(spkr.speaking_time_minutes * 60 * 1000);

                await user.SendMessageAsync("You are out of time.");
            }
            else
            {
                await AddspeakerAsync(user);

                spkr = new Data.speaker();
                spkr.user = Data.get_users().Where(s => s.Name == user.Username && s.ID == user.Discriminator).FirstOrDefault();
                
                Thread.Sleep(spkr.speaking_time_minutes * 60 * 1000);

                await user.SendMessageAsync("You are out of time.");
            }
        }

        //template
        // [Command ("")]
        // public async Task NameAsync (SocketUser user) {
        //     await user.SendMessageAsync ("Text");
        // }

    }

}