namespace Project.Classes {
    public static class Consts {
        #region GAME_CONSTANTS

        public const int DEFAULT_FIELD_SIZE_Y = 9;  
        public const int DEFAULT_FIELD_SIZE_X = 9;  
        public const int DEFAULT_NUM_OF_WALLS = 10;  
        
        public const string DEFAULT_WINNER_CONGRATULATION = "Winner:\n";  

        #endregion
        #region TAGS

        public const string GAME_MANAGER_TAG = "GameManager";  

        #endregion

        #region CONSOLE_COMMANDS

        public const string SET_WALL_COMMAND = "wall";
        public const string MOVE_COMMAND = "move";
        public const string JUMP_COMMAND = "jump";

        #endregion
    }
}