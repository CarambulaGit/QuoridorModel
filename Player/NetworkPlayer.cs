using System.Threading;
using System.Threading.Tasks;
using Project.Classes.Field;
using UnityEngine;

namespace Project.Classes.Player {
    public class NetworkPlayer : Player {
        public int NetworkId { get; set; }
        
        public NetworkPlayer(int networkId, Pawn pawn = null,
            int numOfWalls = Consts.DEFAULT_NUM_OF_WALLS) : base(pawn, numOfWalls) {
            NetworkId = networkId;
        }

        public override bool TrySetWall(Wall wall) {
            if (!CanSetWall(wall)) return false;
            ClientSend.SendMove(wall);
            return false;
        }

        public override bool TryMovePawn(Point newPos) {
            if (!CanMovePawn(newPos)) return false;
            ClientSend.SendMove(newPos);
            return false;
        }

        public void MovePawnByServer(Point newPos) {
            if (!CanMovePawn(newPos)) {
                Debug.LogError("Server send illegal for local game state move");
            }
            
            var oldPos = Pawn.Pos;
            Pawn.UnsafeMove(newPos);

            _moveDone = true;
            OnPawnMoved?.Invoke(oldPos, newPos);
        }

        public void SetWallByServer(Wall wall) {
            if (!CanSetWall(wall)) {
                Debug.LogError("Server send illegal for local game state move");
            }
            
            Pawn.Field.UnsafeSetWall(wall);

            _moveDone = true;
            OnWallPlaced?.Invoke(wall);
        }

        public override object Clone() {
            return new LocalPlayer(Pawn, NumOfWalls);
        }
    }
}