using System;

namespace Sokoban.Model
{
    [Serializable]
    public readonly struct MoveHistory
    {
        public MoveHistory(Coordinate from, Coordinate to, Direction direction)
        {
            From = from;
            To = to;
            Direction = direction;
        }

        public Coordinate From
        {
            get;
        }

        public Coordinate To
        {
            get;
        }

        public Direction Direction
        {
            get;
        }
    }
}
