using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Threading;
using timebot.Classes;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace timebot.Modules.Commnads
{

    public class commands : ModuleBase<SocketCommandContext>
    {

        public async Task SendPMAsync(string message, SocketUser user)
        {
            await user.SendMessageAsync(message);
        }

        [Command("ping")]
        public async Task PingAsync()
        {
            await Context.User.SendMessageAsync("Pong!");
        }

        [Command("commands")]
        public async Task CommandsAsync()
        {

            await Context.User.SendMessageAsync(String.Concat("```Here are the commands available" + System.Environment.NewLine +
                "tb!ping : Make sure the bot is alive" + System.Environment.NewLine +
                "tb!changedefaults#: Change the default speaking time" + System.Environment.NewLine +
                "tb!starttimerName#Disc: start a timer for a specific person" + System.Environment.NewLine + "tb!addspeakerName#Disc: adds a speaker to the list" + System.Environment.NewLine + "```"));
        }

        [Command("addspeaker")]
        public async Task AddspeakerAsync(IGuildUser user)
        {
            Data.speaker spkr = new Data.speaker();
            spkr.user = new Data.user();
            spkr.user.Name = user.Username;
            spkr.user.Discriminator = user.Discriminator;
            spkr.user.admin = false;
            spkr.speaking_time_minutes = Data.get_speaking_time();

            Data.insert_user(spkr.user);
            Data.insert_speaker(spkr);

            await Context.User.SendMessageAsync("You have been added as a speaker");
        }

        [Command("changedefaults")]
        public async Task changedefaults(int minutes)
        {
            Data.reset_speaking_time(minutes);

            await Context.User.SendMessageAsync("Speaking times have been reset");
        }

        [Command("starttimer")]
        public async Task StarttimerAsync(IGuildUser user)
        {

            Data.speaker spkr = new Data.speaker();

            if (!Data.is_speaker(user))
            {
                await AddspeakerAsync(user);

                spkr = Data.get_speakers().Where(s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator).First();

                spkr.start_time = DateTime.Now;

                string msg = String.Concat("You are now the speaker. You have ", spkr.speaking_time_minutes, " minutes remaining");

                await user.SendMessageAsync(msg);

                timer tmr = new timer();

                tmr.user = user;

                tmr.StartTimer(spkr.speaking_time_minutes * 60 * 1000);
            }
            else
            {
                spkr = Data.get_speakers().Where(s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator).First();

                spkr.start_time = DateTime.Now;

                string msg = String.Concat("You are now the speaker. You have ", spkr.speaking_time_minutes, " minutes remaining");

                await user.SendMessageAsync(msg);

                timer tmr = new timer();

                tmr.user = user;

                tmr.StartTimer(spkr.speaking_time_minutes * 60 * 1000);
            }
        }



        //template
        // [Command ("")]
        // public async Task NameAsync (SocketUser user) {
        //     await user.SendMessageAsync ("Text");
        // }

    }

}