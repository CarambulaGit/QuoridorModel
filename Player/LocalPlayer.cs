using Project.Classes.Field;

namespace Project.Classes.Player {
    public class LocalPlayer : Player {
        public LocalPlayer(Pawn pawn = null, int numOfWalls = Consts.DEFAULT_NUM_OF_WALLS) : base(pawn, numOfWalls) { }

        public override object Clone() {
            return new LocalPlayer(Pawn, NumOfWalls);
        }
    }
}