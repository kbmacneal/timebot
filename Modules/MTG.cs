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
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using NodaTime;
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

            var response = await "https://api.scryfall.com/cards/"
                .AppendPathSegment ("named")
                .SetQueryParams (new { exact = cardname })
                .GetAsync ()
                .ReceiveString ();

            var value = Classes.MTG.Mtgc.FromJson (response);

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

        [Command ("mtga")]
        [Summary ("Gets the art of an MTG card by name.")]
        public async Task MtgaAsync (params string[] args)
        {
            string cardname = string.Join (" ", args);

            var response = await "https://api.scryfall.com/cards/"
                .AppendPathSegment ("named")
                .SetQueryParams (new { exact = cardname })
                .GetAsync ()
                .ReceiveString ();

            var value = Classes.MTG.Mtgc.FromJson (response);

            if (value == null)
            {
                await ReplyAsync ("Card Not Found.");
                return;
            }
            else
            {
                await ReplyAsync (value.ImageUris.ArtCrop.ToString ());
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