using System;
using Project.Classes.Field;

namespace Project.Classes.Player {
    public class StupidBot : Bot {
        public StupidBot(int networkId, Pawn pawn = null, int numOfWalls = Consts.DEFAULT_NUM_OF_WALLS) : base(networkId, pawn, numOfWalls) { }

        protected override Action GetNextMove() {
            return () => TryMovePawn(Pawn.GetPossibleDirections()[0]);
        }

        public override object Clone() {
            return new StupidBot(NetworkId, Pawn, NumOfWalls);
        }
    }
}