using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using timebot.Classes;

//TODO: add the new logic in the call method to the bet and raise methods to make them all work the same way
namespace timebot.Commands
{
    public class PlayHoldem : ModuleBase<SocketCommandContext>
    {
        private RequestOptions _opt = new RequestOptions
        {
            RetryMode = RetryMode.RetryRatelimit
        };


        [Command("startholdem")]
        public async Task StartholdemAsync()
        {
            HoldEm game = new HoldEm();

            Program.HoldEm.Add(Context.Channel.Id, game);

            await ReplyAsync("Game started. Use the joinholdem command to grab a seat at the table.");
        }

        [Command("joinholdem")]
        public async Task JoinholdemAsync()
        {
            HoldEm Game = Program.HoldEm.FirstOrDefault(e => e.Key == Context.Channel.Id).Value;

            if (Game == null)
            {
                await ReplyAsync("A game has not been started in this channel. Use the startholdem command to start a game.");

                return;
            }

            if (Game.players.Select(e => e.ID).Contains(Context.Message.Author.Id))
            {
                await ReplyAsync("You have already joined this game.");

                return;
            }

            Player player = new Player
            {
                cash_pool = 10000,
                ID = Context.Message.Author.Id
            };

            Game.players.Add(player);

            string message = generate_name(Context.Guild.GetUser(Context.Message.Author.Id)) + " has joined the game.";


            await ReplyAsync(message, false, null, _opt);
        }

        [Command("startround")]
        public async Task StartroundAsync()
        {

            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            //perform the small and big blinds

            int small_index = (game.dealer_index + 1) % game.players.Count();
            int big_index = (game.dealer_index + 2) % game.players.Count();

            game.players[small_index].cash_pool = game.players[small_index].cash_pool - game.small_blind;

            game.players[big_index].cash_pool = game.players[big_index].cash_pool - game.big_blind;

            SocketGuildUser small_user = Context.Guild.GetUser(game.players[small_index].ID);
            SocketGuildUser big_user = Context.Guild.GetUser(game.players[big_index].ID);

            await ReplyAsync(generate_name(small_user) + " has been debited the small blind.", false, null, _opt);

            await ReplyAsync(generate_name(big_user) + " has been debited the big blind.", false, null, _opt);

            game.current_round = new betting_round();

            // betting_round round = new betting_round();

            game.current_round.pot += game.big_blind;
            game.current_round.pot += game.small_blind;

            //deal the hole cards

            Stack<StandardCard> deck = game.deck;
            // int max_deal = game.players.Count * 2;

            for (int i = 0; i < 2; i++)
            {
                foreach (var player in game.players)
                {
                    player.hole.Add(deck.Pop());

                }
            }

            game.players.ForEach(e => send_cards(e.ID));

            await ReplyAsync("Cards Dealt.", false, null, _opt);

            game.current_round.call_position = game.dealer_index + 1;

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)), false, null, _opt);
        }

        [Command("holdemtable")]
        public async Task HoldemtableAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            await ReplyAsync(game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool.ToString());
        }

        [Command("holdemchips")]
        public async Task HoldemchipsAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            await ReplyAsync(game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool.ToString());
        }

        [Command("holdempot")]
        public async Task HoldempotAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            await ReplyAsync(game.current_round.pot.ToString());
        }

        [Command("holdembet")]
        public async Task HoldembetAsync(int amount)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            game.current_round.pot += amount;

            do
            {
                game.current_round.call_position = (game.current_round.call_position + 1) % game.players.Count();
            } while (!game.players[game.current_round.call_position].fold);

            game.current_round.call_count = 0;

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        [Command("holdemcheck")]
        public async Task HoldemcheckAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            do
            {
                game.current_round.call_position = (game.current_round.call_position + 1) % game.players.Count();
            } while (!game.players[game.current_round.call_position].fold);

            game.current_round.call_count++;

            if (game.current_round.call_count == game.players.Count() - 1)
            {
                await lay_down_next(Context);
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        [Command("holdemcall")]
        public async Task HoldemcallAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;
            int last_player_bet_amount = 0;

            if (game.current_round.bets.LastOrDefault(e => e.player_id == Context.Message.Author.Id) == null)
            {
                last_player_bet_amount = 0;
            }
            else
            {
                last_player_bet_amount = game.current_round.bets.LastOrDefault(e => e.player_id == Context.Message.Author.Id).amount;
            }

            int last_bet_amount = 0;

            if (game.current_round.bets.Count == 0)
            {
                last_bet_amount = 0;
            }
            else
            {
                last_bet_amount = game.current_round.bets.Last().amount;
            }

            game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool -= last_bet_amount - last_player_bet_amount;

            game.current_round.pot += last_bet_amount - last_player_bet_amount;

            game.current_round.call_position = determine_next_call_index(game.current_round.call_position, game.players);

            game.current_round.call_count++;

            if (game.current_round.call_count >= game.players.Count())
            {
                await lay_down_next(Context);
                game.current_round.call_position = game.dealer_index + 1;
            }

            if(game.current_round.stop)
            {
                game.current_round = null;
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        [Command("holdemraise")]
        public async Task HoldemraiseAsync(int amount)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            int last_player_bet_amount = game.current_round.bets.Last(e => e.player_id == Context.Message.Author.Id).amount;

            int last_bet_amount = game.current_round.bets.Last().amount;

            game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool -= last_bet_amount - last_player_bet_amount + amount;

            game.current_round.pot += last_bet_amount - last_player_bet_amount + amount;

            do
            {
                game.current_round.call_position = (game.current_round.call_position + 1) % game.players.Count();
            } while (!game.players[game.current_round.call_position].fold);

            game.current_round.call_count = 0;

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        [Command("holdemfold")]
        public async Task HoldemfoldAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            game.players.First(e => e.ID == Context.Message.Author.Id).fold = true;

            do
            {
                game.current_round.call_position = (game.current_round.call_position + 1) % game.players.Count();
            } while (!game.players[game.current_round.call_position].fold);

            game.current_round.call_count++;

            if (game.current_round.call_count == game.players.Count() - 1)
            {
                await lay_down_next(Context);
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        [Command("holdemleave")]
        public async Task HoldemleaveAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;


            game.players.Remove(game.players.First(e => e.ID == Context.Message.Author.Id));

            await ReplyAsync(generate_name(Context.Guild.GetUser(Context.Message.Author.Id)) + " has removed themself from the game", false, null, _opt);
        }

        private async Task lay_down_next(SocketCommandContext context)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == context.Channel.Id).Value;
            game.current_round.call_count = 0;

            if (game.current_round.flop == null)
            {
                game.current_round.flop = new List<StandardCard>();

                game.current_round.flop.Add(game.deck.Pop());
                game.current_round.flop.Add(game.deck.Pop());
                game.current_round.flop.Add(game.deck.Pop());

                List<string> message = new List<string>();

                message.Add("The Flop:");

                game.current_round.flop.ForEach(e => message.Add(StandardCard.value_to_output[e.value].ToString() + " of " + StandardCard.suit_to_output[e.suit].ToString()));

                string rtner = string.Join(System.Environment.NewLine, message);

                await context.Channel.SendMessageAsync(rtner);

                return;
            }

            if (game.current_round.turn == null)
            {
                game.current_round.turn = game.deck.Pop();

                List<string> message = new List<string>();

                message.Add("The Turn:");

                message.Add(StandardCard.value_to_output[game.current_round.turn.value].ToString() + " of " + StandardCard.suit_to_output[game.current_round.turn.suit].ToString());

                string rtner = string.Join(System.Environment.NewLine, message);

                await context.Channel.SendMessageAsync(rtner);

                return;
            }

            if (game.current_round.river == null)
            {
                game.current_round.river = game.deck.Pop();

                List<string> message = new List<string>();

                message.Add("The River:");

                message.Add(StandardCard.value_to_output[game.current_round.river.value].ToString() + " of " + StandardCard.suit_to_output[game.current_round.river.suit].ToString());

                string rtner = string.Join(System.Environment.NewLine, message);

                await context.Channel.SendMessageAsync(rtner);

                return;
            }

            //if we get here, that means that all three rounds of betting have been completed and its time to calculate the hands and determine a winner

            foreach(var player in game.players)
            {
                List<StandardCard> cumulative_hand = new List<StandardCard>();

                player.hole.ForEach(e=>cumulative_hand.Add(e));

                game.current_round.flop.ForEach(e=>cumulative_hand.Add(e));

                cumulative_hand.Add(game.current_round.turn);

                cumulative_hand.Add(game.current_round.river);

                player.hand_weight = Classes.StandardCard.eval_hand(cumulative_hand);
            }

            var winner = game.players.OrderByDescending(e=>e.hand_weight).First();

            we_have_a_winner(winner, Context);
        }

        private static SocketGuildUser get_usr_from_index(SocketCommandContext context, int index)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == context.Channel.Id).Value;

            SocketGuildUser usr = context.Guild.GetUser(game.players[index].ID);

            return usr;
        }

        private static string generate_name(SocketGuildUser usr)
        {
            return usr.Nickname == null || usr.Nickname == "" ? usr.Username : usr.Nickname;
        }
        private void send_cards(UInt64 id)
        {
            SocketGuildUser usr = Context.Guild.GetUser(id);

            List<string> message = new List<string> { "Your Hole Cards:" };

            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            Player player = game.players.First(e => e.ID == id);

            player.hole.ForEach(e => message.Add(StandardCard.value_to_output[e.value].ToString() + " of " + StandardCard.suit_to_output[e.suit].ToString()));

            usr.SendMessageAsync(string.Join(System.Environment.NewLine, message), false, null, _opt).GetAwaiter().GetResult();
        }

        private static int determine_next_call_index (int current_index, List<Player> players)
        {
            int rtn = 0;

            rtn = players.FindIndex(current_index,players.Count -1, e=> e.fold == false);

            if(rtn == -1)
            {
                rtn = players.FindIndex(0,players.Count-1, e=> e.fold == false);
            }


            return rtn;
        }

        private static void we_have_a_winner(Player player, SocketCommandContext con)
        {
            SocketCommandContext cont = con;
            HoldEm game = Program.HoldEm.First(e => e.Key == cont.Channel.Id).Value;

            SocketGuildUser usr = cont.Guild.GetUser(player.ID);

            List<string> hole_cards = new List<string>();

            player.hole.ForEach(e => hole_cards.Add(StandardCard.value_to_output[e.value].ToString() + " of " + StandardCard.suit_to_output[e.suit].ToString()));

            string msg = generate_name(usr) + " has won the round, pot size " + game.current_round.pot.ToString() + " imperial credits. Hole Cards were " + System.Environment.NewLine + string.Join(System.Environment.NewLine, hole_cards) + System.Environment.NewLine + "Use tb!startround to begin another round.";

            player.cash_pool += game.current_round.pot;

            cont.Channel.SendMessageAsync(msg,false,null,null).GetAwaiter().GetResult();
        }


    }
}
