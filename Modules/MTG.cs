using Discord.Commands;
using Flurl;
using Flurl.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace timebot.Modules.Commands
{
    public class MTG : ModuleBase<SocketCommandContext>
    {
        [Command("mtgc")]
        [Summary("Gets an MTG card by name.")]
        public async Task MtgcAsync(params string[] args)
        {
            string cardname = string.Join(" ", args);

            var response = await "https://api.scryfall.com/cards/"
                .AppendPathSegment("named")
                .SetQueryParams(new { exact = cardname })
                .GetAsync()
                .ReceiveString();

            var value = Classes.MTG.Mtgc.FromJson(response);

            if (value == null)
            {
                await ReplyAsync("Card Not Found.");
                return;
            }
            else
            {
                await ReplyAsync(value.ImageUris.Large.ToString());
                return;
            }
        }

        [Command("mtga")]
        [Summary("Gets the art of an MTG card by name.")]
        public async Task MtgaAsync(params string[] args)
        {
            string cardname = string.Join(" ", args);

            var response = await "https://api.scryfall.com/cards/"
                .AppendPathSegment("named")
                .SetQueryParams(new { exact = cardname })
                .GetAsync()
                .ReceiveString();

            var value = Classes.MTG.Mtgc.FromJson(response);

            if (value == null)
            {
                await ReplyAsync("Card Not Found.");
                return;
            }
            else
            {
                await ReplyAsync(value.ImageUris.ArtCrop.ToString());
                return;
            }
        }

        public EventHandler<ErrorEventArgs> HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ToString();

            return null;
        }
    }
}