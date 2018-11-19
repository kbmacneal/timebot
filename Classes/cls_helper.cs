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

namespace timebot.Classes
{
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

            embed.WithTitle(Helper.GetPropValue(obj, title_property_name).ToString());

            foreach (var property in properties)
            {
                if (property != title_property_name)
                {
                    embed.AddInlineField(property, Helper.GetPropValue(obj, property));
                }

            }

            return embed.Build();
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