using System;
using System.Threading;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace timebot.Classes
{
    public class timer
    {
        public IGuildUser user { get; set; }

        public void StartTimer(int dueTime)
        {
            Timer t = new Timer(new TimerCallback(TimerProc));
            t.Change(dueTime, System.Threading.Timeout.Infinite);
        }

        private void TimerProc(object state)
        {
            // The state object is the Timer object.
            Timer t = (Timer)state;
            t.Dispose();
            send_complete_message();
        }

        public void send_complete_message()

        {
            this.user.SendMessageAsync("You are out of time.");
        }
    }
}