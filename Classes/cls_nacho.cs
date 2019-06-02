using Discord.WebSocket;
using JsonFlatFileDataStore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace timebot.Classes
{
    public class Nacho
    {
        public class representative
        {
            public int ID { get; set; }
            public string name { get; set; }
            public ulong discriminator { get; set; }
            public string faction_text { get; set; }
            public ulong faction_id { get; set; }
        }

        public List<representative> get_rep(string name, ulong discriminator)
        {
            var store = new DataStore("data.json");

            // Get employee collection
            return store.GetCollection<representative>().AsQueryable().Where(e => e.name == name && e.discriminator == discriminator).ToList();
        }

        public List<representative> get_rep(int ID)
        {
            var store = new DataStore("data.json");

            // Get employee collection
            return store.GetCollection<representative>().AsQueryable().Where(e => e.ID == ID).ToList();
        }

        public List<representative> get_rep(string faction)
        {
            var store = new DataStore("data.json");

            // Get employee collection
            return store.GetCollection<representative>().AsQueryable().Where(e => e.faction_text == faction).ToList();
        }

        public List<representative> get_rep()
        {
            var store = new DataStore("data.json");

            // Get employee collection
            return store.GetCollection<representative>().AsQueryable().ToList();
        }

        public async Task assign_representative(representative rep)
        {
            // Open database (create new if file doesn't exist)
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<representative>();

            await collection.InsertOneAsync(rep);

            store.Dispose();
        }

        public async Task remove_rep(representative rep)
        {
            // Open database (create new if file doesn't exist)
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<representative>();

            await collection.DeleteOneAsync(e => e.ID == rep.ID);

            store.Dispose();
        }

        public async Task remove_rep(SocketUser user)
        {
            // Open database (create new if file doesn't exist)
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<representative>();

            await collection.DeleteManyAsync(e => e.name == user.Username && e.discriminator.ToString() == user.Discriminator);

            store.Dispose();
        }
    }
}