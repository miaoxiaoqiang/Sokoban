using Sokoban.Model;
using Sokoban.Core.Parser;
using System.Collections.Generic;
using System;

namespace Sokoban.Core
{
    internal sealed class StaticObjManager
    {
        static StaticObjManager()
        {
            MapParser = new XSBMapParser();
            AnswersFormat = new Dictionary<Direction, ValueTuple<string, string>>()
            {
                { Direction.Left, ValueTuple.Create("l", "L") },
                { Direction.Right, ValueTuple.Create("r", "R") },
                { Direction.Up, ValueTuple.Create("u", "U") },
                { Direction.Down, ValueTuple.Create("d", "D") }
            };
        }

        public static XSBMapParser MapParser
        {
            get;
        }

        public static IReadOnlyDictionary<Direction, ValueTuple<string, string>> AnswersFormat
        {
            get;
        }
    }
}
