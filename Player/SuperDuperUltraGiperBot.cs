using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Classes.Field;
using Project.Classes.Pathfinding;

namespace Project.Classes.Player {
    public class SuperDuperUltraGiperBot : Bot {
        private const int DEFAULT_DEPTH = 1;
        private const float TURN_WEIGHT = 1f;
        private const float NUM_OF_WALLS_WEIGHT = 1f;
        private const float PATH_COUNT_WEIGHT = 5f;
        private int _depth;
        private bool _isFirst;
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
                    if (eval > maxEval) {
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
            var myIndex = _isFirst ? 0 : 1;
            var otherIndex = 1 - myIndex;
            var myPawn = field.Pawns[myIndex];
            var otherPawn = field.Pawns[otherIndex];
            var players = field.Pawns.Select(pawn => pawn.Owner).ToList();
            var myShortestPath = AStarQuoridor.FindPath(field.FieldSpaces, myPawn.Pos, myPawn.WinCondition,
                Point.ManhattanLengthFloat).Count;
            var otherShortestPath = AStarQuoridor.FindPath(field.FieldSpaces, otherPawn.Pos, otherPawn.WinCondition,
                Point.ManhattanLengthFloat).Count;
            var myWallsCount = players[myIndex].NumOfWalls;
            var otherWallsCount = players[otherIndex].NumOfWalls;
            
            if (myMove && myShortestPath == 1) {
                return float.MaxValue;
            } else if (!myMove && otherShortestPath == 1) {
                return float.MinValue;
            }

            if (myWallsCount == 0 && otherShortestPath < myShortestPath) {
                return float.MinValue;
            }
            else if (otherWallsCount == 0 && myShortestPath < otherShortestPath) {
                return float.MaxValue;
            }
            
            result += myMove ? 1 : -1 * TURN_WEIGHT;
            result += (myWallsCount - otherWallsCount) * NUM_OF_WALLS_WEIGHT;
            result += (otherShortestPath - myShortestPath) * PATH_COUNT_WEIGHT;
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

            foreach (var possiblePos in field.GetPossibleWallPositions()) {
                var newField = (Field.Field) field.Clone();
                if (newField.TrySetWall(possiblePos)) {
                    newField.Pawns[index].Owner.DecrementNumOfWalls();
                    void Move() => TrySetWall(possiblePos);
                    result.AddPair(newField, Move);
                }
                else {
                    throw new Exception($"Can't place wall to {possiblePos}");
                }
            }
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