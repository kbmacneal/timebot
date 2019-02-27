using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using NodaTime;
using RestSharp;
using timebot.Classes;

namespace timebot.Modules.Commands
{
    public class MTG : ModuleBase<SocketCommandContext>
    {
        [Command ("mtgc")]
        [Summary ("Gets an MTG card by name.")]
        public async Task MtgcAsync (params string[] args)
        {
            string cardname = string.Join (" ", args);

            var client = new RestClient ();

            var base_url = "https://api.scryfall.com/cards/named";

            client.BaseUrl = new Uri (base_url);

            var request = new RestRequest ();

            request.AddQueryParameter ("exact", cardname);

            var response = client.Get (request);

            if (response.IsSuccessful)
            {
                var value = Classes.MTG.Mtgc.FromJson (response.Content);

                if (value == null)
                {
                    await ReplyAsync ("Card Not Found.");
                    return;
                }
                else
                {
                    await ReplyAsync (value.ImageUris.Large.ToString ());
                    return;
                }
            }
            else
            {
                await ReplyAsync ("Card Not Found.");
                return;
            }
        }

        public EventHandler<ErrorEventArgs> HandleDeserializationError (object sender, ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ToString ();

            return null;
        }

    }
}