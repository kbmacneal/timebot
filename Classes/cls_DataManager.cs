using System.IO;
using JsonFlatFileDataStore;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace timebot.Classes
{

    public class Data
    {
        public class authorized_user
        {
            public string Name { get; set; }
            public string ID { get; set; }
        }

        public List<authorized_user> get_auth_user()
        {

            // Open database (create new if file doesn't exist)
            var store = new DataStore("data.json");

            // Get employee collection
            return store.GetCollection<authorized_user>().AsQueryable().ToList();
        }

        public Boolean insert_auth_user(authorized_user user)
        {
            List<authorized_user> auths = get_auth_user();

            if (auths.Contains(user))
            {

                // Open database (create new if file doesn't exist)
                var store = new DataStore("data.json");

                // Get employee collection
                var collection = store.GetCollection<authorized_user>();

                collection.InsertOne(user);

                store.Dispose();

                return true;
            }
            else { return false; }
        }
    }

}