using System;
using System.Collections.Generic;
using System.Linq;
using BlockType = Project.Classes.Field.FieldSpace.BlockType;

namespace Project.Classes.Field {
    public class Pawn {
        public Point Pos {
            get => _pos;
            private set {
                if (_pos.Equals(value)) {
                    return;
                }

                _pos = value;
                PosChanged?.Invoke();
            }
        }

        public Player.Player Owner { get; }
        public Classes.Field.Field Field { get; }
        
        public Predicate<Point> WinCondition { get; private set; }
        public int Y => Pos.Y;
        public int X => Pos.X;

        public bool IsWinner => WinCondition(Pos);

        private List<Point> _possibleDirs = new List<Point>();
        private Point _pos;
        public event Action PosChanged;
        
        public Pawn(Point pos, Player.Player owner, Classes.Field.Field field, Predicate<Point> winCondition) {
            Pos = pos;
            Owner = owner;
            Field = field;
            WinCondition = winCondition;
        }

        public bool TryMove(Point newPos) {
            if (!CanMove(newPos)) return false;
            Pos = newPos;
            return true;
        }

        public void UnsafeMove(Point newPos) {
            Pos = newPos;
        }

        public bool CanMove(Point newPos) {
            return GetPossibleDirections().Contains(newPos);
        }

        public List<Point> GetPossibleDirections() {
            _possibleDirs.Clear();
            var other = GetOther;
            LookLeft(other);
            LookRight(other);
            LookUp(other);
            LookDown(other);
            return _possibleDirs;
        }


        private void LookLeft(List<Pawn> other) {
            if (X == 0 || Field.FieldSpaces[Y, X - 1].Type == BlockType.Wall) {
                return;
            }

            if ( /*Field.FieldSpaces[Y, X - 2].Type == BlockType.Platform && */!IsPosTaken(other, Y, X - 2)) {
                _possibleDirs.Add(new Point(Y, X - 2));
                return;
            }

            if (X >= 4 && Field.FieldSpaces[Y, X - 3].Type != BlockType.Wall && !IsPosTaken(other, Y, X - 4)) {
                _possibleDirs.Add(new Point(Y, X - 4));
                return;
            }

            if (Y >= 2 &&
                Field.FieldSpaces[Y - 1, X - 2].Type != BlockType.Wall && !IsPosTaken(other, Y - 2, X - 2)) {
                _possibleDirs.Add(new Point(Y - 2, X - 2));
            }

            if (Y <= Field.FieldSpaces.GetLength(0) - 3 &&
                Field.FieldSpaces[Y + 1, X - 2].Type != BlockType.Wall && !IsPosTaken(other, Y + 2, X - 2)) {
                _possibleDirs.Add(new Point(Y + 2, X - 2));
            }
        }


        private void LookRight(List<Pawn> other) {
            var xLen = Field.FieldSpaces.GetLength(1);
            if (X == xLen - 1 || Field.FieldSpaces[Y, X + 1].Type == BlockType.Wall) {
                return;
            }

            if ( /*Field.FieldSpaces[Y, X + 2].Type == BlockType.Platform && */!IsPosTaken(other, Y, X + 2)) {
                _possibleDirs.Add(new Point(Y, X + 2));
                return;
            }

            if (X <= xLen - 5 && Field.FieldSpaces[Y, X + 3].Type != BlockType.Wall && !IsPosTaken(other, Y, X + 4)) {
                _possibleDirs.Add(new Point(Y, X + 4));
                return;
            }

            if (Y >= 2 &&
                Field.FieldSpaces[Y - 1, X + 2].Type != BlockType.Wall && !IsPosTaken(other, Y - 2, X + 2)) {
                _possibleDirs.Add(new Point(Y - 2, X + 2));
            }

            if (Y <= Field.FieldSpaces.GetLength(0) - 3 &&
                Field.FieldSpaces[Y + 1, X + 2].Type != BlockType.Wall && !IsPosTaken(other, Y + 2, X + 2)) {
                _possibleDirs.Add(new Point(Y + 2, X + 2));
            }
        }

        private void LookUp(List<Pawn> other) {
            if (Y == 0 || Field.FieldSpaces[Y - 1, X].Type == BlockType.Wall) {
                return;
            }

            if ( /*Field.FieldSpaces[Y - 2, X].Type == BlockType.Platform && */!IsPosTaken(other, Y - 2, X)) {
                _possibleDirs.Add(new Point(Y - 2, X));
                return;
            }

            if (Y >= 4 && Field.FieldSpaces[Y - 3, X].Type != BlockType.Wall && !IsPosTaken(other, Y - 4, X)) {
                _possibleDirs.Add(new Point(Y - 4, X));
                return;
            }

            if (X >= 2 &&
                Field.FieldSpaces[Y - 2, X - 1].Type != BlockType.Wall && !IsPosTaken(other, Y - 2, X - 2)) {
                _possibleDirs.Add(new Point(Y - 2, X - 2));
            }

            if (X <= Field.FieldSpaces.GetLength(1) - 3 &&
                Field.FieldSpaces[Y - 2, X + 1].Type != BlockType.Wall && !IsPosTaken(other, Y - 2, X + 2)) {
                _possibleDirs.Add(new Point(Y - 2, X + 2));
            }
        }

        private void LookDown(List<Pawn> other) {
            var yLen = Field.FieldSpaces.GetLength(0);
            if (Y == yLen - 1 || Field.FieldSpaces[Y + 1, X].Type == BlockType.Wall) {
                return;
            }

            if ( /*Field.FieldSpaces[Y + 2, X].Type == BlockType.Platform && */!IsPosTaken(other, Y + 2, X)) {
                _possibleDirs.Add(new Point(Y + 2, X));
                return;
            }

            if (Y <= yLen - 5 && Field.FieldSpaces[Y + 3, X].Type != BlockType.Wall && !IsPosTaken(other, Y + 4, X)) {
                _possibleDirs.Add(new Point(Y + 4, X));
                return;
            }

            if (X >= 2 &&
                Field.FieldSpaces[Y + 2, X - 1].Type != BlockType.Wall && !IsPosTaken(other, Y + 2, X - 2)) {
                _possibleDirs.Add(new Point(Y + 2, X - 2));
            }

            if (X <= Field.FieldSpaces.GetLength(1) - 3 &&
                Field.FieldSpaces[Y + 2, X + 1].Type != BlockType.Wall && !IsPosTaken(other, Y + 2, X + 2)) {
                _possibleDirs.Add(new Point(Y + 2, X + 2));
            }
        }

        private List<Pawn> GetOther => Field.GetOtherPawns(this);

        private bool IsPosTaken(IEnumerable<Pawn> pawns, int y, int x) {
            return pawns.Any(pawn => pawn.Y == y && pawn.X == x);
        }

        public void Reset(Point pos) {
            Pos = pos;
        }

        public void Reset(Point pos, Predicate<Point> winCondition) {
            Pos = pos;
            WinCondition = winCondition;
        }

        public Pawn Copy(Field field, Player.Player owner) {
            return new Pawn(Pos, owner, field, WinCondition);
        }
    }
}