using System;

namespace timebot.Classes
{
    public class PopCount
    {
        public int ID { get; set; }
        public ulong FactionID { get; set; }
        public string FactionName { get; set; }
        public int MemCount { get; set; }
        public DateTime timestamp { get; set; }
    }
}