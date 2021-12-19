using System;
using Project.Classes.Field;

namespace Project.Classes.Player {
    public class RandomBot : Bot {
        private static Random _random = new Random();

        public RandomBot(int networkId, Pawn pawn = null, int numOfWalls = Consts.DEFAULT_NUM_OF_WALLS) : base(networkId, pawn, numOfWalls){}

        protected override Action GetNextMove() {
            if (!HasWalls) return () => TryMovePawn(Pawn.GetPossibleDirections().GetRandom());
            var possibleWalls = Pawn.Field.GetPossibleWallPositions();
            var possiblePawnMoves = Pawn.GetPossibleDirections();
            var possibleWallsCount = possibleWalls.Count;
            var possiblePawnMovesCount = possiblePawnMoves.Count;
            var index = _random.Next(possibleWallsCount + possiblePawnMovesCount);
            return index >= possibleWallsCount
                ? (Action) (() => TryMovePawn(possiblePawnMoves[index - possibleWallsCount]))
                : () => TrySetWall(possibleWalls[index]);
        }
        
        public override object Clone() {
            return new RandomBot(NetworkId, Pawn, NumOfWalls);
        }
    }
}