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
            public int speaking_time_minutes{get;set;}
        }

        public static List<user> get_users()
        {

            // Open database (create new if file doesn't exist)
            var store = new DataStore("data.json");

            // Get employee collection
            return store.GetCollection<user>().AsQueryable().ToList();
        }

        public static List<speaker> get_speakers()
        {

            // Open database (create new if file doesn't exist)
            var store = new DataStore("data.json");

            // Get employee collection
            return store.GetCollection<speaker>().AsQueryable().ToList();
        }

        public static Boolean is_user_authorized(user user)
        {
            List<user> users = get_users();

            if(users.Where(s=>s.admin == true).Contains(user))
            {return true;}
            else {return false;}
        }

        public static void insert_user(user user)
        {
            // Open database (create new if file doesn't exist)
                var store = new DataStore("data.json");

                // Get employee collection
                var collection = store.GetCollection<user>();

                collection.InsertOne(user);

                store.Dispose();
        }

        public static void insert_speaker(speaker spkr)
        {
            // Open database (create new if file doesn't exist)
                var store = new DataStore("data.json");

                // Get employee collection
                var collection = store.GetCollection<speaker>();

                collection.InsertOne(spkr);

                store.Dispose();
        }

        public static void reset_speaking_time(int minutes)
        {
            get_speakers().ForEach(s=>s.speaking_time_minutes = minutes);
        }

        public static int get_speaking_time()
        {
            return get_speakers().FirstOrDefault().speaking_time_minutes;
        }
    }

}