using System;
using System.Threading;
using System.Threading.Tasks;
using Project.Classes.Field;

namespace Project.Classes.Player {
    public abstract class Bot : Player {
        protected Bot(int networkId, Pawn pawn = null, int numOfWalls = Consts.DEFAULT_NUM_OF_WALLS) : base(networkId, pawn, numOfWalls) { }
        
        public sealed override async Task MakeMove(CancellationToken ct) {
            await Task.Delay(100, ct);
            GetNextMove()();
            await base.MakeMove(ct);
        }

        protected abstract Action GetNextMove();
    }
}