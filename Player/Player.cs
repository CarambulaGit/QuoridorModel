using System;
using System.Threading;
using System.Threading.Tasks;
using Project.Classes.Field;

namespace Project.Classes.Player {
    public abstract class Player : ICloneable {
        public bool myTurn;
        private bool _moveDone;

        private int _numOfWalls;
        private int _maxWalls;

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

        public bool CanSetWall => NumOfWalls > 0;

        public event Action NumOfWallsChanged;
        public event Action<Wall> OnWallPlaced;
        public event Action<Point, Point> OnPawnMoved;

        public Player(Pawn pawn = null, int numOfWalls = Consts.DEFAULT_NUM_OF_WALLS) {
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

        public bool TrySetWall(Wall wall) {
            if (Pawn == null || !myTurn || _moveDone || !CanSetWall || !Pawn.Field.TrySetWall(wall)) {
                return false;
            }

            DecrementNumOfWalls();
            _moveDone = true;
            OnWallPlaced?.Invoke(wall);
            return true;
        }

        public bool TryMovePawn(Point newPos) {
            var oldPos = Pawn.Pos;
            if (Pawn == null || !myTurn || _moveDone || !Pawn.TryMove(newPos)) {
                return false;
            }

            _moveDone = true;
            OnPawnMoved?.Invoke(oldPos, newPos);
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