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

//TODO: 
// allin logic
namespace timebot.Commands
{
    public class PlayHoldem : ModuleBase<SocketCommandContext>
    {
        private static RequestOptions _opt = new RequestOptions
        {
            RetryMode = RetryMode.RetryRatelimit
        };


        [Command("startholdem")]
        [Summary("Starts a holdem game in the channel.")]
        public async Task StartholdemAsync()
        {
            HoldEm game = new HoldEm();

            Program.HoldEm.Add(Context.Channel.Id, game);

            await ReplyAsync("Game started. Use the joinholdem command to grab a seat at the table.");
        }

        [Command("joinholdem")]
        [Summary("Adds your user id to the list of players at the holdem table.")]
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


            var Player = Classes.Player.GetPlayer(Context.Message.Author.Id);

            if (Player == null)
            {
                Player = new Player
                {
                    cash_pool = 10000,
                    ID = Context.Message.Author.Id
                };
            }

            Game.players.Add(Player);

            string message = generate_name(Context.Guild.GetUser(Context.Message.Author.Id)) + " has joined the game.";


            await ReplyAsync(message, false, null, _opt);
        }

        [Command("startround")]
        [Summary("Once the players are all gathered, starts the holdem round.")]
        public async Task StartroundAsync()
        {

            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            game.players.ForEach(e => e.hole = new List<StandardCard>());

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
        [Summary("Lists the players at the table.")]
        public async Task HoldemtableAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            List<string> rtn_msg = new List<string>();

            rtn_msg.Add("Players at the Table:");

            game.players.ForEach(e => rtn_msg.Add(generate_name(Context.Guild.GetUser(e.ID))));

            await ReplyAsync(string.Join(System.Environment.NewLine, rtn_msg), false, null, _opt);
        }

        [Command("holdemchips")]
        [Summary("Shows the amount of chips you have.")]
        public async Task HoldemchipsAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            await ReplyAsync(game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool.ToString(), false, null, _opt);
        }

        [Command("holdempot")]
        [Summary("How many chips are in the pot right now.")]
        public async Task HoldempotAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            await ReplyAsync(game.current_round.pot.ToString() + " imperial credits.", false, null, _opt);
        }

        [Command("holdembet")]
        [Summary("Starts the betting")]
        public async Task HoldembetAsync(int amount)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            if (!(in_bet_position(Context, Context.Message.Author.Id)))
            {
                await ReplyAsync("You are not in a position to bet, please wait.", false, null, _opt);
                return;
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

            if (last_bet_amount != 0)
            {
                await ReplyAsync("You cannot make a bet when the bet has already been initiated. Use the tb!holdemraise command to raise a bet or the tb!holdemcall to call a bet.", false, null, _opt);
                return;
            }

            game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool -= amount;

            game.current_round.pot += amount;

            game.current_round.call_position = determine_next_call_index(game.current_round.call_position, game.players);

            game.current_round.call_count = 0;

            game.current_round.call_count++;

            if (game.current_round.call_count >= game.players.Count())
            {
                await lay_down_next(Context);
                game.current_round.call_position = game.dealer_index + 1;
            }

            if (game.current_round.stop)
            {
                game.current_round = null;
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)), false, null, _opt);
        }

        [Command("holdemcheck")]
        [Summary("Advances betting without betting chips. Can only be used at the start of a round or after a check.")]
        public async Task HoldemcheckAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            if (!(in_bet_position(Context, Context.Message.Author.Id)))
            {
                await ReplyAsync("You are not in a position to bet, please wait.", false, null, _opt);
                return;
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

            if (last_bet_amount != 0)
            {
                await ReplyAsync("You cannot check when a bet has been placed. Use the tb!holdemraise command to raise a bet or the tb!holdemcall to call a bet.", false, null, _opt);
                return;
            }

            game.current_round.call_position = determine_next_call_index(game.current_round.call_position, game.players);

            game.current_round.call_count++;

            if (game.current_round.call_count >= game.players.Count())
            {
                await lay_down_next(Context);
                game.current_round.call_position = game.dealer_index + 1;
            }

            if (game.current_round.stop)
            {
                game.current_round = null;
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)), false, null, _opt);
        }

        [Command("holdemcall")]
        [Summary("Calls the last bet made.")]
        public async Task HoldemcallAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;
            if (!(in_bet_position(Context, Context.Message.Author.Id)))
            {
                await ReplyAsync("You are not in a position to bet, please wait.", false, null, _opt);
                return;
            }

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

            game.current_round.call_count = 0;

            game.current_round.call_count++;

            if (game.current_round.call_count >= game.players.Count())
            {
                await lay_down_next(Context);
                game.current_round.call_position = game.dealer_index + 1;
            }

            if (game.current_round.stop)
            {
                game.current_round = null;
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)), false, null, _opt);
        }

        [Command("holdemraise")]
        [Summary("Raises the previous bet by the amount indicated.")]
        public async Task HoldemraiseAsync(int amount)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            if (!(in_bet_position(Context, Context.Message.Author.Id)))
            {
                await ReplyAsync("You are not in a position to bet, please wait.", false, null, _opt);
                return;
            }

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

            game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool -= amount - last_player_bet_amount;

            game.current_round.pot += amount - last_player_bet_amount;

            game.current_round.call_position = determine_next_call_index(game.current_round.call_position, game.players);

            game.current_round.call_count++;

            game.current_round.call_count = 0;

            if (game.current_round.call_count >= game.players.Count())
            {
                await lay_down_next(Context);
                game.current_round.call_position = game.dealer_index + 1;
            }

            if (game.current_round.stop)
            {
                game.current_round = null;
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)), false, null, _opt);
        }

        [Command("holdemallin")]
        [Summary("For the truly brave.")]
        private async Task AllinAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            if (!(in_bet_position(Context, Context.Message.Author.Id)))
            {
                await ReplyAsync("You are not in a position to bet, please wait.", false, null, _opt);
                return;
            }

            game.current_round.pot += (int)game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool;

            game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool = 0;
            
            if(game.players.Where(e=>!e.fold && !e.allin).Count() == 0)
            {
                determine_winner(Context);
            }

            game.current_round.call_position = determine_next_call_index(game.current_round.call_position, game.players);

            game.current_round.call_count = 0;

            game.current_round.call_count++;

            if (game.current_round.call_count >= game.players.Count())
            {
                await lay_down_next(Context);
                game.current_round.call_position = game.dealer_index + 1;
            }

            if (game.current_round.stop)
            {
                game.current_round = null;
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)), false, null, _opt);
        }

        [Command("holdemfold")]
        [Summary("For lily-livered pole-cats.")]
        public async Task HoldemfoldAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            if (!(in_bet_position(Context, Context.Message.Author.Id)))
            {
                await ReplyAsync("You are not in a position to bet, please wait.", false, null, _opt);
                return;
            }

            game.players.First(e => e.ID == Context.Message.Author.Id).fold = true;

            if(game.players.Where(e=>!e.fold).Count() == 1)
            {
                we_have_a_winner(game.players.First(e=>!e.fold), Context);
            }

            if(game.players.Where(e=>!e.fold && !e.allin).Count() == 0)
            {
                determine_winner(Context);
            }


            game.current_round.call_position = determine_next_call_index(game.current_round.call_position, game.players);

            game.current_round.call_count++;

            if (game.current_round.call_count >= game.players.Count())
            {
                await lay_down_next(Context);
                game.current_round.call_position = game.dealer_index + 1;
            }

            if (game.current_round.stop)
            {
                game.current_round = null;
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)), false, null, _opt);
        }

        [Command("holdemleave")]
        
        public async Task HoldemleaveAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            if (game.current_round != null)
            {
                await ReplyAsync("You cannot remove yourself from a game currently in progress.", false, null, _opt);
                return;
            }

            await game.players.First(e => e.ID == Context.Message.Author.Id).Save();

            game.players.Remove(game.players.First(e => e.ID == Context.Message.Author.Id));

            await ReplyAsync(generate_name(Context.Guild.GetUser(Context.Message.Author.Id)) + " has removed themselves from the game", false, null, _opt);
        }

        [Command("holdemestop")]
        [Summary("For Administrators. Immediately stops the game currently going on.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task HoldemestopAsync()
        {
            Program.HoldEm.Remove(Program.HoldEm.First(e => e.Key == Context.Channel.Id).Key);
            ;

            await ReplyAsync("Game halted.");

        }

        private async Task lay_down_next(SocketCommandContext context)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == context.Channel.Id).Value;
            game.current_round.call_count = 0;
            game.current_round.bets = new List<player_bet>();

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

                await context.Channel.SendMessageAsync(rtner, false, null, _opt);

                return;
            }

            if (game.current_round.turn == null)
            {
                game.current_round.turn = game.deck.Pop();

                List<string> message = new List<string>();

                message.Add("The Turn:");

                message.Add(StandardCard.value_to_output[game.current_round.turn.value].ToString() + " of " + StandardCard.suit_to_output[game.current_round.turn.suit].ToString());

                string rtner = string.Join(System.Environment.NewLine, message);

                await context.Channel.SendMessageAsync(rtner, false, null, _opt);

                return;
            }

            if (game.current_round.river == null)
            {
                game.current_round.river = game.deck.Pop();

                List<string> message = new List<string>();

                message.Add("The River:");

                message.Add(StandardCard.value_to_output[game.current_round.river.value].ToString() + " of " + StandardCard.suit_to_output[game.current_round.river.suit].ToString());

                string rtner = string.Join(System.Environment.NewLine, message);

                await context.Channel.SendMessageAsync(rtner, false, null, _opt);

                return;
            }

            //if we get here, that means that all three rounds of betting have been completed and its time to calculate the hands and determine a winner

            determine_winner(context);
        }

        private static void determine_winner(SocketCommandContext context)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == context.Channel.Id).Value;

            foreach (var player in game.players)
            {
                List<StandardCard> cumulative_hand = new List<StandardCard>();

                player.hole.ForEach(e => cumulative_hand.Add(e));

                game.current_round.flop.ForEach(e => cumulative_hand.Add(e));

                cumulative_hand.Add(game.current_round.turn);

                cumulative_hand.Add(game.current_round.river);

                player.hand_weight = Classes.StandardCard.eval_hand(cumulative_hand);
            }

            Player winner = null;

            if (game.players.OrderByDescending(e => e.hand_weight).ToArray()[0].hand_weight == game.players.OrderByDescending(e => e.hand_weight).ToArray()[1].hand_weight)
            {
                int first_max = game.players.OrderByDescending(e => e.hand_weight).ToArray()[0].hole.OrderByDescending(e => e.value).First().value;

                int second_max = game.players.OrderByDescending(e => e.hand_weight).ToArray()[1].hole.OrderByDescending(e => e.value).First().value;

                if (first_max > second_max)
                {
                    winner = game.players.OrderByDescending(e => e.hand_weight).ToArray()[0];
                }
                else
                {
                    winner = game.players.OrderByDescending(e => e.hand_weight).ToArray()[1];
                }
            }
            else
            {
                winner = game.players.OrderByDescending(e => e.hand_weight).First();
            }



            we_have_a_winner(winner, context);
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

        private static int determine_next_call_index(int current_index, List<Player> players)
        {
            int rtn = 0;

            // rtn = players.FindIndex(current_index, players.Count - 1, e => !e.fold && !e.allin);

            Player[] eligible = new Player[players.Count()];

            players.CopyTo(eligible);

            eligible[current_index] = null;

            for (int i = current_index + 1; i < eligible.Length; i++)
            {
                if (!eligible[i].fold && !eligible[i].allin)
                {
                    rtn = i;

                    return rtn;
                }
            }

            if (rtn == 0)
            {
                for (int i = 0; i < eligible.Length; i++)
                {
                    if (eligible[i] != null)
                    {
                        if (!eligible[i].fold && !eligible[i].allin)
                        {
                            rtn = i;

                            return rtn;
                        }
                    }
                }

            }

            return rtn;


            // if (rtn == -1)
            // {
            //     rtn = players.FindIndex(0, players.Count - 1, e => e.fold == false);
            // }


            // return rtn;
        }

        private static void we_have_a_winner(Player player, SocketCommandContext con)
        {
            SocketCommandContext cont = con;
            HoldEm game = Program.HoldEm.First(e => e.Key == cont.Channel.Id).Value;

            SocketGuildUser usr = cont.Guild.GetUser(player.ID);

            List<string> hole_cards = new List<string>();

            game.current_round.stop = true;

            game.dealer_index = (game.dealer_index + 1) % game.players.Count();

            player.hole.ForEach(e => hole_cards.Add(StandardCard.value_to_output[e.value].ToString() + " of " + StandardCard.suit_to_output[e.suit].ToString()));

            string msg = generate_name(usr) + " has won the round, pot size " + game.current_round.pot.ToString() + " imperial credits. Hole Cards were " + System.Environment.NewLine + string.Join(System.Environment.NewLine, hole_cards) + System.Environment.NewLine + "Use tb!startround to begin another round.";

            player.cash_pool += game.current_round.pot;

            cont.Channel.SendMessageAsync(msg, false, null, PlayHoldem._opt).GetAwaiter().GetResult();
        }

        public static bool in_bet_position(SocketCommandContext con, ulong ID)
        {
            bool rtn = false;
            SocketCommandContext cont = con;
            HoldEm game = Program.HoldEm.First(e => e.Key == cont.Channel.Id).Value;

            if (get_usr_from_index(con, game.current_round.call_position).Id == ID)
            {
                rtn = true;
            }



            return rtn;
        }


    }
}
