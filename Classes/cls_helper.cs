using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

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
                if(property != title_property_name)
                {
                    embed.AddInlineField(property, Helper.GetPropValue(obj, property));
                }               
                
            }

            return embed.Build();
        }
    }
}