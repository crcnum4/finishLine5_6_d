﻿using System;
using System.Collections.Generic;

namespace FinishLineDaytime
{

    public class Die
    {
        public bool isRed;
        public int val;
        public int sides;

        public Die(int sides, bool isRed)
        {
            this.sides = sides;
            this.isRed = isRed;
            this.val = 1;
        }

        public Die()
        {
            this.sides = 6;
            this.val = 1;
        }

        public void Roll(Random rand)
        {
            this.val = rand.Next(1, this.sides + 1);
        }
    }

    public class Card
    {
        public string suit;
        public int val;
        private readonly Dictionary<int, string> FACE_MAP = new Dictionary<int, string>() {
        {1, "Ac"},
        {10, "10"},
        {11, "Ja"},
        {12, "Qu"},
        {13, "Ki"}
    };

        public Card(string suit, int val)
        {
            this.suit = suit;
            this.val = val;
        }

        public string Display()
        {
            if (this.val == 0)
            {
                return "Jkr";
            }

            if (FACE_MAP.ContainsKey(this.val))
            {
                return this.suit + this.FACE_MAP[this.val];
            }
            return this.suit + "0" + this.val;
        }
    }

    public class Deck
    {
        public List<Card> cards = new List<Card>();

        public Deck(string[] suits, int[] values, int numOfJkrs)
        {
            foreach (var suit in suits)
            {
                foreach (var val in values)
                {
                    this.cards.Add(new Card(suit, val));
                }
            }
            for (int jkr = 0; jkr < numOfJkrs; jkr++)
            {
                this.cards.Add(new Card("", 0));
            }
        }

        public void Shuffle(Random rand)
        {
            for (int index = this.cards.Count - 1; index > 0; index--)
            {
                int position = rand.Next(index + 1);
                //(this.cards[position], this.cards[index]) = (this.cards[index], this.cards[position]);
                Card temp = this.cards[index];
                this.cards[index] = this.cards[position];
                this.cards[position] = temp;
            }
        }
    }

    public class Marker
    {
        public int position;
        public string name;

        public Marker(string name)
        {
            this.position = -1;
            this.name = name;
        }

        public void Move(int spaces)
        {
            this.position += spaces;
        }
    }

    public class FLMarker : Marker
    {
        public bool stopped;
        public FLMarker(string name) : base(name)
        {
            return;
        }

        public void Move(int spaces, int stopValue, Deck gameDeck)
        {
            // add in Finish line logic
            for (int counter = 1; counter <= spaces; counter++)
            {
                if (this.position + counter >= gameDeck.cards.Count)
                {
                    return;
                }   
                if (gameDeck.cards[this.position + counter].val >= stopValue)
                {
                    base.Move(counter);
                    return;
                }
            }
            base.Move(spaces);
        }
    }

    public class Player
    {
        public string name;
        public FLMarker[] markers;
        public int finishedMarkers = 0;

        public Player(string name, string[] markers)
        {
            this.name = name;
            this.markers = new FLMarker[markers.Length];
            for (int markerName = 0; markerName < markers.Length; markerName++)
            {
                this.markers[markerName] = new FLMarker(markers[markerName]);
            }
        }

        public string hasMarkersAt(int position)
        {
            string master = "";
            foreach (var marker in this.markers)
            {
                if (marker.position == position)
                {
                    master += marker.name;
                }
                else
                {
                    master += " ";
                }
            }
            return master;
        }

    }

    public class FinishLine
    {
        private readonly List<int> RESTRICTED_VALUES = new List<int> { 0, 1, 2, 11, 12, 13 };
        private readonly int[] RESTRICED_POSITIONS = new int[] { 0, 1, 2, 51, 52, 53 };
        private const int NUM_JOKERS = 2;
        private readonly string[] SUITS = new string[] { "\u2663", "\u2660", "\u2665", "\u2666" };
        private readonly int[] VALUES = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        private readonly string[] MARKER_NAMES = new string[] { "1", "2", "3" };

        public Die redDie;
        public Die blackDie;
        public Deck deck;
        public int numPlayers;
        public Player[] players;
        private Random rand = new Random();
        public bool noWinner = true;

        public FinishLine(int numPlayers, string[] playerNames)
        {
            this.numPlayers = numPlayers;
            this.players = new Player[numPlayers];
            for (var count = 0; count < playerNames.Length; count++)
            {
                this.players[count] = new Player(playerNames[count], MARKER_NAMES);
            }
            this.redDie = new Die(6, true);
            this.blackDie = new Die(6, false);
            this.deck = new Deck(this.SUITS, this.VALUES, NUM_JOKERS);
            this.rand = new Random();
            this.deck.Shuffle(this.rand);
            ValidateDeck();
            this.redDie.Roll(this.rand);
            this.blackDie.Roll(this.rand);
        }

        private void InitializePlayerRow(string[] playerRow)
        {
            for (int count = 0; count < players.Length; count++)
            {
                playerRow[count] = "\t";
            }
        }

        private void addToPlayerRow(string[] playerRow, int position, string pre, string post)
        {
            for (int count = 0; count < players.Length; count++)
            {
                playerRow[count] += pre + this.players[count].hasMarkersAt(position) + post;
            }
        }

        private void DisplayBoard()
        {
            Console.Clear();
            string master = "";
            string cardRow = "\t";
            string[] playerRow = new string[this.numPlayers];
            InitializePlayerRow(playerRow);
            foreach (var player in players)
            {
                cardRow += player.name + "\t";
            }
            for (int count = 0; count < players.Length; count++)
            {
                playerRow[0] += "" + this.players[count].hasMarkersAt(-1) + "\t";
            }
            //addToPlayerRow(playerRow, -1, "", "\t");
            master += cardRow + "\n" + playerRow[0] + "\n\n";
            cardRow = "\t";
            InitializePlayerRow(playerRow);

            int counter = 0;
            foreach (Card card in this.deck.cards)
            {
                cardRow += "[" + card.Display() + "]";
                addToPlayerRow(playerRow, counter, " ", " ");
                counter++;
                if (counter % 9 == 0)
                {
                    master += cardRow + "\n";
                    foreach(string row in playerRow)
                    {
                        master += row + "\n";
                    }
                    master += "\n";
                    cardRow = "\t";
                    InitializePlayerRow(playerRow);
                }
                else
                {
                    cardRow += "\t";
                    for (int count = 0; count < players.Length; count++)
                    {
                        playerRow[count] += "\t";
                    }
                }
            }
            Console.WriteLine(master);
        }

        private void ValidateCard(int position)
        {
            if (RESTRICTED_VALUES.Contains(this.deck.cards[position].val))
            {
                // swap the card
                while (true)
                {
                    //3-50
                    int newPosition = this.rand.Next(3, 51);
                    if (RESTRICTED_VALUES.Contains(this.deck.cards[newPosition].val))
                    {
                        continue;
                    }
                    Card temp = this.deck.cards[position];
                    this.deck.cards[position] = this.deck.cards[newPosition];
                    this.deck.cards[newPosition] = temp;
                    break;
                }
            }
        }

        private void ValidateDeck()
        {
            // check first and last 3 cards for restricted values. swap with a valid card from the center.
            int[] positions = new int[] { 0, 1, 2, 51, 52, 53 };
            foreach (int position in positions)
            {
                ValidateCard(position);
            }
        }

        private void Turn(Player player)
        {
            DisplayBoard();
            string master = "";
            master += player.name + "'s turn";
            this.redDie.Roll(this.rand);
            this.blackDie.Roll(this.rand);
            int stopValue = this.redDie.val + this.blackDie.val;
            master += "\nRed: " + this.redDie.val + "\tBlack: " + this.blackDie.val + "\t Stop Value: " + stopValue;
            // TODO: Ask for die and marker usage

            player.markers[chooseMarker("Red", master)].Move(this.redDie.val, stopValue, this.deck);
            DisplayBoard();
            
            player.markers[chooseMarker("Black", master)].Move(this.blackDie.val, stopValue, this.deck);
            DisplayBoard();
        }

        private int chooseMarker(string dieString, string master)
        {
            Console.WriteLine(master);
            Console.WriteLine("Choose marker(123) for {0} die", dieString);
            return Convert.ToInt32(Console.ReadLine()) - 1;
        }

        private Player Round()
        {
            //TODO loop through players
            foreach (var player in players)
            {
                this.Turn(player);
                if (didWin(player))
                {
                    return player;
                }
            }
            return null;
        }

        public void PlayGame()
        {
            while (this.noWinner)
            {
                Player winner = this.Round();
                if ( winner != null)
                {
                    Console.WriteLine("{0} has won!!! yay.", winner.name);
                    break;
                }
            }
        }

        private bool didWin(Player player)
        {
            return (player.hasMarkersAt(53) == "123");
        }

    }

    class MainClass
    {
        public static void Main(string[] args)
        {
            FinishLine game = new FinishLine(4, new string[] {"Cliff", "Jackie", "Arnell", "Isaac"});
            game.PlayGame();
        }
    }
}
