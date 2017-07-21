using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrSparringPartner
{
    public class Board
    {
        const int TotalPieces = 7;

        public Board()
        {
            InitializeBoard();
        }

        public delegate void GameEndedDelegate(string winner);

        public int DiceValue { get; set; }
        public string NextToMove { get; set; }
        public List<int> BlackPieces { get; set; }
        public List<int> WhitePieces { get; set; }
        public BoardState State { get; private set; }

        public void InitializeBoard()
        {
            DiceValue = -1;
            NextToMove = "white";
            BlackPieces = new List<int> { TotalPieces, 0, 0, 0, 0, 0, 0 ,0 ,0 ,0 ,0, 0, 0, 0, 0, 0 };
            WhitePieces = new List<int> { TotalPieces, 0, 0, 0, 0, 0, 0 ,0 ,0 ,0 ,0, 0, 0, 0, 0, 0 };
            State = BoardState.Active;
        }

        public List<Move> GetPossibleMoves()
        {
            List<Move> possibleMoves = new List<Move>();
            if(DiceValue > 0)
            {
                List<int> ours, theirs;
                if (NextToMove == "white")
                {
                    ours = WhitePieces;
                    theirs = BlackPieces;
                }
                else
                {
                    ours = BlackPieces;
                    theirs = WhitePieces;
                }
                for(int from = 0; from < 16 - DiceValue; ++from)
                {
                    int to = from + DiceValue;
                    //can move if we dont have a piece on that spot + if it's square 8 (netral safe spot) only when enemy doesn't have a piece there
                    if (ours[from] > 0 && (to == 15 || ours[to] == 0) && (to != 8 || theirs[to] == 0))
                        possibleMoves.Add(new Move(from, to));
                }
            }
            return possibleMoves;
        }

        public void DoMove(Move move)
        {
            if(move.From >= 0 && move.From < 16 && move.To >= 0 && move.To < 16)
            {
                List<int> ours, theirs;
                if (NextToMove == "white")
                {
                    ours = WhitePieces;
                    theirs = BlackPieces;
                }
                else
                {
                    ours = BlackPieces;
                    theirs = WhitePieces;
                }
                if(ours[move.From] > 0 && (move.To == 15 || ours[move.To] == 0) && (move.To != 8 || theirs[move.To] == 0))
                {
                    --ours[move.From];
                    ++ours[move.To];
                    if(move.To > 4 && move.To < 13 && theirs[move.To] > 0)
                    {
                        --theirs[move.To];
                        ++theirs[0];
                    }
                    DiceValue = -1;
                    if (move.To != 4 && move.To != 8 && move.To != 14)
                        NextToMove = NextToMove == "white" ? "black" : "white";
                    if (move.To == 15 && ours[15] == TotalPieces)
                        //game has ended
                        if (ours == WhitePieces)
                            State = BoardState.Ended_W;
                        else
                            State = BoardState.Ended_B;
                }
            }
        }

        public Board Clone()
        {
            Board clone = new Board();
            clone.DiceValue = DiceValue;
            clone.State = State;
            clone.NextToMove = NextToMove;
            for(int i = 0; i < 16; ++i)
            {
                clone.WhitePieces[i] = WhitePieces[i];
                clone.BlackPieces[i] = BlackPieces[i];
            }
            return clone;
        }

        public class Move
        {
            public Move(int from, int to)
            {
                From = from;
                To = to;
            }

            public int From { get; set; }
            public int To { get; set; }
        }

        public enum BoardState
        {
            Active,
            Ended_W, //white victory
            Ended_B  //black victory
        }
    }
}
