namespace Project.Classes.Field {
    public class Wall {
        // Wall takes 3 FieldSpaces
        // X and Y coordinates of central FieldSpace
        public enum Type {
            Horizontal,
            Vertical
        }

        public Point Pos { get; }

        public int Y => Pos.Y;
        public int X => Pos.X;
        public Type WallType { get; }

        public Wall(int y, int x, Type type) {
            Pos = new Point(y, x);
            WallType = type;
        }
    }
}