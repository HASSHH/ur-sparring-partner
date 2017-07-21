using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UrSparringPartner.Engines
{
    public class RandomEngine : ISparringPartner
    {
        private Random _rng = new Random();

        public RandomEngine(Board board)
        {
            Board = board;
        }

        public event MoveFoundDelegate MoveFound;

        public Board Board { get; set; }

        public void GoMove()
        {
            List<Board.Move> possibleMoves = Board.GetPossibleMoves();
            if(possibleMoves.Count > 0)
            {
                int index = _rng.Next(possibleMoves.Count);
                Board.Move move = possibleMoves[index];
                MoveFound?.Invoke(move);
            }
        }

        public Task GoMove(CancellationToken ct)
        {
            return Task.Run(() => GoMove());
        }
    }
}
