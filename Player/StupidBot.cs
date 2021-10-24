using System;

namespace Project.Classes.Player {
    public class StupidBot : Bot {
        protected override Action GetNextMove() {
            return () => TryMovePawn(Pawn.GetPossibleDirections()[0]);
        }
    }
}