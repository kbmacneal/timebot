using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using timebot.Classes;

namespace timebot.Modules.Commands {

    // public class BingoBot : ModuleBase<SocketCommandContext> {

    //     private static Bingo bingo { get; set; } = new Bingo ();

    //     [Command ("playbingo")]
    //     [Summary("Starts the bingo game.")]
    //     public async Task PlaybingoAsync () {
    //         bingotimer tmr = new bingotimer ();
    //         tmr.Context = Context;
    //         tmr.bingo = bingo;

    //         await ReplyAsync ("Waiting two minutes for signups.");
    //         await ReplyAsync ("You may join the game by using the command tb!iwanttoplay");

    //         tmr.StartTimer (2 * 60 * 1000);
    //     }

    //     [Command ("playwinner")]
    //     [Summary("Indicates to the bot that you have BINGO.")]
    //     public async Task PlaywinnerAsync () {
    //         bingo.make_winner ((SocketGuildUser) Context.Message.Author);
    //     }

    //     [Command ("iwanttoplay")]
    //     [Summary("Indicates to the bot that you wish to play in the bingo game.")]
    //     public async Task Iwanttoplay () {
    //         participant part = new participant ();
    //         part.channel_id = Context.Message.Channel.Id;
    //         part.username = Context.Message.Author.Username;
    //         part.disc = Context.Message.Author.Discriminator;
    //         part.user_id = Context.Message.Author.Id;
    //         bingo.add_participant (part);
    //         await ReplyAsync ("You have been added to the bingo game.");

    //         string message = bingo.format_card (bingo.Gen_Card ());

    //         await Context.User.SendMessageAsync ("Here is your card" + System.Environment.NewLine + message);
    //     }

    // }

}