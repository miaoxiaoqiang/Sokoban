using System;

namespace Sokoban.Model
{
    /// <summary>
    /// 定义二维平面的坐标点
    /// </summary>
    [Serializable]
    public struct Coordinate : IEquatable<Coordinate>
    {
        public byte X
        {
            get;
            set;
        }

        public byte Y
        {
            get;
            set;
        }

        public ItemType Type 
        {
            get;
            set;
        }

        public readonly bool Equals(Coordinate other)
        {
            return X == other.X && Y == other.Y && Type == other.Type;
        }

        public override readonly string ToString()
        {
            return $"{X}:{Y}:{Core.Parser.XSBMapParser.GetItemChar(Type)}";
        }
    }
}
