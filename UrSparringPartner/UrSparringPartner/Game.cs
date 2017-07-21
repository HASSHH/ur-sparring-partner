using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UrSparringPartner.Engines;
using WebSocketSharp;

namespace UrSparringPartner
{
    class Game
    {
        //The amount of delay (in miliseconds) added when sending to server; so it's not 'instant' roll-move
        private int _serverResponseDelayMs;
        private WebSocket _ws;

        public Game(WebSocket ws)
        {
            _ws = ws;
            _ws.OnMessage += OnWsMessage;
            Board = new Board();
            MyScore = 0;
            OpponentScore = 0;
            string delayValue = ConfigurationManager.AppSettings["server-response-delay-ms"];
            if (delayValue == null || !int.TryParse(delayValue, out _serverResponseDelayMs))
                _serverResponseDelayMs = 1000;
        }

        public Board Board { get; set; }
        public string GameId { get; set; }
        public string OurColor { get; set; }
        public ISparringPartner SparringPartner { get; set; }
        public int MyScore { get; set; }
        public int OpponentScore { get; set; }


        private void OnWsMessage(object sender, MessageEventArgs e)
        {
            string data = e.Data;
            try
            {
                JObject message = JObject.Parse(data);
                if(message["action"] != null)
                {
                    string action = message["action"].Value<string>();
                    switch (action)
                    {
                        case "wait-for-game":
                            WaitNewGame(message["body"]);
                            break;
                        case "game-started":
                            GameStarted(message["body"]);
                            Console.WriteLine("Game started");
                            break;
                        case "update-dice-roll":
                            UpdateDice(message["body"]);
                            break;
                        case "update-with-move":
                            UpdateWithMove(message["body"]);
                            break;
                        case "opponent-left":
                            Console.WriteLine("Opponent left");
                            break;
                        default:
                            break;
                    }
                }
            }
            catch{ }
        }

        //////////////////
        //  FROM SERVER //
        //////////////////

        private void UpdateDice(JToken msg)
        {
            if(msg["dice"] != null && msg["endTurn"] != null)
            {
                try
                {
                    int dice = msg["dice"].Value<int>();
                    bool endTurn = msg["endTurn"].Value<bool>();
                    if(dice >= 0 && dice <= 4)
                    {
                        Board.DiceValue = dice;
                        if (endTurn)
                        {
                            Board.NextToMove = Board.NextToMove == "white" ? "black" : "white";
                            if (Board.NextToMove == OurColor)
                                RollDice();
                        }
                        else if (Board.NextToMove == OurColor)
                            SparringPartner.GoMove();
                    }
                }
                catch { }
            }
        }

        private void UpdateWithMove(JToken msg)
        {
            if(msg["start"] != null && msg["stop"] != null && msg["nextToMove"] != null)
            {
                try
                {
                    int start = msg["start"].Value<int>();
                    int stop = msg["stop"].Value<int>();
                    string nextToMove = msg["nextToMove"].Value<string>();
                    Board.Move move = new Board.Move(start, stop);
                    Board.DoMove(move);
                    if (Board.State == Board.BoardState.Active)
                    {
                        if (Board.NextToMove == OurColor)
                            RollDice();
                    }
                    else
                    {
                        //game is over
                        string winnerColor = Board.State == Board.BoardState.Ended_W ? "white" : "black";
                        if (OurColor == winnerColor)
                            ++MyScore;
                        else
                            ++OpponentScore;
                        Console.WriteLine("Game ended. Score: Me {0} Vs {1} Opponent", MyScore, OpponentScore);
                        Rematch();
                    }
                }
                catch { }
            }
        }

        private void GameStarted(JToken msg)
        {
            if(msg["id"] != null && msg["color"] != null)
            {
                GameId = msg["id"].Value<string>();
                OurColor = msg["color"].Value<string>();
                Board.InitializeBoard();
                string engineChoice = ConfigurationManager.AppSettings["engine-mode"];
                if (engineChoice == null)
                    engineChoice = "random";
                Console.ForegroundColor = ConsoleColor.Blue;
                switch (engineChoice)
                {
                    case "random":
                        SparringPartner = new RandomEngine(Board);
                        Console.WriteLine("Starting 'random' engine.");
                        break;
                    case "advance":
                        SparringPartner = new AdvanceFirstEngine(Board);
                        Console.WriteLine("Starting 'advance first' engine.");
                        break;
                    case "minimax":
                        SparringPartner = new ProbabilisticMinimaxEngine(Board);
                        Console.WriteLine("Starting 'minimax' engine.");
                        break;
                    default:
                        SparringPartner = new RandomEngine(Board);
                        Console.WriteLine("Starting 'random' engine.");
                        break;
                }
                Console.ResetColor();
                SparringPartner.MoveFound += SparringPartner_MoveFound;
                if (OurColor == "white")
                    RollDice();

            }
        }

        private void WaitNewGame(JToken msg)
        {
            if(msg["code"] != null)
            {
                string gameCode = msg["code"].Value<string>();
                Console.WriteLine("Waiting for opponent. Game code: {0}", gameCode);
            }
        }

        //////////////////
        //  TO SERVER   //
        //////////////////
        public void NewGame()
        {
            JObject jObject = JObject.FromObject(new
            {
                action = "start-new-game"
            });
            Thread.Sleep(_serverResponseDelayMs);
            _ws.Send(jObject.ToString());
        }

        public void JoinGame(string gameId)
        {
            JObject jObject = JObject.FromObject(new
            {
                action = "join-game",
                body = new
                {
                    code = gameId
                }
            });
            Thread.Sleep(_serverResponseDelayMs);
            _ws.Send(jObject.ToString());
        }

        private void RollDice()
        {
            JObject jObject = JObject.FromObject(new
            {
                action = "roll-dice",
                body = new
                {
                    id = GameId
                }
            });
            Thread.Sleep(_serverResponseDelayMs);
            _ws.Send(jObject.ToString());
        }

        private void Rematch()
        {
            JObject jObject = JObject.FromObject(new
            {
                action = "rematch",
                body = new
                {
                    id = GameId
                }
            });
            Thread.Sleep(_serverResponseDelayMs);
            _ws.Send(jObject.ToString());
        }

        private void SparringPartner_MoveFound(Board.Move move)
        {
            JObject jObject = JObject.FromObject(new
            {
                action = "do-move",
                body = new
                {
                    id = GameId,
                    cell = move.From
                }
            });
            Thread.Sleep(_serverResponseDelayMs);
            _ws.Send(jObject.ToString());
        }
    }
}
