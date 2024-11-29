namespace Sokoban.Model
{
    public enum ItemType : byte
    {
        /// <summary>
        /// 地板，符号("-")
        /// </summary>
        Floor = 0,
        /// <summary>
        /// 目标点，符号(".")
        /// </summary>
        Goal = 1,
        /// <summary>
        /// 玩家人物，符号("@")
        /// </summary>
        Man = 2,
        /// <summary>
        /// 人在目标点，符号("+")
        /// </summary>
        ManGoal = 3,
        /// <summary>
        /// 箱子，符号("$")
        /// </summary>
        Box = 4,
        /// <summary>
        /// 箱子在目标点，符号("*")
        /// </summary>
        BoxGoal = 5,
        /// <summary>
        /// 墙壁障碍，符号("#")
        /// </summary>
        Wall = 6,
        /// <summary>
        /// 表示空地，符号("_")
        /// </summary>
        Empty = 7
    }
}
