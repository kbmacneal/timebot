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

namespace timebot.Classes
{
    public class Meeting
    {
        public static string[] factions { get; } =
        {
            "House Crux",
            "House Vela",
            "House Fornax",
            "The Houses Minor",
            "High Church of the Messiah-as-Emperox"

        };
        public class notice
        {
            public int ID { get; set; }
            public string title { get; set; }
            public string text { get; set; }
            public List<string> acknowledged { get; set; }
            public List<string> attendees { get; set; }
        }

        public static async Task insert_notice(notice note)
        {
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<notice>();

            await collection.InsertOneAsync(note);

            store.Dispose();
        }

        public static async Task<notice> get_notice(int id)
        {
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<notice>();

            notice note = store.GetCollection<notice>().AsQueryable().FirstOrDefault(e => e.ID == id);

            store.Dispose();

            return note;
        }

        public static void add_acknowledged(int id, string ack)
        {
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<notice>();

            notice note = get_notice(id).GetAwaiter().GetResult();

            note.acknowledged.Add(ack);

            dynamic source = new ExpandoObject();
            source.acknowledged = note.acknowledged;
            collection.UpdateOne(e => e.ID == note.ID, source as object);

            note = get_notice(note.ID).GetAwaiter().GetResult();

            List<string> edited = note.text.Split(System.Environment.NewLine).ToList();

            edited.ForEach(e => e = e.Contains(ack) ? e + @" \:white_check_mark:" : e);

            note.text = String.Join(System.Environment.NewLine, edited);

            source = new ExpandoObject();
            source.text = note.text;
            collection.UpdateOne(e => e.ID == note.ID, source as object);
        }

        public static void add_attendee(int id, string att)
        {
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<notice>();

            notice note = get_notice(id).GetAwaiter().GetResult();

            note.attendees.Add(att);

            dynamic source = new ExpandoObject();
            source.attendees = note.attendees;
            collection.UpdateOne(e => e.ID == note.ID, source as object);

            note = get_notice(note.ID).GetAwaiter().GetResult();

            List<string> edited = note.text.Split(System.Environment.NewLine).ToList();

            edited.ForEach(e => e = e.Contains(att) ? e + @" \:speaker:" : e);

            note.text = String.Join(System.Environment.NewLine, edited);

            source = new ExpandoObject();
            source.text = note.text;
            collection.UpdateOne(e => e.ID == note.ID, source as object);

        }

        public static void update_text(int id, string text)
        {
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<notice>();

            notice note = get_notice(id).GetAwaiter().GetResult();

            note.text = text;

            dynamic source = new ExpandoObject();
            source.text = note.text;
            collection.UpdateOne(e => e.ID == note.ID, source as object);

        }

        public static string gen_text(string title, string date, string time, string timezone)
        {
            List<string> message = new List<string>();

            message.Add(title);

            DateTime test = DateTime.MinValue;

            if (!(DateTime.TryParse(date + " " + time + " " + timezone, out test)))
            {
                return string.Empty;
            }
            else
            {
                DateTime UTCTime = test.ToUniversalTime();

                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(UTCTime, cstZone);
                message.Add(cstTime + (cstZone.IsDaylightSavingTime(cstTime) ? cstZone.DaylightName : cstZone.StandardName).ToString());

                TimeZoneInfo gmtZone = TimeZoneInfo.FindSystemTimeZoneById("Greenwich Mean Time");
                DateTime gmtTime = TimeZoneInfo.ConvertTimeFromUtc(UTCTime, gmtZone);
                message.Add(gmtTime + (gmtZone.IsDaylightSavingTime(gmtTime) ? gmtZone.DaylightName : gmtZone.StandardName).ToString());

            }

            message.Add("Confirmation of Attendees");
            factions.ToList().ForEach(e => message.Add(e));

            message.Add(@"\:white_check_mark: <- Indicates Confirmation");

            return string.Join(System.Environment.NewLine, message);
        }
    }
}