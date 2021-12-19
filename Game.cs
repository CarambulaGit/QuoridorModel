using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Project.Classes.Field;
using Project.Classes.Player;

namespace Project.Classes {
    public class Game {
        private static object locker = new object();
        private bool _gameRunning;
        private Task _waitTask;
        private IEnumerator<Player.Player> _playersEnumerator;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        public Field.Field Field { get; private set; }
        public List<Player.Player> Players { get; private set; }
        public Player.Player CurrentPlayer { get; private set; }

        public bool GameRunning {
            get => _gameRunning;
            private set {
                if (_gameRunning == value) {
                    return;
                }

                _gameRunning = value;
                if (_gameRunning) {
                    GameStarted?.Invoke();
                }
                else {
                    GameFinished?.Invoke();
                }
            }
        }

        public event Action GameStarted;
        public event Action GameFinished;
        public event Action<Player.Player> GameFinishedWithWinner;
        public event Action OnNextTurn;
        public event Action OnNextPlayer;
        public event Action PlayersOrderChanged;

        private Game(int ySize, int xSize, List<Player.Player> players) {
            if (players.Count < 2) {
                throw new ArgumentException("There are must be at least 2 players");
            }

            if (players.Count > 4) {
                throw new ArgumentException("There are must be no more than 4 players");
            }

            if (players.Count != 2) {
                throw new ArgumentException("Not 2 players variation not implemented yet");
            }

            if (xSize.IsEven() || ySize.IsEven()) {
                throw new ArgumentException("Field sizes must be odd");
            }

            Players = players;
            _playersEnumerator = Players.GetEnumerator();
            // todo add reset methods to event
            GameStarted += () => { };
            Field = new Field.Field(xSize, ySize);
            var yLen = Field.FieldSpaces.GetLength(0);
            var xLen = Field.FieldSpaces.GetLength(1);
            var positions = new Point[4] {
                new Point(yLen - 1, xLen / 2),
                new Point(0, xLen / 2),
                new Point(yLen / 2, 0),
                new Point(yLen / 2, xLen - 1)
            };
            var winConditions = new Predicate<Point>[] {
                p => p.Y == 0,
                p => p.Y == yLen - 1,
                p => p.X == xLen - 1,
                p => p.X == 0,
            };

            for (var i = 0; i < Players.Count; i++) {
                Players[i].Pawn = new Pawn(positions[i], Players[i], Field, winConditions[i]);
                Field.TryAddPawn(Players[i].Pawn);
            }
        }

        public static Game CreatePlayerVsPlayer(int firstPlayedId, int secondPlayerId) {
            var players = new List<Player.Player> {new LocalPlayer(firstPlayedId), new LocalPlayer(secondPlayerId)};
            return new Game(Consts.DEFAULT_FIELD_SIZE_Y, Consts.DEFAULT_FIELD_SIZE_X, players);
        }

        public static Game CreatePlayerVsBot(int firstPlayedId, int secondPlayerId, bool playerMoveFirst = true) {
            var players = playerMoveFirst
                ? new List<Player.Player> {new LocalPlayer(firstPlayedId), new SuperDuperUltraGiperBot(secondPlayerId)}
                : new List<Player.Player> {new SuperDuperUltraGiperBot(firstPlayedId), new LocalPlayer(secondPlayerId)};
            return new Game(Consts.DEFAULT_FIELD_SIZE_Y, Consts.DEFAULT_FIELD_SIZE_X, players);
        }

        public Player.Player FindPlayerWithNetworkId(int networkId) {
            return Players.Find(player => player.NetworkId == networkId);
        }

        public void Tick() {
            lock (locker) {
                if (!GameRunning) return;
            }

            if (_waitTask.IsCompleted && !IsThereWinner(out var winner)) {
                if (_waitTask.IsFaulted) {
                    throw _waitTask.Exception.InnerExceptions[0];
                }

                OnNextTurn?.Invoke();
                _waitTask = WaitForMove(_tokenSource.Token);
            }
        }

        private bool IsThereWinner(out Player.Player winner) {
            winner = null;
            foreach (var player in Players) {
                if (player.Pawn.IsWinner) {
                    winner = player;
                    FinishGame(winner);
                    return true;
                }
            }

            return false;
        }

        // public static Stopwatch stopWatch = new Stopwatch();

        private async Task WaitForMove(CancellationToken ct) {
            // stopWatch.Restart();
            CurrentPlayer.myTurn = true;
            OnNextPlayer?.Invoke();
            // await Task.Run(() => CurrentPlayer.MakeMove(ct), ct);
            var task = Task.Run(() => CurrentPlayer.MakeMove(ct), ct);
            while (!task.IsCompleted) {
                if (ct.IsCancellationRequested) {
                    return;
                }

                await Task.Yield();
            }

            CurrentPlayer.myTurn = false;
            CurrentPlayer = _playersEnumerator.GetNextCycled();
            // stopWatch.Stop();
            // Debug.LogAssertion(stopWatch.ElapsedMilliseconds);

            if (task.IsFaulted) {
                throw task.Exception.InnerExceptions[0];
            }
        }

        public void StartGame() {
            lock (locker) {

                if (GameRunning) {
                    throw new Exception("Game already going");
                }

                GameRunning = true;
                _playersEnumerator.Reset();
                CurrentPlayer = _playersEnumerator.GetNextCycled();
                OnNextTurn?.Invoke();
                _waitTask = WaitForMove(_tokenSource.Token);
            }
        }

        private void FinishGame(Player.Player winner) {
            CancelGame();
            GameFinishedWithWinner?.Invoke(winner);
        }

        public void CancelGame() {
            if (!GameRunning) {
                throw new Exception("Game isn't going");
            }

            GameRunning = false;
            _tokenSource.Cancel();
            _waitTask = null;
        }

        public void Restart() {
            if (GameRunning) {
                CancelGame();
            }

            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();

            Field.Reset();
            Players.ForEach(player => player.Reset());
            var yLen = Field.FieldSpaces.GetLength(0);
            var xLen = Field.FieldSpaces.GetLength(1);
            var positions = new Point[4] {
                new Point(0, xLen / 2),
                new Point(yLen - 1, xLen / 2),
                new Point(yLen / 2, 0),
                new Point(yLen / 2, xLen - 1)
            };

            var pawns = Players.Select(player => player.Pawn).ToList();
            for (var i = 0; i < pawns.Count; i++) {
                pawns[i].Reset(positions[i]);
            }

            StartGame();
        }

        public void Restart(List<Player.Player> players) {
            if (!Players.HasSameContent(players)) {
                return;
            }

            if (GameRunning) {
                CancelGame();
            }

            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();

            Field.Reset();
            Players = players;
            _playersEnumerator = Players.GetEnumerator();
            Players.ForEach(player => player.Reset());
            var yLen = Field.FieldSpaces.GetLength(0);
            var xLen = Field.FieldSpaces.GetLength(1);
            var positions = new Point[4] {
                new Point(0, xLen / 2),
                new Point(yLen - 1, xLen / 2),
                new Point(yLen / 2, 0),
                new Point(yLen / 2, xLen - 1)
            };

            var winConditions = new Predicate<Point>[] {
                p => p.Y == yLen - 1,
                p => p.Y == 0,
                p => p.X == xLen - 1,
                p => p.X == 0,
            };

            var pawns = Players.Select(player => player.Pawn).ToList();
            for (var i = 0; i < pawns.Count; i++) {
                pawns[i].Reset(positions[i], winConditions[i]);
            }

            StartGame();
            PlayersOrderChanged?.Invoke();
        }
    }
}