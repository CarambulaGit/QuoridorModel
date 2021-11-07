using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Classes.Field;
using Project.Classes.Pathfinding;
using UnityEngine;

namespace Project.Classes.Player {
    public class SuperDuperUltraGiperBot : Bot {
        private const int DEFAULT_DEPTH = 1;
        private const float TURN_WEIGHT = 1f;
        private const float NUM_OF_WALLS_WEIGHT = 1f;
        private const float PATH_COUNT_WEIGHT = 5f;
        private int _depth;
        private bool _isFirst;
        private List<Wall> _walls = new List<Wall>();
        private Action _move; // todo

        public SuperDuperUltraGiperBot(Pawn pawn = null, int numOfWalls = Consts.DEFAULT_NUM_OF_WALLS,
            int depth = DEFAULT_DEPTH) : base(pawn, numOfWalls) {
            _depth = depth;
        }

        protected override Action GetNextMove() {
            _isFirst = Pawn.Field.Pawns.IndexOf(Pawn) == 0;
            return CalculateBestMove(_depth <= 0 ? DEFAULT_DEPTH : _depth);
        }

        private Action CalculateBestMove(int depth) {
            Minimax(Pawn.Field, depth, float.MinValue, float.MaxValue, true);
            return _move;
        }

        private float Minimax(Field.Field field, int depth, float alpha, float beta, bool maximizingPlayer) {
            if (depth == 0 || field.Pawns.Any(pawn => pawn.IsWinner)) {
                return CalculatePosRating(field, maximizingPlayer);
            }

            if (maximizingPlayer) {
                var maxEval = float.MinValue;
                foreach (var position in GetChildGamePositions(field, _isFirst ? 0 : 1)) {
                    var eval = Minimax(position.TElem, depth - 1, alpha, beta, false);
                    if (eval >= maxEval) {
                        maxEval = eval;
                        _move = position.GElem;
                    }

                    alpha = Utils.Max(alpha, eval);
                    if (beta <= alpha) {
                        break;
                    }
                }

                return maxEval;
            }
            else {
                var minEval = float.MaxValue;
                foreach (var position in GetChildGamePositions(field, _isFirst ? 1 : 0)) {
                    var eval = Minimax(position.TElem, depth - 1, alpha, beta, true);
                    if (eval < minEval) {
                        minEval = eval;
                    }

                    beta = Utils.Min(beta, eval);
                    if (beta <= alpha) {
                        break;
                    }
                }

                return minEval;
            }
        }

        private float CalculatePosRating(Field.Field field, bool myMove) {
            var result = 0f;
            GetPositionVariables(field, out var myIndex, out var otherIndex, out var myPawn, out var otherPawn,
                out var myShortestPath, out var otherShortestPath);
            var myShortestPathCount = myShortestPath.Count;
            var otherShortestPathCount = otherShortestPath.Count;
            var players = field.Pawns.Select(pawn => pawn.Owner).ToList();
            var myWallsCount = players[myIndex].NumOfWalls;
            var otherWallsCount = players[otherIndex].NumOfWalls;

            if (myShortestPathCount == 0) {
                return float.MaxValue;
            }
            else if (!myMove && otherShortestPathCount == 1) {
                return float.MinValue;
            }

            if (myWallsCount == 0 && otherShortestPathCount < myShortestPathCount) {
                return float.MinValue;
            }

            result += myMove ? 1 : -1 * TURN_WEIGHT;
            result += (myWallsCount - otherWallsCount) * NUM_OF_WALLS_WEIGHT;
            result += (otherShortestPathCount - myShortestPathCount) * PATH_COUNT_WEIGHT;
            return result;
        }

        private PairsList<Field.Field, Action> GetChildGamePositions(Field.Field field, int index) {
            var result = new PairsList<Field.Field, Action>();
            var currentPlayerPawn = field.Pawns[index];
            GetPositionsMovingPawn(field, index, currentPlayerPawn, result);
            GetPlacingWall(field, index, result);
            return result;
        }

        private void GetPositionsMovingPawn(Field.Field field, int index, Pawn currentPlayerPawn,
            PairsList<Field.Field, Action> result) {
            foreach (var possiblePos in currentPlayerPawn.GetPossibleDirections()) {
                var newField = (Field.Field) field.Clone();
                if (newField.Pawns[index].TryMove(possiblePos)) {
                    void Move() => TryMovePawn(possiblePos);
                    result.AddPair(newField, Move);
                }
                else {
                    throw new Exception($"Can't move to {possiblePos}");
                }
            }
        }

        private void GetPlacingWall(Field.Field field, int index, PairsList<Field.Field, Action> result) {
            if (!field.Pawns[index].Owner.CanSetWall) {
                return;
            }

            GetPositionVariables(field, out var myIndex, out var otherIndex, out var myPawn, out var otherPawn,
                out var myShortestPath, out var otherShortestPath);
            var myTurn = index == myIndex;
            int delta;
            List<Point> enemyPath;
            Point startPos;
            if (myTurn) {
                delta = otherShortestPath.Count - myShortestPath.Count;
                enemyPath = otherShortestPath;
                startPos = otherPawn.Pos;
            }
            else {
                delta = myShortestPath.Count - otherShortestPath.Count;
                enemyPath = myShortestPath;
                startPos = myPawn.Pos;
            }


            foreach (var possiblePos in GetWallPositions(enemyPath, startPos)) {
                var newField = (Field.Field) field.Clone();
                if (!newField.TrySetWall(possiblePos)) continue;
                newField.Pawns[index].Owner.DecrementNumOfWalls();

                var myNewShortestPathCount = AStarQuoridor.FindPath(newField.FieldSpaces, newField.Pawns[index].Pos,
                    newField.Pawns[index].WinCondition,
                    Point.ManhattanLengthFloat).Count;
                var otherNewShortestPathCount = AStarQuoridor.FindPath(newField.FieldSpaces,
                    newField.Pawns[otherIndex].Pos,
                    newField.Pawns[otherIndex].WinCondition,
                    Point.ManhattanLengthFloat).Count;
                var newDelta = myTurn
                    ? otherNewShortestPathCount - myNewShortestPathCount
                    : myNewShortestPathCount - otherNewShortestPathCount;
                if (delta >= newDelta) {
                    continue;
                }

                void Move() => TrySetWall(possiblePos);
                result.AddPair(newField, Move);
            }
        }

        private List<Wall> GetWallPositions(List<Point> enemyPath, Point startPoint) {
            _walls.Clear();
            if (enemyPath.Count == 0) return _walls;
            var curPoint = startPoint;
            var enemyPathCount = enemyPath.Count;
            for (var i = 0; i < enemyPathCount; i++) {
                var moveVector = (enemyPath[i] - curPoint) / 2;
                Wall w1;
                Wall w2;
                switch (moveVector) {
                    case (0, 1):
                    case (0, -1):
                        w1 = new Wall(curPoint.Y + moveVector.Y + 1, curPoint.X + moveVector.X, Wall.Type.Vertical);
                        w2 = new Wall(curPoint.Y + moveVector.Y - 1, curPoint.X + moveVector.X, Wall.Type.Vertical);
                        break;
                    case (1, 0):
                    case (-1, 0):
                        w1 = new Wall(curPoint.Y + moveVector.Y, curPoint.X + moveVector.X + 1, Wall.Type.Horizontal);
                        w2 = new Wall(curPoint.Y + moveVector.Y, curPoint.X + moveVector.X - 1, Wall.Type.Horizontal);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _walls.Add(w1);
                _walls.Add(w2);

                curPoint = enemyPath[i];
            }

            return _walls;
        }

        private void GetPositionVariables(Field.Field field, out int myIndex, out int otherIndex, out Pawn myPawn,
            out Pawn otherPawn, out List<Point> myShortestPath,
            out List<Point> otherShortestPath) {
            myIndex = _isFirst ? 0 : 1;
            otherIndex = 1 - myIndex;
            myPawn = field.Pawns[myIndex];
            otherPawn = field.Pawns[otherIndex];
            myShortestPath = AStarQuoridor.FindPath(field.FieldSpaces, myPawn.Pos, myPawn.WinCondition,
                Point.ManhattanLengthFloat);
            otherShortestPath = AStarQuoridor.FindPath(field.FieldSpaces, otherPawn.Pos, otherPawn.WinCondition,
                Point.ManhattanLengthFloat);
        }

        private Action MovePawnToFinish(Point pos, Field.Field field) {
            var path = AStarQuoridor.FindPath(field.FieldSpaces, pos, Pawn.WinCondition,
                Point.ManhattanLengthFloat);
            if (path.Count > 0) {
                return () => { Pawn.TryMove(path[0]); };
            }

            throw new Exception("Already at finish");
        }

        public override object Clone() {
            return new SuperDuperUltraGiperBot(Pawn, NumOfWalls, _depth);
        }
    }
}