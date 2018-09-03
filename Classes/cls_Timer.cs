using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace timebot.Classes {
    public class timer {
        public IGuildUser user { get; set; }

        public void StartTimer (int dueTime) {
            Timer t = new Timer (new TimerCallback (TimerProc));
            t.Change (dueTime, System.Threading.Timeout.Infinite);
        }

        private void TimerProc (object state) {
            // The state object is the Timer object.
            Timer t = (Timer) state;
            t.Dispose ();
            send_complete_message ();
        }

        public void send_complete_message ()

        {
            this.user.SendMessageAsync ("You are out of time.");
        }
    }

    public class bingotimer {
        public SocketCommandContext Context { get; set; }
        public Bingo bingo { get; set; }
        public bool setup { get; set; } = true;
        public void StartTimer (int dueTime) {
            Timer t = new Timer (new TimerCallback (TimerProc));
            t.Change (dueTime, System.Threading.Timeout.Infinite);
        }

        public void StartTimer () {
            Timer t = new Timer (new TimerCallback (TimerProc));
            t.Change ((int) (0.25 * 60.00 * 1000.00), System.Threading.Timeout.Infinite);
        }

        private void TimerProc (object state) {
            // The state object is the Timer object.
            Timer t = (Timer) state;
            t.Dispose ();
            if (setup) continue_bingo ();
        }

        public void continue_bingo ()

        {
            this.setup = false;

            this.Context.Channel.SendMessageAsync ("Beginning Game");
            this.Context.Channel.SendMessageAsync ("To declare yourself the winner, use the command tb!playwinner");

            while (!bingo.bingo && !bingo.stopped) {
                if (bingo.is_there_a_winner (this.Context.Channel.Id)) break;
                this.Context.Channel.SendMessageAsync (bingo.call_next ());
                Thread.Sleep (10 * 1000);
            }

            this.Context.Channel.SendMessageAsync ("Winner is " + bingo.get_winner (Context.Message.Channel.Id).username.ToString ());

            bingo.clear_participants (Context.Message.Channel.Id);
        }
    }
}