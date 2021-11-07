using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Project.Classes.Field;

namespace Project.Classes.Pathfinding {
    public static class AStarQuoridor {
        private static List<Point> _finishes = new List<Point>();
        // public static Stopwatch stopWatch = new Stopwatch();


        public static List<Point> FindPath(FieldSpace[,] field, Point start, Predicate<Point> winningCondition,
            Func<Point, Point, float> heuristicLength) {
            var shortestPath = new List<Point>();
            if (winningCondition(start)) {
                return shortestPath;
            }

            _finishes = CalculateFinishes(field.GetLength(0), field.GetLength(1), winningCondition)
                .OrderBy(finish => Point.ManhattanLengthFloat(start, finish)).ToList();
            foreach (var finish in _finishes) {
                // stopWatch.Start();
                var path = AStar<FieldSpace>.FindPath(field, start, finish, heuristicLength);
                // stopWatch.Stop();
                // Debug.Log(stopWatch.ElapsedMilliseconds);
                if (path == null) {
                    continue;
                }

                if (path.Count == 1) {
                    shortestPath.Clear();
                    shortestPath.AddRange(path);
                    return shortestPath;
                }

                if (path.Count < shortestPath.Count || shortestPath.Count == 0) {
                    shortestPath.Clear();
                    shortestPath.AddRange(path);
                }
            }

            return shortestPath;
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
            for (var y = 0; y < fieldYLen; y += 2) {
                for (var x = 0; x < fieldXLen; x += 2) {
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