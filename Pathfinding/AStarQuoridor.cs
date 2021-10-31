using System;
using System.Collections.Generic;
using Project.Classes.Field;

namespace Project.Classes.Pathfinding {
    public static class AStarQuoridor {
        private static List<Point> _path = new List<Point>();
        private static List<Point> _finishes = new List<Point>();

        public static List<Point> FindPath(FieldSpace[,] field, Point start, Predicate<Point> winningCondition,
            Func<Point, Point, float> heuristicLength) {
            _path.Clear();
            _finishes = CalculateFinishes(field.GetLength(0), field.GetLength(1), winningCondition);
            foreach (var finish in _finishes) {
                var path = AStar<FieldSpace>.FindPath(field, start, finish, heuristicLength);
                if (path.Count == 1) {
                    return path;
                }
                
                if (path.Count < _path.Count || _path.Count == 0) {
                    _path = path;
                }
            }

            return _path;
        }

        public static bool IsTherePaths(FieldSpace[,] field, List<Pawn> pawns,
            Func<Point, Point, float> heuristicLength) {
            bool flag;
            foreach (var pawn in pawns) {
                flag = false;
                _finishes = CalculateFinishes(field.GetLength(0), field.GetLength(1), pawn.WinCondition);
                foreach (var finish in _finishes) {
                    var path = AStar<FieldSpace>.FindPath(field, pawn.Pos, finish, heuristicLength);
                    if (path == null) continue;
                    flag = true;
                    break;
                }

                if (!flag) {
                    return false;
                }
            }

            return true;
        }

        private static List<Point> CalculateFinishes(int fieldYLen, int fieldXLen, Predicate<Point> winningCondition) {
            _finishes.Clear();
            for (var y = 0; y < fieldYLen; y++) {
                for (var x = 0; x < fieldXLen; x++) {
                    var curPoint = new Point(y, x);
                    if (winningCondition(curPoint)) {
                        _finishes.Add(curPoint);
                    }
                }
            }

            return _finishes;
        }
    }
}