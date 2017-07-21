using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace UrSparringPartner
{
    class Program
    {
        static WebSocket _ws;
        static Game _game;

        static void Main(string[] args)
        {
            string url = ConfigurationManager.AppSettings["server-url"];
            if (url == null)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Configuration file does not contain 'server-url'");
                Console.ResetColor();
            }
            else
                using (var ws = new WebSocket(url, protocols: new string[] { "ur-protocol" }))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Connected to server: {0}", url);
                    Console.ResetColor();
                    _ws = ws;
                    ws.Log.Level = LogLevel.Fatal;
                    ws.OnOpen += OnWsOpen;
                    ws.Connect();

                    while (_ws.IsAlive)
                        ;
                }
            Console.ReadLine();
        }

        private static void OnWsOpen(object sender, EventArgs e)
        {
            _game = new Game(_ws);
            Console.Write("1.New game\n2.Join game\n\toption = ");
            string opt = Console.ReadLine();
            switch (opt)
            {
                case "1":
                    _game.NewGame();
                    break;
                case "2":
                    Console.Write("Connect to game with ID: ");
                    string gameId = Console.ReadLine();
                    _game.JoinGame(gameId);
                    break;
                default:
                    break;
            }
        }
    }
}
