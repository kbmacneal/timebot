using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NodaTime;
using static timebot.Modules.Commands.commands;

namespace timebot.Classes {
    public class timer {
        public IGuildUser user { get; set; }

        public void StartTimer (int dueTime) {
            System.Threading.Timer t = new System.Threading.Timer (new TimerCallback (TimerProc));
            t.Change (dueTime, System.Threading.Timeout.Infinite);
        }

        private void TimerProc (object state) {
            // The state object is the Timer object.
            System.Threading.Timer t = (System.Threading.Timer) state;
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
            System.Threading.Timer t = new System.Threading.Timer (new TimerCallback (TimerProc));
            t.Change (dueTime, System.Threading.Timeout.Infinite);
        }

        public void StartTimer () {
            System.Threading.Timer t = new System.Threading.Timer (new TimerCallback (TimerProc));
            t.Change ((int) (0.25 * 60.00 * 1000.00), System.Threading.Timeout.Infinite);
        }

        private void TimerProc (object state) {
            // The state object is the Timer object.
            System.Threading.Timer t = (System.Threading.Timer) state;
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

    public class ReminderTimer {
        public SocketCommandContext Context { get; set; }

        public static async Task<System.Timers.Timer> CreateTimer (SocketChannel chan, nextevent t) {

            Instant now = SystemClock.Instance.GetCurrentInstant ();

            var e_time = DateTimeOffset.Parse (t.time);

            Instant timer_instant = Instant.FromDateTimeOffset (e_time);

            Duration interval = timer_instant - now;

            System.Timers.Timer tmr = new System.Timers.Timer (interval.TotalMilliseconds);

            tmr.AutoReset = false;

            tmr.Elapsed += async (object sender, ElapsedEventArgs e) => {
                await ReminderCallBack (chan, t);
            };

            return tmr;
        }
        public static async Task RegisterTimer (SocketChannel chan, nextevent e) {
            var t = await CreateTimer (chan, e);

            t.Start ();

            Program._timers.Add (t);
        }

        public static async Task ReminderCallBack (SocketChannel chan, nextevent t) {
            if (chan == null) return;

            var emb = Helper.ObjToEmbed (t, "name");

            var role = Program._client.GetGuild (452883319328210984).GetRole (452986469808734219);

            await Program._client.GetGuild(452883319328210984).GetTextChannel(chan.Id).SendMessageAsync(role.Mention + ": Timer Has Elapsed", false, emb, null);
        }
    }
}