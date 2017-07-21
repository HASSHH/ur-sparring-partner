using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UrSparringPartner.Engines
{
    /// <summary>
    /// The strategy this engine is using is to push the most advanced piece he can.
    /// </summary>
    public class AdvanceFirstEngine : ISparringPartner
    {
        public AdvanceFirstEngine(Board board)
        {
            Board = board;
        }

        public event MoveFoundDelegate MoveFound;

        public Board Board { get; set; }

        public void GoMove()
        {
            List<Board.Move> possibleMoves = Board.GetPossibleMoves();
            if (possibleMoves.Count > 0)
            {
                //Get the move that moves the furthest piece on the board - Due to how moves are generated it should be the last one in the list but I wish to avoid dependency
                Board.Move move = possibleMoves[possibleMoves.Count -1];
                foreach(Board.Move m in possibleMoves)
                    if (m.From > move.From)
                        move = m;
                MoveFound?.Invoke(move);
            }
        }

        public Task GoMove(CancellationToken ct)
        {
            return Task.Run(() => GoMove());
        }
    }
}
