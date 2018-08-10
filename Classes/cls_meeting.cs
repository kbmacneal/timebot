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
using NodaTime;
using NodaTime.TimeZones;

namespace timebot.Classes
{
    public class Meeting
    {
        public static string[] factions { get; } = {
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

        public static async Task<List<notice>> get_notice()
        {
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<notice>();

            List<notice> note = store.GetCollection<notice>().AsQueryable().ToList();

            store.Dispose();

            return note;
        }

        public static void add_acknowledged(int id, string ack)
        {
            var store = new DataStore("data.json");

            // Get employee collection
            var collection = store.GetCollection<notice>();

            notice note = get_notice(id).GetAwaiter().GetResult();

            if (note.acknowledged == null)
            {
                note.acknowledged = new List<string>();
            }

            note.acknowledged.Add(ack);

            dynamic source = new ExpandoObject();
            source.acknowledged = note.acknowledged;
            collection.UpdateOneAsync(e => e.ID == note.ID, source as object);

            note = get_notice(note.ID).GetAwaiter().GetResult();

            string check = "<:white_check_mark:477266462109728770>";

            note.text = note.text.Replace(ack, ack + " " + check);

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

            if (note.attendees == null)
            {
                note.attendees = new List<string>();
            }

            note.attendees.Add(att);

            dynamic source = new ExpandoObject();
            source.attendees = note.attendees;
            collection.UpdateOneAsync(e => e.ID == note.ID, source as object);

            note = get_notice(note.ID).GetAwaiter().GetResult();

            List<string> edited = note.text.Split(System.Environment.NewLine).ToList();

            string speaker = "<:speaker:477266361882640404>";

            note.text = note.text.Replace(att, att + " " + speaker);

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

        public static string gen_text(string title, string datetime)
        {
            List<string> message = new List<string>();

            message.Add(title);

            DateTime test = DateTime.MinValue;

            string[] split = datetime.Split(" ");

            string[] date = split[0].Split("/");

            string[] time = split[1].Split(":");

            DateTime dt = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[0]), Convert.ToInt32(date[1]), Convert.ToInt32(time[0]), Convert.ToInt32(time[1]), Convert.ToInt32(time[2]));

            DateTime adjusted = TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.Local);

            if (!(DateTime.TryParse(datetime, out test)))
            {
                return string.Empty;
            }
            else
            {
                DateTime UTCTime = adjusted.ToUniversalTime();

                var easternTimeZone = DateTimeZoneProviders.Tzdb["America/New_York"];
                var centralTimeZone = DateTimeZoneProviders.Tzdb["America/Chicago"];


                DateTime estTime = Instant.FromDateTimeUtc(UTCTime)
                  .InZone(easternTimeZone)
                  .ToDateTimeUnspecified();

                message.Add(estTime + " " + "Eastern");

                DateTime cstTime = Instant.FromDateTimeUtc(UTCTime)
                  .InZone(centralTimeZone)
                  .ToDateTimeUnspecified();

                message.Add(estTime + " " + "Central");

                message.Add(UTCTime.ToString() + " UTC");

            }

            message.Add("Confirmation of Attendees");
            factions.ToList().ForEach(e => message.Add(e));

            string check = "<:white_check_mark:477266462109728770>";
            string speaker = "<:speaker:477266361882640404>";

            message.Add(check + "<- Indicates Confirmation");
            message.Add(speaker + "<- Indicates Attendance");

            return string.Join(System.Environment.NewLine, message);
        }
    }
}