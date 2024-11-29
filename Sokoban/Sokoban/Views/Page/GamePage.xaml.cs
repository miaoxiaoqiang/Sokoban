using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Sokoban.Core;
using Sokoban.Model;
using Sokoban.Core.Parser;

namespace Sokoban
{
    /// <summary>
    /// GamePage.xaml 的交互逻辑
    /// </summary>
    public sealed partial class GamePage : Page, IInputKey, ILayoutGame
    {
        private readonly IReadOnlyDictionary<double, RotateTransform> Rotations;
        private readonly StringBuilder stepBuilder;
        private readonly SoundPlayer musicplayer;
        private readonly IReadOnlyDictionary<string, System.IO.Stream> _musicDict;
        private readonly IList<Tuple<Coordinate, Rectangle>> moveableItems;

        private Direction direction = Direction.Down;
        private Coordinate manCoor;
        private Stage _stage;

        public GamePage()
        {
            moveableItems = new List<Tuple<Coordinate, Rectangle>>();
            _musicDict = new Dictionary<string, System.IO.Stream>()
            {
                { "Clear", new System.IO.MemoryStream(Properties.Resources.StageClear) },
                { "Man", new System.IO.MemoryStream(Properties.Resources.ManMove) },
                { "Box", new System.IO.MemoryStream(Properties.Resources.BoxMove) }
            };
            musicplayer = new SoundPlayer();
            stepBuilder = new StringBuilder();
            Rotations = new Dictionary<double, RotateTransform>()
            {
                { 0, new RotateTransform(0, 15, 15) },
                { 180, new RotateTransform(180, 15, 15) },
                { 90, new RotateTransform(90, 15, 15) },
                { -90, new RotateTransform(-90, 15, 15) }
            };

            InitializeComponent();

            BtnBackStage.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(BackStage), true);
            BtnRegret.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Regret), true);
            BtnResetStage.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ResetCurrentStage), true);
            PART_Next.AddHandler(TextBlock.MouseLeftButtonDownEvent, new MouseButtonEventHandler(GoNext), true);
            PART_Cancel.AddHandler(TextBlock.MouseLeftButtonDownEvent, new MouseButtonEventHandler(CancelGo), true);

            musicplayer.Stream = _musicDict["Box"];
            musicplayer.Play();
        }

        private void BackStage(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is IWindow window)
            {
                window.Navigate("Stage");
            }
        }

        private void Regret(object sender, MouseButtonEventArgs e)
        {
            if (_stage.HistoryStack.Count > 0)
            {
                if (_stage.Cleared)
                {
                    _stage.Cleared = false;
                }

                MoveHistory history = _stage.HistoryStack.Pop();
                Tuple<Coordinate, Rectangle> manTuple = moveableItems.First();
                direction = history.Direction;

                if (history.To.Type == ItemType.Man || history.To.Type == ItemType.ManGoal)
                {
                    ChangeManCoorStatus(history.From.X, history.From.Y, ref manCoor);
                    MoveMan(history.To.X, history.To.Y, false, true, ref manCoor);
                }
                else
                {
                    byte backx = history.From.X;
                    byte backy = history.From.Y;
                    if (direction == Direction.Up || direction == Direction.Down)
                    {
                        backy = Convert.ToByte(backy + ((int)Direction.Up + Math.Abs((int)direction - 26)) - 25);
                    }
                    else
                    {
                        backx = Convert.ToByte(backx + ((int)Direction.Left + Math.Abs((int)direction - 25)) - 24);
                    }

                    ChangeManCoorStatus(backx, backy, ref manCoor);
                    MoveMan(history.From.X, history.From.Y, false, true, ref manCoor);

                    Tuple<Coordinate, Rectangle> result = moveableItems.FirstOrDefault(p => (p.Item1.Type == ItemType.Box || p.Item1.Type == ItemType.BoxGoal)
                                                                                        && Canvas.GetLeft(p.Item2) / 30 == history.To.X && Canvas.GetTop(p.Item2) / 30 == history.To.Y);
                    MoveBox(history.From.X, history.From.Y, history.To.X, history.To.Y, false, true, result.Item2);
                }

                stepBuilder.Remove(stepBuilder.Length - 1, 1);
                _stage.LURD = stepBuilder.ToString();

                if (_stage.HistoryStack.Count == 0)
                {
                    _stage.Reset();
                }
            }
        }

        private void ResetCurrentStage(object sender, MouseButtonEventArgs e)
        {
            while (_stage.HistoryStack.Count > 0)
            {
                Regret(sender, e);
            }
        }

        private void GoNext(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is IWindow window)
            {
                if (window.GetPage("Stage") is IStage stage)
                {
                    Stage nextstage = stage.GetNextStage(_stage);
                    if (nextstage != null)
                    {
                        InitLayout(nextstage);
                    }
                    else
                    {
                        BackStage(null, null);
                    }
                }
            }
        }

        private void CancelGo(object sender, MouseButtonEventArgs e)
        {
            PART_ClearI.Visibility = Visibility.Collapsed;
            PART_ClearII.Visibility = Visibility.Collapsed;
        }

        public void InitLayout(Stage stage)
        {
            PART_ClearI.Visibility = Visibility.Collapsed;
            PART_ClearII.Visibility = Visibility.Collapsed;

            moveableItems.Clear();
            stepBuilder.Clear();

            DataContext = stage;
            _stage = stage;

            if (gameCanvas.Children.Count > 0)
            {
                gameCanvas.Children.Clear();
            }
            gameCanvas.ClearValue(WidthProperty);
            gameCanvas.ClearValue(HeightProperty);
            gameCanvas.Width = 30 * stage.Cols;
            gameCanvas.Height = 30 * stage.Rows;

            if (!string.IsNullOrEmpty(stage.LURD))
            {
                stepBuilder.Append(stage.LURD);
            }

            for (byte y = 0; y < stage.Map.GetLength(0); y++)
            {
                for (byte x = 0; x < stage.Map.GetLength(1); x++)
                {
                    if (stage.Map[y, x] != (byte)ItemType.Empty)
                    {
                        ItemType item = (ItemType)stage.Map[y, x];

                        if (item == ItemType.Wall || item == ItemType.Floor)
                        {
                            Rectangle rectangle3 = new()
                            {
                                Width = 30,
                                Height = 30,
                                Fill = ItemBrushFactory.GetItemBrush(item)
                            };
                            Canvas.SetZIndex(rectangle3, 71 + stage.Map[y, x]);
                            Canvas.SetLeft(rectangle3, x * 30);
                            Canvas.SetTop(rectangle3, y * 30);
                            gameCanvas.Children.Add(rectangle3);

                            continue;
                        }

                        Rectangle rectangle = new()
                        {
                            Width = 30,
                            Height = 30,
                            Fill = ItemBrushFactory.GetItemBrush(ItemType.Floor)
                        };
                        Canvas.SetZIndex(rectangle, 71);
                        Canvas.SetLeft(rectangle, x * 30);
                        Canvas.SetTop(rectangle, y * 30);
                        gameCanvas.Children.Add(rectangle);

                        if (item == ItemType.ManGoal || item == ItemType.BoxGoal)
                        {
                            Rectangle rectangleGoal = new()
                            {
                                Width = 30,
                                Height = 30,
                                Fill = ItemBrushFactory.GetItemBrush(ItemType.Goal)
                            };
                            Canvas.SetZIndex(rectangleGoal, 71 + stage.Map[y, x]);
                            Canvas.SetLeft(rectangleGoal, x * 30);
                            Canvas.SetTop(rectangleGoal, y * 30);
                            gameCanvas.Children.Add(rectangleGoal);
                        }

                        Rectangle rectangle1 = new()
                        {
                            Width = 30,
                            Height = 30,
                            Fill = ItemBrushFactory.GetItemBrush(item)
                        };
                        Canvas.SetZIndex(rectangle1, 71 + stage.Map[y, x]);
                        Canvas.SetLeft(rectangle1, x * 30);
                        Canvas.SetTop(rectangle1, y * 30);
                        gameCanvas.Children.Add(rectangle1);

                        if (item == ItemType.Man || item == ItemType.ManGoal)
                        {
                            manCoor = new Coordinate() { X = x, Y = y, Type = item };
                            if (moveableItems.Count > 0)
                            {
                                moveableItems.Insert(0, Tuple.Create(new Coordinate() { Type = item, X = x, Y = y }, rectangle1));
                            }
                            else
                            {
                                moveableItems.Add(Tuple.Create(new Coordinate() { Type = item, X = x, Y = y }, rectangle1));
                            }
                        }
                        else if (item == ItemType.Box || item == ItemType.BoxGoal)
                        {
                            moveableItems.Add(Tuple.Create(new Coordinate() { Type = item, X = x, Y = y }, rectangle1));
                        }
                    }
                }
            }
        }

        public void GetInputKey(object sender, KeyEventArgs e)
        {
            if (_stage != null && !_stage.Cleared)
            {
                Tuple<Coordinate, Rectangle> manTuple = moveableItems.First();
                int keyvalue = (int)e.Key;
                if (keyvalue <= 26 && keyvalue >= 23)
                {
                    if (direction == (Direction)keyvalue)
                    {
                        Move(direction, ref manCoor);
                    }
                    else
                    {
                        RotateManElement(manTuple.Item2, 90 * ((keyvalue & 1) == 1 ? 0 : 1) + 90 * (25 + ((keyvalue & 1) == 1 ? -1 : 0) - keyvalue));
                        direction = (Direction)keyvalue;
                    }
                }
            }
        }

        private void Move(Direction direction, ref Coordinate coordinate)
        {
            byte _x = coordinate.X;
            byte _y = coordinate.Y;
            byte x = coordinate.X;
            byte y = coordinate.Y;
            int direvalue = (int)direction;

            if (((int)direction & 1) == 0)
            {
                if ((y + direvalue - 25) < 0 || (y + direvalue - 25) > _stage.Map.GetLength(0))
                {
                    return;
                }

                y = Convert.ToByte(y + (direvalue - 25));
                _y = Convert.ToByte(_y + 2 * (direvalue - 25));
            }
            else
            {
                if ((x + direvalue - 24) < 0 || (x + direvalue - 24) > _stage.Map.GetLength(1))
                {
                    return;
                }

                x = Convert.ToByte(x + (direvalue - 24));
                _x = Convert.ToByte(_x + 2 * (direvalue - 24));
            }

            if (_stage.Map[y, x] == (byte)ItemType.Wall || _stage.Map[y, x] == (byte)ItemType.Empty)
            {
                return;
            }

            if (_stage.Map[y, x] == (byte)ItemType.Box || _stage.Map[y, x] == (byte)ItemType.BoxGoal)
            {
                if (_stage.Map[_y, _x] == (byte)ItemType.Wall
                    || _stage.Map[_y, _x] == (byte)ItemType.Box
                    || _stage.Map[_y, _x] == (byte)ItemType.BoxGoal
                    || _stage.Map[_y, _x] == (byte)ItemType.Empty)
                {
                    return;
                }

                Tuple<Coordinate, Rectangle> result = moveableItems.FirstOrDefault(p => (p.Item1.Type == ItemType.Box || p.Item1.Type == ItemType.BoxGoal)
                                                                                         && Canvas.GetLeft(p.Item2) / 30 == x && Canvas.GetTop(p.Item2) / 30 == y);
                if (result != null)
                {
                    PlaySound("Box");
                    MoveBox(x, y, _x, _y, true, false, result.Item2);
                    MoveMan(x, y, false, false, ref coordinate);
                    CheckWin();
                }
            }
            else
            {
                PlaySound("Man");
                MoveMan(x, y, true, false, ref coordinate);
            }
        }

        private void MoveBox(byte rawX, byte rawY, byte nextX, byte nextY, bool pushHis, bool isRegret, Rectangle boxRect)
        {
            _stage.Map[nextY, nextX] = Convert.ToByte(Math.Abs((byte)ItemType.Box * (isRegret ? -1 : 1) + _stage.Map[nextY, nextX]));
            _stage.Map[rawY, rawX] = Convert.ToByte(Math.Abs((byte)ItemType.Box * (isRegret ? 1 : -1) + _stage.Map[rawY, rawX]));
            if (isRegret)
            {
                Canvas.SetLeft(boxRect, rawX * 30);
                Canvas.SetTop(boxRect, rawY * 30);
                boxRect.Fill = ItemBrushFactory.GetItemBrush((ItemType)_stage.Map[rawY, rawX]);
            }
            else
            {
                Canvas.SetLeft(boxRect, nextX * 30);
                Canvas.SetTop(boxRect, nextY * 30);
                boxRect.Fill = ItemBrushFactory.GetItemBrush((ItemType)_stage.Map[nextY, nextX]);
            }

            if (pushHis)
            {
                _stage.HistoryStack.Push(new MoveHistory
                (
                    new Coordinate() { X = rawX, Y = rawY, Type = (ItemType)_stage.Map[rawY, rawX] },
                    new Coordinate() { X = nextX, Y = nextY, Type = (ItemType)_stage.Map[nextY, nextX] },
                    direction
                ));
                _stage.LURD = stepBuilder.Append(StaticObjManager.AnswersFormat[direction].Item2).ToString();
            }
        }

        private void MoveMan(byte nextX, byte nextY, bool pushHis, bool isRegret, ref Coordinate mancoor)
        {
            _stage.Map[nextY, nextX] = Convert.ToByte(Math.Abs((byte)ItemType.Man * (isRegret ? -1 : 1) + _stage.Map[nextY, nextX]));
            _stage.Map[mancoor.Y, mancoor.X] = Convert.ToByte(Math.Abs((byte)ItemType.Man * (isRegret ? 1 : -1) + _stage.Map[mancoor.Y, mancoor.X]));

            Tuple<Coordinate, Rectangle> manTuple = moveableItems.First();
            if (!isRegret)
            {
                Canvas.SetLeft(manTuple.Item2, nextX * 30);
                Canvas.SetTop(manTuple.Item2, nextY * 30);
                mancoor.Type = (ItemType)_stage.Map[nextY, nextX];
            }
            else
            {
                Canvas.SetLeft(manTuple.Item2, mancoor.X * 30);
                Canvas.SetTop(manTuple.Item2, mancoor.Y * 30);
                mancoor.Type = (ItemType)_stage.Map[mancoor.Y, mancoor.X];
            }
            manTuple.Item2.Fill = ItemBrushFactory.GetItemBrush(mancoor.Type);

            if (pushHis && !isRegret)
            {
                _stage.LURD = stepBuilder.Append(StaticObjManager.AnswersFormat[direction].Item1).ToString();
                _stage.HistoryStack.Push(new MoveHistory
                (
                    new Coordinate() { X = mancoor.X, Y = mancoor.Y, Type = (ItemType)_stage.Map[mancoor.Y, mancoor.X] },
                    new Coordinate() { X = nextX, Y = nextY, Type = (ItemType)_stage.Map[nextY, nextX] },
                    direction
                ));
            }

            if (!isRegret)
            {
                mancoor.X = nextX;
                mancoor.Y = nextY;
            }
        }

        private void CheckWin()
        {
            int boxcount = XSBMapParser.GetItemCount(_stage.Map, (byte)ItemType.Box);
            int goalcount = XSBMapParser.GetItemCount(_stage.Map, (byte)ItemType.Goal);

            if (goalcount == 0 || (goalcount > 0 && boxcount == 0))
            {
                if (Window.GetWindow(this) is IWindow window)
                {
                    if (window.GetPage("Stage") is IStage stage)
                    {
                        int index = stage.GetIndex(_stage);
                        if (index > -1)
                        {
                            PlaySound("Clear");

                            if (_stage.BestSteps == 0)
                            {
                                _stage.BestSteps = _stage.Steps;
                            }
                            else if (_stage.Steps <= _stage.BestSteps)
                            {
                                _stage.BestSteps = _stage.Steps;
                            }
                            _stage.Cleared = true;
                            _stage.CompletedMapString = Convert.ToBase64String(Encoding.UTF8.GetBytes(XSBMapParser.MapDataToString(_stage.Map)));
                            _stage.Hash = _stage.Hash;

                            StaticObjManager.MapParser.SaveLURD(
                                _stage,
                                AppDomain.CurrentDomain.BaseDirectory + "map\\finished\\" + (index + 1).ToString() + ".clear"
                            );

                            PART_ClearI.Visibility = Visibility.Visible;
                            PART_ClearII.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        private void RotateManElement(Rectangle element, double angle)
        {
            element.ClearValue(UIElement.RenderTransformProperty);
            element.RenderTransform = Rotations[angle];
        }

        private void ChangeManCoorStatus(byte x, byte y, ref Coordinate manCoor)
        {
            manCoor.X = x;
            manCoor.Y = y;
        }

        private void PlaySound(string soundName)
        {
            _musicDict[soundName].Position = 0;
            musicplayer.Stream = null;
            musicplayer.Stream = _musicDict[soundName];
            musicplayer.Play();
        }
    }
}
