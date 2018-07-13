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

    public class BingoBot : ModuleBase<SocketCommandContext>
    {
        private static Bingo bingo;

        public async Task StartTimer(int dueTime)
        {
            Timer t = new Timer(new TimerCallback(TimerProc));
            t.Change(dueTime, System.Threading.Timeout.Infinite);

            WaitUntilCompleted(t);
        }

        private void TimerProc(object state)
        {
            // The state object is the Timer object.
            Timer t = (Timer)state;
            t.Dispose();
        }

        void WaitUntilCompleted(Timer timer)
        {
            List<WaitHandle> waitHnd = new List<WaitHandle>();
            WaitHandle h = new AutoResetEvent(false);
            if (!timer.Dispose(h)) throw new Exception("Timer already disposed.");
            waitHnd.Add(h);

            WaitHandle.WaitAll(waitHnd.ToArray());
        }

        [Command("playbingo")]
        public async Task PlaybingoAsync()
        {

            await ReplyAsync("Waiting two minutes for signups.");
            await ReplyAsync("You may join the game by using the command tb!iwanttoplay");

            StartTimer(2 * 60 * 1000).GetAwaiter().GetResult();

            foreach (participant part in bingo.get_participants(Context.Message.Channel.Id))
            {
                bingo.print_card(part.part,bingo.Gen_Card());
            }

            StartTimer(1 * 60 * 1000).GetAwaiter().GetResult();

            await ReplyAsync("Beginning Game");
            await ReplyAsync("To declare yourself the winner, use the command tb!playwinner");

            while(!bingo.bingo && !bingo.stopped)
            {
                await ReplyAsync(bingo.call_next());
                StartTimer((int)(0.25 * 60.00 * 1000.00)).GetAwaiter().GetResult();
            }

            if(bingo.bingo) await ReplyAsync("Winner is " + bingo.winner.Nickname.ToString());

            bingo.clear_participants(Context.Message.Channel.Id);

        }

        [Command("playwinner")]
        public async Task PlaywinnerAsync()
        {
            bingo.winner = (SocketGuildUser)Context.Message.Author;
        }

        [Command("iwanttoplay")]
        public async Task Iwanttoplay()
        {
            participant part = new participant();
            part.channel_id = Context.Message.Channel.Id;
            part.part = (SocketGuildUser)Context.Message.Author;
            bingo.add_participant(part);
            await ReplyAsync("You have been added to the bingo game.");
        }

    }

}