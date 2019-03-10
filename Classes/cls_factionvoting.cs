using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JsonFlatFileDataStore;

namespace timebot.Classes {
    public class vote {
        public int ID { get; set; }
        public int vote_id { get; set; }
        public int selection { get; set; }
        public string name { get; set; }
        public ulong discriminator { get; set; }
        public string faction_name { get; set; }
        public ulong faction_id { get; set; }
    }
    public class factionvoting {
        public List<vote> getvote (string username, ulong discriminator) {
            List<vote> rtn = new List<vote> ();

            var store = new DataStore ("data.json");

            return store.GetCollection<vote> ().AsQueryable ().Where (e => e.name == username && e.discriminator == discriminator).ToList ();
        }
        public List<vote> getvote (int ID) {
            List<vote> rtn = new List<vote> ();

            var store = new DataStore ("data.json");

            return store.GetCollection<vote> ().AsQueryable ().Where (e => e.ID == ID).ToList ();
        }

        public List<vote> getvote (string faction) {
            List<vote> rtn = new List<vote> ();

            var store = new DataStore ("data.json");

            return store.GetCollection<vote> ().AsQueryable ().Where (e => e.faction_name == faction).ToList ();
        }

        public List<vote> getvote () {
            List<vote> rtn = new List<vote> ();

            var store = new DataStore ("data.json");

            return store.GetCollection<vote> ().AsQueryable ().ToList ();
        }

        public async Task delete_question (int question_id) {
            List<vote> rtn = new List<vote> ();

            var store = new DataStore ("data.json");

            await store.GetCollection<vote> ().DeleteManyAsync (e => e.vote_id == question_id);
        }

        public async Task add_vote (vote vote) {
            List<vote> rtn = new List<vote> ();

            var store = new DataStore ("data.json");

            await store.GetCollection<vote> ().InsertOneAsync (vote);
        }

        public List<vote> return_tally (int question_id) {
            List<vote> rtn = new List<vote> ();

            var store = new DataStore ("data.json");

            return store.GetCollection<vote> ().AsQueryable ().Where (e => e.vote_id == question_id).ToList ();
        }
    }

}