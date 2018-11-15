using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JsonFlatFileDataStore;
using Newtonsoft.Json;
using timebot.Classes;

namespace timebot.Classes
{

    public class Player
    {
        public List<StandardCard> hole { get; set; } = new List<StandardCard>();
        public UInt64 ID { get; set; }
        public long cash_pool { get; set; }
        public bool fold { get; set; } = false;
        public bool allin { get; set; } = false;
        public int hand_weight{get;set;}=0;
    }

    public class player_bet
    {
        public UInt64 player_id { get; set; }
        public int amount { get; set; }
    }

    public class betting_round
    {
        public List<StandardCard> flop { get; set; }
        public StandardCard turn { get; set; }
        public StandardCard river { get; set; }
        public List<player_bet> bets { get; set; } = new List<player_bet>();
        public int call_count {get;set;} = 0;
        public int pot { get; set; }
        public int max_bet_position { get; set; }
        public int call_position { get; set; }
        public bool stop {get;set;}=false;
    }

    public class HoldEm
    {
        public Stack<StandardCard> deck { get; set; } 
        // public int card_round { get; set; } = 0;
        public betting_round current_round {get;set;}
        public int big_blind { get; set; } = 500;
        public int small_blind { get; set; }
        public int ante { get; set; } = 50;
        public int dealer_index { get; set; } = 0;
        public int min_raise { get; set; } = 50;
        public int min_bet { get; set; }
        public List<Player> players { get; set; }

        public HoldEm()
        {
            this.deck = StandardCard.shuffleDeck(StandardCard.straightDeck());
            this.players = new List<Player>();
            this.small_blind = this.big_blind / 2;
            this.min_bet = small_blind;

            this.players.Add(new Player{
                ID = 111259937123897344,
                cash_pool = 100000
            });

            this.players.Add(new Player{
                ID = 111259937123897344,
                cash_pool = 100000
            });
            this.players.Add(new Player{
                ID = 111259937123897344,
                cash_pool = 100000
            });
            this.players.Add(new Player{
                ID = 111259937123897344,
                cash_pool = 100000
            });

        }
    }



}
