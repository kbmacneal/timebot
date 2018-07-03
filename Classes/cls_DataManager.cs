using System.IO;
using JsonFlatFileDataStore;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace timebot.Classes
{

    public class Data
    {
        public class user
        {
            public string Name { get; set; }
            public string Discriminator { get; set; }
            public Boolean admin { get; set; }
        }

        public class speaker
        {
            public user user { get; set; }
            public DateTime start_time { get; set; } = DateTime.Now;
            public int speaking_time_minutes { get; set; }
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

        public static Boolean is_speaker(SocketUser user)
        {

            // Open database (create new if file doesn't exist)
            var store = new DataStore("data.json");

            // Get employee collection
            if (store.GetCollection<speaker>().AsQueryable().ToList().Where(s => s.user.Name == user.Username && s.user.Discriminator == user.Discriminator).ToList().Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Boolean is_user_authorized(user user)
        {
            List<user> users = get_users();

            if (users.Where(s => s.admin == true && s.Name == user.Name && s.Discriminator == user.Discriminator).Count() > 0)
            { return true; }
            else { return false; }
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
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<speaker>();

            List<speaker> spkrs = get_speakers();
            spkrs.ForEach(s => s.speaking_time_minutes = minutes);

            spkrs.ForEach(s => collection.UpdateOneAsync(e => s.user == e.user, s));
        }

        public static int get_speaking_time()
        {
            if (get_speakers().Count == 0)
            {
                return 5;
            }
            else
            {
                return get_speakers().FirstOrDefault().speaking_time_minutes;
            }

        }
    }

}