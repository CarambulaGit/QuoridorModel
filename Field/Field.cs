using System;
using System.Collections.Generic;
using System.Linq;
using Project.Classes.Pathfinding;
using BlockType = Project.Classes.Field.FieldSpace.BlockType;

namespace Project.Classes.Field {
    public struct Point {
        public int Y { get; }
        public int X { get; }

        public Point(int y, int x) {
            Y = y;
            X = x;
        }

        public static Point operator +(Point p1, Point p2) {
            return new Point(p1.Y + p2.Y, p1.X + p2.X);
        }

        public static Point operator -(Point p1, Point p2) {
            return new Point(p1.Y - p2.Y, p1.X - p2.X);
        }

        public static Point operator *(Point p, int num) {
            return new Point(p.Y * num, p.X * num);
        }

        public static Point operator /(Point p, int num) {
            return new Point(p.Y / num, p.X / num);
        }
        
        public static int ManhattanLength(Point p1, Point p2) {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        public static float ManhattanLengthFloat(Point p1, Point p2) {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        public override string ToString() {
            return $"y = {Y}, x = {X}";
        }

        public override bool Equals(object obj) {
            if (!(obj is Point point)) {
                return false;
            }

            return X == point.X && Y == point.Y;
        }

        public void Deconstruct(out int y, out int x) {
            y = Y;
            x = X;
        }
    }

    public class Field : ICloneable {
        public int XSize { get; }

        public int YSize { get; }

        public FieldSpace[,] FieldSpaces { get; }
        private List<Pawn> _pawns = new List<Pawn>();
        private List<Wall> _allWalls = new List<Wall>();
        public List<Pawn> Pawns => new List<Pawn>((Pawn[]) _pawns.ToArray().Clone());

        public event Action OnFieldSizeChanged;
        public event Action<Wall> OnWallPlaced;
        public event Action OnFieldCleared;

        private Point[] relativePointsHor = {new Point(0, -1), new Point(0, 1), new Point(-1, 0), new Point(1, 0)};
        private Point[] relativePointsVer = {new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1)};

        public Field(int xSize, int ySize, params Pawn[] pawns) {
            if (xSize <= 1 || ySize <= 1) {
                throw new Exception("Field sizes too small");
            }

            XSize = xSize;
            YSize = ySize;
            _pawns = pawns.ToList();

            var n = YSize * 2 - 1;
            var m = XSize * 2 - 1;

            FieldSpaces = new FieldSpace[n, m];
            for (var y = 0; y < n; y++) {
                for (var x = 0; x < m; x++) {
                    FieldSpaces[y, x] = y.IsEven() && x.IsEven()
                        ? new FieldSpace(BlockType.Platform)
                        : new FieldSpace();
                    if (y.IsOdd() && x.IsOdd()) {
                        _allWalls.Add(new Wall(y, x, Wall.Type.Horizontal));
                        _allWalls.Add(new Wall(y, x, Wall.Type.Vertical));
                    }
                }
            }

            OnFieldSizeChanged?.Invoke();
        }

        private Field(FieldSpace[,] fieldSpaces, params Pawn[] pawns) {
            var n = fieldSpaces.GetLength(0);
            var m = fieldSpaces.GetLength(1);
            YSize = (n + 1) / 2;
            XSize = (m + 1) / 2;
            FieldSpaces = fieldSpaces;
            Pawns.AddRange(pawns);


            for (var y = 0; y < n; y++) {
                for (var x = 0; x < m; x++) {
                    if (y.IsOdd() && x.IsOdd()) {
                        _allWalls.Add(new Wall(y, x, Wall.Type.Horizontal));
                        _allWalls.Add(new Wall(y, x, Wall.Type.Vertical));
                    }
                }
            }
        }

        public bool TryAddPawn(Pawn pawn) {
            // todo
            _pawns.Add(pawn);
            return true;
        }

        public void RemovePawn(Pawn pawn) {
            _pawns.Remove(pawn);
        }

        public List<Pawn> GetOtherPawns(Pawn pawn) {
            return Pawns.Where(p => p != pawn).ToList();
        }


        public bool CanSetWall(Wall wall) {
            return CheckWallPosition(wall) && IsThereWayToFinish(wall);
        }

        private bool CheckWallPosition(Wall wall) {
            if (wall.X < 1 ||
                wall.Y < 1 ||
                wall.X >= FieldSpaces.GetLength(1) - 1 ||
                wall.Y >= FieldSpaces.GetLength(0) - 1) {
                return false;
            }

            if (FieldSpaces[wall.Y, wall.X].Type != BlockType.Empty) {
                return false;
            }

            var relativePoints = wall.WallType switch {
                Wall.Type.Horizontal => relativePointsHor,
                Wall.Type.Vertical => relativePointsVer,
                _ => throw new ArgumentOutOfRangeException()
            };
            if (FieldSpaces[wall.Y + relativePoints[0].Y, wall.X + relativePoints[0].X].Type != BlockType.Empty ||
                FieldSpaces[wall.Y + relativePoints[1].Y, wall.X + relativePoints[1].X].Type != BlockType.Empty ||
                FieldSpaces[wall.Y + relativePoints[2].Y, wall.X + relativePoints[2].X].Type == BlockType.Platform ||
                FieldSpaces[wall.Y + relativePoints[3].Y, wall.X + relativePoints[3].X].Type == BlockType.Platform) {
                return false;
            }

            return true;
        }

        private bool IsThereWayToFinish(Wall wall) {
            var fieldWithNewWall = new Field(FieldSpaces.DeepCopy());
            fieldWithNewWall.SetWall(wall);
            return AStarQuoridor.IsTherePaths(fieldWithNewWall.FieldSpaces, Pawns, Point.ManhattanLengthFloat);
        }


        public bool TrySetWall(Wall wall) {
            if (!CanSetWall(wall)) return false;
            SetWall(wall);
            OnWallPlaced?.Invoke(wall);
            return true;
        }


        public void UnsafeSetWall(Wall wall) {
            SetWall(wall);
            OnWallPlaced?.Invoke(wall);
        }

        private void SetWall(Wall wall) {
            FieldSpaces[wall.Y, wall.X].Type = BlockType.Wall;
            switch (wall.WallType) {
                case Wall.Type.Horizontal:
                    FieldSpaces[wall.Y, wall.X - 1].Type = BlockType.Wall;
                    FieldSpaces[wall.Y, wall.X + 1].Type = BlockType.Wall;
                    break;
                case Wall.Type.Vertical:
                    FieldSpaces[wall.Y - 1, wall.X].Type = BlockType.Wall;
                    FieldSpaces[wall.Y + 1, wall.X].Type = BlockType.Wall;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public List<Wall> GetPossibleWallPositions() {
            return _allWalls.Where(CanSetWall).ToList();
        }

        public void Reset() {
            var n = YSize * 2 - 1;
            var m = XSize * 2 - 1;
            for (var y = 0; y < n; y++) {
                for (var x = 0; x < m; x++) {
                    if (FieldSpaces[y, x] == null) {
                        FieldSpaces[y, x] = y.IsEven() && x.IsEven()
                            ? new FieldSpace(BlockType.Platform)
                            : new FieldSpace();
                    }
                    else {
                        FieldSpaces[y, x].Type = y.IsEven() && x.IsEven()
                            ? BlockType.Platform
                            : BlockType.Empty;
                    }
                }
            }

            OnFieldCleared?.Invoke();
        }

        public override bool Equals(object obj) {
            if (!(obj is Field field)) {
                return false;
            }

            if (XSize != field.XSize || YSize != field.YSize) {
                return false;
            }

            var n = field.FieldSpaces.GetLength(0);
            var m = field.FieldSpaces.GetLength(0);
            if (FieldSpaces.GetLength(0) != n || FieldSpaces.GetLength(1) != m) {
                return false;
            }

            for (var i = 0; i < n; i++) {
                for (var j = 0; j < m; j++) {
                    if (!field.FieldSpaces[i, j].Equals(FieldSpaces[i, j])) {
                        return false;
                    }
                }
            }

            return true;
        }

        public object Clone() {
            var fieldSpaces = FieldSpaces.DeepCopy();
            var result = new Field(fieldSpaces);
            var players = Pawns.Select(pawn => pawn.Owner.Clone()).Cast<Player.Player>().ToList();
            var pawns = new Pawn[Pawns.Count];
            for (var i = 0; i < Pawns.Count; i++) {
                pawns[i] = Pawns[i].Copy(result, players[i]); // todo test this
                players[i].Pawn = pawns[i];
                result.TryAddPawn(pawns[i]);
            }

            // var pawns = (Pawn[]) Pawns.ToArray().Clone();
            return result;
        }
    }
}