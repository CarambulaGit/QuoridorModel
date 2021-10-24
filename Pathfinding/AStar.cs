using System;
using System.Collections.Generic;
using Project.Classes.Field;
using Project.Interfaces;

namespace Project.Classes.Pathfinding {
    public static class AStar<T> where T : ICanBeCrossed {
        public class Node {
            public Node Parent { get; set; }
            public Point Point { get; private set; }
            public float DistFromStart { get; set; }
            public float DistToFinish { get; private set; }

            public float F => DistFromStart + DistToFinish;

            public Node(Node parent, Point point, float distToFinish, Func<Point, Point, float> heuristicLength) {
                Parent = parent;
                Point = point;
                DistFromStart = Parent == null
                    ? 0
                    : Parent.DistFromStart + heuristicLength(Point, parent.Point);
                DistToFinish = distToFinish;
            }
        }

        private static List<Node> _visited = new List<Node>();
        private static List<Node> _needToVisit = new List<Node>();
        private static List<Point> _path = new List<Point>();
        private static List<Node> _neighbours = new List<Node>();
        private static Point[,] neighbourPoints = new Point[4, 2];

        public static List<Point> FindPath(T[,] arr, Point start, Point finish,
            Func<Point, Point, float> heuristicLength) {
            _path.Clear();
            _visited.Clear();
            _needToVisit.Clear();
            _needToVisit.Add(new Node(null, start, heuristicLength(start, finish), heuristicLength));
            while (_needToVisit.Count > 0) {
                // var currentNode = _needToVisit.OrderBy(node => node.F).First();
                _needToVisit.Sort((node1, node2) => node1.F.CompareTo(node2.F));
                var currentNode = _needToVisit[0];
                if (currentNode.Point.Equals(finish)) {
                    return GetPathForNode(currentNode);
                }

                _needToVisit.Remove(currentNode);
                _visited.Add(currentNode);
                foreach (var neighbour in GetNeighbours(arr, currentNode, heuristicLength, finish)) {
                    if (_visited.Exists(node => node.Point.Equals(neighbour.Point)))
                           continue;
                    var openNode = _needToVisit.Find(node =>
                        node.Point.Equals(neighbour.Point));
                    if (openNode == null) {
                        _needToVisit.Add(neighbour);
                    }
                    else if (openNode.F > neighbour.F) {
                        openNode.Parent = currentNode;
                        openNode.DistFromStart = neighbour.DistFromStart;
                    }
                }
            }

            return null;
        }

        private static List<Point> GetPathForNode(Node node) {
            _path.Clear();
            var curNode = node;
            while (curNode.Parent != null) {
                _path.Insert(0, curNode.Point);
                curNode = curNode.Parent;
            }

            return _path;
        }

        private static List<Node> GetNeighbours(T[,] field, Node node, Func<Point, Point, float> heuristicLength,
            Point finish) {
            _neighbours.Clear();
            neighbourPoints[0, 0] = new Point(node.Point.Y + 1, node.Point.X);
            neighbourPoints[0, 1] = new Point(1, 0);
            neighbourPoints[1, 0] = new Point(node.Point.Y - 1, node.Point.X);
            neighbourPoints[1, 1] = new Point(-1, 0);
            neighbourPoints[2, 0] = new Point(node.Point.Y, node.Point.X + 1);
            neighbourPoints[2, 1] = new Point(0, 1);
            neighbourPoints[3, 0] = new Point(node.Point.Y, node.Point.X - 1);
            neighbourPoints[3, 1] = new Point(0, -1);
            var len = neighbourPoints.GetLength(0);
            for (var i = 0; i < len; i++) {
                var point = neighbourPoints[i, 0];
                if (point.X < 0 || point.X >= field.GetLength(1))
                    continue;
                if (point.Y < 0 || point.Y >= field.GetLength(0))
                    continue;
                if (!field[point.Y, point.X].CanBeCrossed())
                    continue;
                var additionalPart = neighbourPoints[i, 1];
                var neighbourNode = new Node(node, point + additionalPart, heuristicLength(node.Point, finish),
                    heuristicLength);
                _neighbours.Add(neighbourNode);
            }

            return _neighbours;
        }
    }
}