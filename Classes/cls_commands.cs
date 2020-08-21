using System.Collections.Generic;

namespace timebot.Classes
{
    public class botcommand
    {
        public int id { get; set; }
        public ulong serverid { get; set; }
        public string commandname { get; set; }
    }

    public class commands_json
    {
        public string api_key { get; set; }
        public List<Command> commands { get; set; }
    }
}