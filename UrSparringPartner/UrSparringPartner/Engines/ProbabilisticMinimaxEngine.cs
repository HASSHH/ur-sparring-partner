using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UrSparringPartner.Engines
{
    public class ProbabilisticMinimaxEngine : ISparringPartner
    {
        private int _depth;
        private double[] _diceProb = { 0.0625, 0.25, 0.375, 0.25, 0.0625 };
        private double[] _magicEvalValues = { 0, 100, 210, 330, 460, 600, 750, 810, 990, 1170, 1360, 1560, 1770, 1990, 2220, 2460 };

        public ProbabilisticMinimaxEngine(Board board)
        {
            Board = board;
            string depthValue = ConfigurationManager.AppSettings["minimax-depth"];
            if (depthValue == null || !int.TryParse(depthValue, out _depth))
                _depth = 4;
        }

        public event MoveFoundDelegate MoveFound;

        public Board Board { get; set; }

        public void GoMove()
        {
            List<Board.Move> possibleMoves = Board.GetPossibleMoves();
            if (possibleMoves.Count > 0)
            {
                double bestScore = double.MinValue;
                Board.Move bestMove = null;
                foreach (Board.Move move in possibleMoves)
                {
                    Board nextBoard = Board.Clone();
                    nextBoard.DoMove(move);
                    double score = nextBoard.NextToMove == "white"
                        ? AlphaBetaMax(nextBoard, _depth, double.MinValue, double.MaxValue)
                        : AlphaBetaMin(nextBoard, _depth, double.MinValue, double.MaxValue);
                    if (Board.NextToMove == "black")
                        score *= -1;
                    if(score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }
                MoveFound?.Invoke(bestMove);
            }
        }

        public Task GoMove(CancellationToken ct)
        {
            return Task.Run(() => GoMove());
        }

        /// <summary>
        /// The max function of alphabeta.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="depth"></param>
        /// <param name=""></param>
        /// <returns>The score multiplied with the probability of that branch</returns>
        private double AlphaBetaMax(Board board, int depth, double alpha, double beta)
        {
            //if we are on a leaf node in the game tree we simply return the score of the position (probability 1)
            if (depth == 0)
                return Evaluate(board);
            //else we generate the next nodes (for each possible dice value -probability- and each possible move for those values)
            for (int dice = 0; dice <= 4; ++dice)
            {
                double prob = _diceProb[dice];
                board.DiceValue = dice;
                List<Board.Move> moves = board.GetPossibleMoves();
                if (moves.Count > 0)
                    foreach (Board.Move move in moves)
                    {
                        Board nextBoard = board.Clone();
                        nextBoard.DoMove(move);
                        double score;
                        if (nextBoard.State != Board.BoardState.Active)
                            score = Evaluate(nextBoard);
                        else
                            score = nextBoard.NextToMove == "white" 
                                ? AlphaBetaMax(nextBoard, depth - 1, alpha, beta) 
                                : AlphaBetaMin(nextBoard, depth - 1, alpha, beta);
                        double actualScore = prob * score;
                        if (actualScore >= beta)
                            return actualScore;//beta cutoff;
                        if (actualScore > alpha)
                            alpha = actualScore;
                    }
                else
                {
                    Board nextBoard = board.Clone();
                    nextBoard.NextToMove = nextBoard.NextToMove == "white" ? "black" : "white";
                    double score = nextBoard.NextToMove == "white" 
                        ? AlphaBetaMax(nextBoard, depth - 1, alpha, beta) 
                        : AlphaBetaMin(nextBoard, depth - 1, alpha, beta);
                    double actualScore = prob * score;
                    if (actualScore >= beta)
                        return actualScore;//beta cutoff;
                    if (actualScore > alpha)
                        alpha = actualScore;
                }
            }
            return alpha;
        }

        private double AlphaBetaMin(Board board, int depth, double alpha, double beta)
        {
            //if we are on a leaf node in the game tree we simply return the score of the position (probability 1)
            if (depth == 0)
                return Evaluate(board);
            //else we generate the next nodes (for each possible dice value -probability- and each possible move for those values)
            for (int dice = 0; dice <= 4; ++dice)
            {
                double prob = _diceProb[dice];
                board.DiceValue = dice;
                List<Board.Move> moves = board.GetPossibleMoves();
                if (moves.Count > 0)
                    foreach (Board.Move move in moves)
                    {
                        Board nextBoard = board.Clone();
                        nextBoard.DoMove(move);
                        double score;
                        if (nextBoard.State != Board.BoardState.Active)
                            score = Evaluate(nextBoard);
                        else
                            score = nextBoard.NextToMove == "white" 
                                ? AlphaBetaMax(nextBoard, depth - 1, alpha, beta) 
                                : AlphaBetaMin(nextBoard, depth - 1, alpha, beta);
                        double actualScore = prob * score;
                        if (actualScore <= alpha)
                            return actualScore;//alpha cutoff;
                        if (actualScore < beta)
                            beta = actualScore;
                    }
                else
                {
                    Board nextBoard = board.Clone();
                    nextBoard.NextToMove = nextBoard.NextToMove == "white" ? "black" : "white";
                    double score = nextBoard.NextToMove == "white" 
                        ? AlphaBetaMax(nextBoard, depth - 1, alpha, beta) 
                        : AlphaBetaMin(nextBoard, depth - 1, alpha, beta);
                    double actualScore = prob * score;
                    if (actualScore <= alpha)
                        return actualScore;//alpha cutoff;
                    if (actualScore < beta)
                        beta = actualScore;
                }
            }
            return beta;
        }
        private double Evaluate(Board board)
        {
            double score = 0;
            for(int i = 0; i < 16; ++i)
            {
                score += board.WhitePieces[i] * _magicEvalValues[i];
                score -= board.BlackPieces[i] * _magicEvalValues[i];
            }
            return score;
        }
    }
}
