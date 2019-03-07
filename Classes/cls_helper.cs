using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using Westwind.Utilities;
using System.Security.Cryptography;
using System.Reflection;

namespace timebot.Classes
{

    public static class ListExtensions{
        public static int FindNext<T>(this List<T> list, int StartAt, Predicate<T> p)
        {
            int rtn = 0;

            rtn = list.FindIndex(StartAt, list.Count()-StartAt, p);

            if(rtn == -1)
            {
                rtn = list.FindIndex(0, list.Count(), p);
            }

            return rtn;
        }

        public static T SkipWhileWrap<T>(this IEnumerable<T> list, int StartAt, Predicate<T> p)
        {
            int index = StartAt;
            
            while (true)
            {
                if(p.Invoke(list.ToArray()[index]))
                {
                    index = (index + 1) % list.Count();
                }
                else if(index>list.Count())
                {
                    throw new KeyNotFoundException();
                }
                else{
                    return list.ToArray()[index];
                }

                
            }
        }
    }
    public class Helper
    {

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        public static Embed ObjToEmbed(object obj, string title_property_name = "")
        {
            var properties = obj.GetType().GetProperties().Select(e => e.Name).ToArray();
            var embed = new EmbedBuilder();

            if (title_property_name != "")
            {
                embed.WithTitle(Helper.GetPropValue(obj, title_property_name).ToString());
            }

            embed.WithTitle(Helper.GetPropValue(obj, title_property_name).ToString());

            foreach (var property in properties)
            {
                if (property != title_property_name)
                {
                    embed.AddField(property, Helper.GetPropValue(obj, property));
                }

            }

            return embed.Build();
        }

        public static object SetPropValue(object src, string propName, object value)
        {
            // src.GetType().GetProperty(propName).SetValue(src, value);


            PropertyInfo info = src.GetType().GetProperty(propName);
            
            try
            {
                value = System.Convert.ChangeType(value,
                    info.PropertyType);
            }
            catch (InvalidCastException)
            {
                throw;
            }
            info.SetValue(src, value, null);

            return src;
        }

        public static string calc_salt()
        {
            var random = new RNGCryptoServiceProvider();

            // Maximum length of salt
            int max_length = 8;

            // Empty salt array
            byte[] salt = new byte[max_length];

            // Build the random bytes
            random.GetNonZeroBytes(salt);

            // Return the string encoded salt
            return Convert.ToBase64String(salt);
        }

        public static string compute_hash(string plaintext, string salt, string algorithm = "HMACSHA512")
        {
            if (salt == null || salt == "") salt = calc_salt();
            return Westwind.Utilities.Encryption.ComputeHash(plaintext, salt, algorithm, false);
        }
    }
}