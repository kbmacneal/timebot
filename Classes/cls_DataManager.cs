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
        public class user
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public Boolean admin {get;set;}
        }

        public class speaker
        {
            public user user {get;set;}
            public DateTime start_time{get;set;} = DateTime.Now;
            public DateTime speaking_time{get;set;}
        }

        public static List<user> get_auth_user()
        {

            // Open database (create new if file doesn't exist)
            var store = new DataStore("data.json");

            // Get employee collection
            return store.GetCollection<user>().AsQueryable().ToList();
        }

        public static Boolean is_user_authorized(user user)
        {
            List<user> users = get_auth_user();

            if(users.Where(s=>s.admin == true).Contains(user))
            {return true;}
            else {return false;}
        }

        public static Boolean insert_auth_user(user user)
        {
            List<user> auths = get_auth_user();

            if (auths.Contains(user))
            {

                // Open database (create new if file doesn't exist)
                var store = new DataStore("data.json");

                // Get employee collection
                var collection = store.GetCollection<user>();

                collection.InsertOne(user);

                store.Dispose();

                return true;
            }
            else { return false; }
        }
    }

}