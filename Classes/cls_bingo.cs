using System;
using System.Threading;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using JsonFlatFileDataStore;

namespace timebot.Classes
{
    public class participant
    {
        public ulong channel_id{get;set;}
        public SocketGuildUser part{get;set;}
    }
    public class Bingo
    {
        public char[] letters { get; } = { 'B', 'I', 'N', 'G', 'O' };
        public bool bingo { get; set; } = false;
        public SocketGuildUser winner {get;set;}
        public bool stopped{get;set;} = false;
        public Dictionary<char, List<int>> Gen_Card()
        {
            Dictionary<char, List<int>> bingo_card = new Dictionary<char, List<int>>();
            Random rnd = new Random();

            foreach (char letter in letters)
            {
                List<int> numbers = new List<int>();
                for (int i = 0; i < 5; i++)
                {
                    switch (letter)
                    {
                        case 'B':
                            numbers.Add(rnd.Next(0, 15));
                            break;
                        case 'I':
                            numbers.Add(rnd.Next(16, 30));
                            break;
                        case 'N':
                            numbers.Add(rnd.Next(31, 45));
                            break;
                        case 'G':
                            numbers.Add(rnd.Next(46, 60));
                            break;
                        case 'O':
                            numbers.Add(rnd.Next(61, 75));
                            break;
                        default:
                            break;
                    }
                }
            }

            return bingo_card;

        }

        public void add_participant(participant user)
        {
            // Open database (create new if file doesn't exist)
            var store = new DataStore("participant.json");

            // Get employee collection
            var collection = store.GetCollection<participant>();

            collection.InsertOne(user);

            store.Dispose();
        }

        public List<participant> get_participants(ulong channel_id)
        {
            // Open database (create new if file doesn't exist)
            var store = new DataStore("participant.json");

            // Get employee collection
            return store.GetCollection<participant>().AsQueryable().Where(e=>e.channel_id==channel_id).ToList();
        }

        public void clear_participants(ulong channel_id)
        {
            // Open database (create new if file doesn't exist)
            var store = new DataStore("participant.json");

            store.GetCollection<participant>().DeleteManyAsync(e=>e.channel_id == channel_id);
        }

        public string call_next()
        {
            string rtn_message = "";

            Random rnd = new Random();

            char letter = this.letters[rnd.Next(0, 5)];

            int number = 0;

            switch (letter)
            {
                case 'B':
                    number = rnd.Next(0, 15);
                    break;
                case 'I':
                    number = rnd.Next(16, 30);
                    break;
                case 'N':
                    number = rnd.Next(31, 45);
                    break;
                case 'G':
                    number = rnd.Next(46, 60);
                    break;
                case 'O':
                    number = rnd.Next(61, 75);
                    break;
                default:
                    break;
            }

            rtn_message += letter.ToString();
            rtn_message += "    ";
            rtn_message += number.ToString();

            return rtn_message;
        }

        public void print_card(SocketGuildUser usr, Dictionary<char, List<int>> card)
        {
            List<string> message = new List<string>();
            string tab = "    ";
            message.Add("```");

            this.letters.ToList().ForEach(e => message.Add(e.ToString() + tab));

            for (int i = 0; i < 5; i++)
            {
                string row = "";
                foreach (KeyValuePair<char, List<int>> kvp in card)
                {
                    row += kvp.Value[i].ToString();
                    row += tab;
                }
                message.Add(row);
            }

            message.Add("```");

            string rtn_message = String.Join(System.Environment.NewLine, message);

            usr.SendMessageAsync(rtn_message, false, null, null);
        }
    }

}