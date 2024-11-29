using Sokoban.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Sokoban
{
    public sealed class ItemBrushFactory
    {
        private static readonly IDictionary<ItemType, Tuple<ImageBrush, string>> items;

        static ItemBrushFactory()
        {
            items = new Dictionary<ItemType, Tuple<ImageBrush, string>>
            {
                { ItemType.Floor, Tuple.Create<ImageBrush, string>((ImageBrush)Application.Current.Resources["Floor"], "-") },
                { ItemType.Goal, Tuple.Create<ImageBrush, string>((ImageBrush)Application.Current.Resources["GoalSmall"], ".") },
                { ItemType.Man, Tuple.Create<ImageBrush, string>((ImageBrush)Application.Current.Resources["Man"], "@") },
                { ItemType.ManGoal, Tuple.Create<ImageBrush, string>((ImageBrush)Application.Current.Resources["ManGoal"], "+") },
                { ItemType.Box, Tuple.Create<ImageBrush, string>((ImageBrush)Application.Current.Resources["Box"], "$") },
                { ItemType.BoxGoal, Tuple.Create<ImageBrush, string>((ImageBrush)Application.Current.Resources["BoxGoal"], "*") },
                { ItemType.Wall, Tuple.Create<ImageBrush, string>((ImageBrush)Application.Current.Resources["Wall"], "#") },
                { ItemType.Empty, Tuple.Create<ImageBrush, string>((ImageBrush)Application.Current.Resources["Grass"], "_") }
            };
        }

        public static ImageBrush GetItemBrush(ItemType item)
        {
            return items[item].Item1;
        }

        public static string GetItemDesc(ItemType item)
        {
            return items[item].Item2;
        }
    }
}
