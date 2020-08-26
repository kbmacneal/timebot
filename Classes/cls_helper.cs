using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace timebot.Classes
{
    public static class ListExtensions
    {
        public static int FindNext<T>(this List<T> list, int StartAt, Predicate<T> p)
        {
            int rtn = 0;

            rtn = list.FindIndex(StartAt, list.Count() - StartAt, p);

            if (rtn == -1)
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
                if (p.Invoke(list.ToArray()[index]))
                {
                    index = (index + 1) % list.Count();
                }
                else if (index > list.Count())
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    return list.ToArray()[index];
                }
            }
        }
    }

    public class Helper
    {
        public static List<string> SplitToLines(string input, int max_length)
        {
            List<string> rtn = new List<string>();

            var split = input.Split(System.Environment.NewLine).ToList();

            string insert = "";

            foreach (var item in split)
            {
                if (insert.Length + item.Length + 1 < max_length)
                {
                    insert += item + System.Environment.NewLine;
                }
                else
                {
                    rtn.Add(insert);

                    insert = item + System.Environment.NewLine;
                }
            }

            rtn.Add(insert);

            return rtn;
        }

        public static string GetPlainTextFromHtml(string htmlString)
        {
            return Regex.Replace(htmlString, "<.*?>", String.Empty);
        }

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

        public static object SetPropValuePlain(object src, string propName, object value)
        {
            src.GetType().GetProperty(propName).SetValue(src, value);

            return src;
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
    }
}