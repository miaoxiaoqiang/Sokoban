using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Sokoban.Utils
{
    internal sealed class CursorFactory
    {
        private static readonly IDictionary<string, Cursor> cursorDict;

        static CursorFactory()
        {
            cursorDict = new Dictionary<string, Cursor>
            {
                { "Wall", CursorSafeHandle.CreateCursor(new BitmapImage(new Uri("pack://application:,,,/Sokoban;component/Resources/Images/WallHold.png")), 30, 30) },
                { "Goal", CursorSafeHandle.CreateCursor(new BitmapImage(new Uri("pack://application:,,,/Sokoban;component/Resources/Images/GoalHold.png")), 30, 30) },
                { "Man", CursorSafeHandle.CreateCursor(new BitmapImage(new Uri("pack://application:,,,/Sokoban;component/Resources/Images/ManHold.png")), 30, 30) },
                { "Box", CursorSafeHandle.CreateCursor(new BitmapImage(new Uri("pack://application:,,,/Sokoban;component/Resources/Images/BoxHold.png")), 30, 30) },
                { "Floor", CursorSafeHandle.CreateCursor(new BitmapImage(new Uri("pack://application:,,,/Sokoban;component/Resources/Images/FloorHold.png")), 30, 30) }
            };
        }


        public static Cursor GetCursor(string name)
        {
            return cursorDict[name];
        }
    }
}
