using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UrSparringPartner.Engines
{
    public delegate void MoveFoundDelegate(Board.Move move);

    public interface ISparringPartner
    {
        event MoveFoundDelegate MoveFound;

        Board Board { get; set; }

        void GoMove();

        Task GoMove(CancellationToken ct);
    }
}
