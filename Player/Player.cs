using System;
using System.Threading;
using System.Threading.Tasks;
using Project.Classes.Field;

namespace Project.Classes.Player {
    public abstract class Player : ICloneable {
        public enum MoveType {
            PlacingWall,
            Moving
        }

        public int NetworkId { get; private set; }

        public bool myTurn;
        protected bool _moveDone;

        protected int _numOfWalls;
        protected int _maxWalls;

        // private IPlayerController _playerController;
        public Pawn Pawn { get; set; }

        public int NumOfWalls {
            get => _numOfWalls;
            private set {
                if (_numOfWalls == value) {
                    return;
                }

                _numOfWalls = value;
                NumOfWallsChanged?.Invoke();
            }
        }

        public void DecrementNumOfWalls() => NumOfWalls--;

        public bool HasWalls => NumOfWalls > 0;

        public event Action NumOfWallsChanged;
        public Action<Wall> OnWallPlaced;
        public Action<Point, Point> OnPawnMoved;

        public Player(int networkId, Pawn pawn = null, int numOfWalls = Consts.DEFAULT_NUM_OF_WALLS) {
            NetworkId = networkId;
            Pawn = pawn;
            _maxWalls = numOfWalls;
            NumOfWalls = numOfWalls;
        }

        public virtual async Task MakeMove(CancellationToken ct) {
            // await Task.Run(() => _playerController.GetNextMove()?.Invoke());
            while (!_moveDone) {
                if (ct.IsCancellationRequested) {
                    return;
                }

                await Task.Yield();
            }

            _moveDone = false;
        }

        public virtual bool TrySetWall(Wall wall) {
            if (!CanSetWall(wall)) return false;

            Pawn.Field.UnsafeSetWall(wall);

            DecrementNumOfWalls();
            _moveDone = true;
            OnWallPlaced?.Invoke(wall);
            return true;
        }

        public bool CanSetWall(Wall wall) {
            if (Pawn == null || !myTurn || _moveDone || !HasWalls || !Pawn.Field.CanSetWall(wall)) {
                return false;
            }

            return true;
        }

        public virtual bool TryMovePawn(Point newPos) {
            var oldPos = Pawn.Pos;
            if (!CanMovePawn(newPos)) return false;

            Pawn.UnsafeMove(newPos);

            _moveDone = true;
            OnPawnMoved?.Invoke(oldPos, newPos);
            return true;
        }

        public bool CanMovePawn(Point newPos) {
            if (Pawn == null || !myTurn || _moveDone || !Pawn.CanMove(newPos)) {
                return false;
            }

            return true;
        }

        public void Reset() {
            NumOfWalls = _maxWalls;
            myTurn = false;
            _moveDone = false;
        }

        public abstract object Clone();
    }
}