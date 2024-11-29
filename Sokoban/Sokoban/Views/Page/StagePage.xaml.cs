using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sokoban.Componets;
using Sokoban.Core;
using Sokoban.Model;
using Sokoban.Utils;

namespace Sokoban
{
    /// <summary>
    /// StagePage.xaml 的交互逻辑
    /// </summary>
    public sealed partial class StagePage : Page, IStage
    {
        private bool firstload = true;
        private int pageindex = -1;
        private readonly string errorlogfile;
        private readonly List<System.IO.FileInfo> stagefiles;
        private readonly List<System.IO.FileInfo> clearfiles;
        private readonly int pagecount;
        private readonly IList<Stage> stages;

        public StagePage()
        {
            stages = new List<Stage>();

            InitializeComponent();

            Loaded += StagePage_Loaded;

            BtnRefresh.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(RefreshCurrentPageStage), true);
            BtnBack.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(BackHome), true);
            BtnPrev.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(GetPrev), true);
            BtnLast.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(GetNext), true);

            Stages = new ObservableCollection<Stage>();
            StageItemsControl.ItemsSource = Stages;

            errorlogfile = AppDomain.CurrentDomain.BaseDirectory + "\\logs\\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".log";
            stagefiles = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "map\\").GetFiles("*.map", System.IO.SearchOption.TopDirectoryOnly).ToList();
            for (int i = stagefiles.Count - 1; i >= 0; i--)
            {
                if (!Regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(stagefiles[i].FullName), "^(?!0)([1-9]\\d*)$", RegexOptions.Compiled))
                {
                    stagefiles.Remove(stagefiles[i]);
                }
            }
            stagefiles.Sort(new FileNameSort());
            pagecount = (stagefiles.Count / 100) + (stagefiles.Count % 100 == 0 ? 0 : 1);

            clearfiles = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "map\\finished\\").GetFiles("*.clear", System.IO.SearchOption.TopDirectoryOnly).ToList();
            for (int i = clearfiles.Count - 1; i >= 0; i--)
            {
                if (!Regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(clearfiles[i].FullName), "^(?!0)([1-9]\\d*)$", RegexOptions.Compiled))
                {
                    stagefiles.Remove(stagefiles[i]);
                }
            }
            clearfiles.Sort(new FileNameSort());
        }

        public ObservableCollection<Stage> Stages
        {
            get;
        }

        private void StagePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (firstload)
            {
                firstload = false;
                pageindex++;
                RefreshData();
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            if (e.Delta > 0)
            {
                scrollviewer.LineUp();
                scrollviewer.LineUp();
            }
            else
            {
                scrollviewer.LineDown();
                scrollviewer.LineDown();
            }

            e.Handled = true;
        }

        private void StageClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (sender is StageControl control)
                {
                    if (control.DataContext is Stage stage)
                    {
                        if (!stage.Cleared)
                        {
                            stage.Steps = 0;
                        }

                        if (Window.GetWindow(this) is IWindow window)
                        {
                            if (window.GetPage("Game") is ILayoutGame layout)
                            {
                                window.ShowMessage(MessageType.Loading, "游戏初始化中...", 0);

                                layout.InitLayout(stage);
                                window.Navigate("Game");

                                window.ShowMessage(MessageType.Success, "初始化完毕", 1000);
                            }
                        }
                    }
                }
            }
        }

        private void BackHome(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is IWindow window)
            {
                window.Navigate("Home");
            }
        }

        private void GetPrev(object sender, MouseButtonEventArgs e)
        {
            if (pageindex > 0)
            {
                pageindex--;
            }

            RefreshData();
        }

        private void GetNext(object sender, MouseButtonEventArgs e)
        {
            pageindex++;
            RefreshData();
        }

        private void RefreshCurrentPageStage(object sender, MouseButtonEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            Stages.Clear();

            if (Window.GetWindow(this) is IWindow window)
            {
                window.ShowMessage(MessageType.Loading, "正在加载关卡数据...", 0);

                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);

                    int filecount = stagefiles.Count;
                    Dispatcher.Invoke(() =>
                    {
                        if (pagecount <= 1)
                        {
                            BtnPrev.Visibility = Visibility.Hidden;
                            BtnLast.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            if (pageindex + 1 >= pagecount)
                            {
                                BtnPrev.Visibility = Visibility.Visible;
                                BtnLast.Visibility = Visibility.Hidden;
                            }
                            else if (pageindex + 1 < pagecount && pageindex + 1 > 1)
                            {
                                filecount = (pageindex + 1) * 100;
                                BtnPrev.Visibility = Visibility.Visible;
                                BtnLast.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                filecount = (pageindex + 1) * 100;
                                BtnPrev.Visibility = Visibility.Hidden;
                                BtnLast.Visibility = Visibility.Visible;
                            }
                        }
                    });

                    if(stages.Count() == 0 || stages.Count() < pageindex * 100)
                    {
                        for (int i = pageindex * 100; i < filecount; i++)
                        {
                            Stage stage = null;

                            try
                            {
                                stage = StaticObjManager.MapParser.LoadXSB(stagefiles[i].FullName);
                                stage.Hash = ExtendMethod.ConfuseBytes(Encoding.UTF8.GetBytes(stage.RawMapString));
                                string stagefileindex = System.IO.Path.GetFileNameWithoutExtension(stagefiles[i].FullName);
                                stage.Index = Convert.ToInt32(stagefileindex);

                                System.IO.FileInfo clearfile = clearfiles.FirstOrDefault(p => stagefileindex == System.IO.Path.GetFileNameWithoutExtension(p.FullName));
                                if (clearfile != null)
                                {
                                    StaticObjManager.MapParser.LoadLURD(clearfile.FullName, stage);
                                    //if (string.Compare(stage.Hash, finish[0], StringComparison.Ordinal) == 0)
                                    //{
                                    //    string rawmap = Encoding.UTF8.GetString(Convert.FromBase64String(finish[1]));
                                    //    string[] map = rawmap.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                                    //    var group = map.GroupBy(p => p.Length);
                                    //    if (map.Length == stage.Rows && group.Count() == 1 && group.First().Key == stage.Cols)
                                    //    {
                                    //        bool _pass = true;
                                    //        int goalcount = 0;
                                    //        int boxcount = 0;
                                    //        foreach (string s in map)
                                    //        {
                                    //            if(s.Contains(XSBMapParser.GetItemChar(ItemType.Goal)))
                                    //            {
                                    //                goalcount++;
                                    //            }

                                    //            if (s.Contains(XSBMapParser.GetItemChar(ItemType.Box)))
                                    //            {
                                    //                boxcount++;
                                    //            }

                                    //            if (!XSBMapParser.CheckASCIICharacter(s))
                                    //            {
                                    //                _pass = false;
                                    //                break;
                                    //            }
                                    //        }

                                    //        if (_pass && (goalcount == 0 || (goalcount > 0 && boxcount == 0)))
                                    //        {
                                    //            stage.Steps = finish[2].Length;
                                    //            stage.BestSteps = Convert.ToInt32(finish[3]);
                                    //            stage.Cleared = true;
                                    //            stage.LURD = finish[2];
                                    //            stage.CompletedMapString = rawmap;
                                    //            stage.HistoryStack = CreateHistroiesFromLURD(stage.Map, stage.LURD);

                                    //            int _row = 0;
                                    //            foreach (string s in map)
                                    //            {
                                    //                for (int j = 0; j < s.Length; j++)
                                    //                {
                                    //                    stage.Map[_row, j] = XSBMapParser.GetItemValue(s[j].ToString());
                                    //                }
                                    //                _row++;
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                System.IO.File.AppendAllText(errorlogfile, Environment.NewLine + ex.Message);
                            }
                            catch (System.IO.FileFormatException ex)
                            {
                                System.IO.File.AppendAllText(errorlogfile, Environment.NewLine + ex.Message);
                            }
                            catch (System.Xml.XmlException ex)
                            {
                                System.IO.File.AppendAllText(errorlogfile, Environment.NewLine + ex.Message);
                            }
                            catch (Exception ex)
                            {
                                System.IO.File.AppendAllText(errorlogfile, Environment.NewLine + ex.Message);
                            }
                            finally
                            {
                                if (stage != null)
                                {
                                    stages.Add(stage);

                                    Dispatcher.Invoke(() =>
                                    {
                                        Stages.Add(stage);
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = pageindex * 100; i < filecount; i++)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Stages.Add(stages[i]);
                            });
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        if (Stages.Count > 0)
                        {
                            window.ShowMessage(MessageType.Success, "关卡数据加载完毕", 2000);
                        }
                        else
                        {
                            window.ShowMessage(MessageType.Error, "未能找到关卡文件", 2000);
                        }
                    });
                });
            }
        }

        public Stage GetNextStage(Stage currentStage)
        {
            if (currentStage == null)
            {
                return null;
            }

            int index = Stages.IndexOf(currentStage);
            if (index > -1)
            {
                if (Stages.Count - 1 == index)
                {
                    return null;
                }
                else
                {
                    return Stages[index + 1];
                }
            }

            return null;
        }

        public int GetIndex(Stage currentStage)
        {
            return Stages.IndexOf(currentStage);
        }
    }
}
