using System;

namespace Project.Classes.Player {
    public class RandomBot : Bot {
        private static Random _random = new Random();

        protected override Action GetNextMove() {
            if (NumOfWalls <= 0) return () => TryMovePawn(Pawn.GetPossibleDirections().GetRandom());
            var possibleWalls = Pawn.Field.GetPossibleWallPositions();
            var possiblePawnMoves = Pawn.GetPossibleDirections();
            var possibleWallsCount = possibleWalls.Count;
            var possiblePawnMovesCount = possiblePawnMoves.Count;
            var index = _random.Next(possibleWallsCount + possiblePawnMovesCount);
            return index >= possibleWallsCount
                ? (Action) (() => TryMovePawn(possiblePawnMoves[index - possibleWallsCount]))
                : () => TrySetWall(possibleWalls[index]);

        }
    }
}