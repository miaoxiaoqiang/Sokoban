using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using System.Xml;

using Sokoban.Utils;
using Sokoban.Componets;
using Sokoban.Model;
using Sokoban.Core.Parser;
using System.IO;

namespace Sokoban
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class MapEditWindow : Window
    {
        private readonly OpenFileDialog openFileDialog;
        private readonly SaveFileDialog saveFileDialog;
        private readonly SetNewStageWindow setNewStageWindow;
        private readonly MethodInfo method;

        private string selectImage = string.Empty;
        private Stage _stageInfo;
        private bool isselectitem = false;

        public MapEditWindow()
        {
            StagesItems = new();
            openFileDialog = new OpenFileDialog()
            {
                DefaultExt = "map",
                Filter = "关卡地图文件|*.map",
                Multiselect = true,
                Title = "文件选择",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "map"
            };
            saveFileDialog = new SaveFileDialog()
            {
                DefaultExt = "map",
                Filter = "关卡地图文件|*.map",
                Title = "文件保存",
                OverwritePrompt = true,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "map"
            };

            InitializeComponent();

            BtnSave.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(SaveStage), true);
            BtnNewStage.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(NewStage), true);
            BtnClearAll.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ClearAll), true);
            BtnOpenFile.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectFile), true);
            BtnLayoutCurrentStage.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LayoutCurrentStage), true);
            BtnEditCurrentStage.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(EditCurrentStage), true);
            BtnDelCurrentStage.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(DelCurrenStage), true);
            mapcanvas.AddHandler(Canvas.MouseDownEvent, new MouseButtonEventHandler(CanvasClick), true);

            Wall.AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler(SuCai_Checked), true);
            Man.AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler(SuCai_Checked), true);
            Goal.AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler(SuCai_Checked), true);
            Floor.AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler(SuCai_Checked), true);
            Box.AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler(SuCai_Checked), true);
            DefaultCursor.AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler(SuCai_Checked), true);
            StageItemsList.AddHandler(ListBox.PreviewMouseDownEvent, new MouseButtonEventHandler(ListBoxMouseButtonDown), true);
            StageItemsList.ContextMenuOpening += ListBoxContextMenuOpening;
            StageItemsList.ItemsSource = StagesItems;

            setNewStageWindow = new SetNewStageWindow();
            setNewStageWindow.TransferNewStageEvent += GetNewStage;
            Type type = setNewStageWindow.GetType();
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            method = type.GetMethod("NewOrEdit", flags);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(Owner != null)
            {
                Owner.Visibility = Visibility.Visible;
            }
            e.Cancel = true;
            Hide();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            Utils.IconHelper.RemoveIcon(this);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Left = (SystemParameters.PrimaryScreenWidth / 2) - (ActualWidth / 2);
            Top = (SystemParameters.PrimaryScreenHeight / 2) - (ActualHeight / 2);
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            FrameworkElement overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            FrameworkElement mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }

        public ObservableCollection<Stage> StagesItems
        {
            get;
        }
        
        private void ListBoxMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                e.Handled = false;
            }
            else
            {
                isselectitem = false;
                if (StagesItems.Count > 0)
                {
                    ListBoxItem item = ItemsControl.ContainerFromElement((ItemsControl)e.Source, e.OriginalSource as DependencyObject) as ListBoxItem;

                    if (item != null)
                    {
                        Stage stage = item.DataContext as Stage;
                        Stage stage1 = StageItemsList.SelectedItem as Stage;
                        if (stage != null && stage1 != null && stage1 == stage)
                        {
                            isselectitem = true;
                            e.Handled = false;
                        }
                        else
                        {
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void ListBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (isselectitem)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void NewStage(object sender, MouseButtonEventArgs e)
        {
            method?.Invoke(setNewStageWindow, new object[] { null, Convert.ChangeType(false, typeof(bool)) });

            double newtop = Top + Height / 2;
            double newleft = Left + Width / 2;
            setNewStageWindow.Top = newtop - setNewStageWindow.Height / 2;
            setNewStageWindow.Left = newleft - setNewStageWindow.Width / 2;
            setNewStageWindow.ShowDialog();
        }

        private void SaveStage(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_stageInfo == null)
                {
                    ShowMessage(MessageType.Info, "请先新建关卡内容或\r\n打开一个关卡地图文件", 1500);
                    return;
                }

                if (saveFileDialog.ShowDialog() == true)
                {
                    ShowMessage(MessageType.Loading, "正保存关卡地图中...", 0);
                    _stageInfo.RawMapString = XSBMapParser.MapDataToString(_stageInfo.Map);
                    _stageInfo.Hash = ExtendMethod.ConfuseBytes(Encoding.UTF8.GetBytes(_stageInfo.RawMapString));

                    Core.StaticObjManager.MapParser.SaveXSB(_stageInfo, saveFileDialog.FileName);
                    //StagesItems.Add(_stageInfo);

                    ShowMessage(MessageType.Success, $"关卡地图【{saveFileDialog.SafeFileName}】已保存", 2000);
                }
            }
            catch (ArgumentException ex)
            {
                ShowMessage(MessageType.Error, ex.Message, 5000);
            }
            catch (FileFormatException ex)
            {
                ShowMessage(MessageType.Error, ex.Message, 5000);
            }
            catch (Exception ex)
            {
                ShowMessage(MessageType.Error, ex.Message, 5000);
            }
        }

        private void EditCurrentStage(object sender, MouseButtonEventArgs e)
        {
            if (StagesItems.Count == 0)
            {
                ShowMessage(MessageType.Info, "列表中未有关卡信息", 2000);
                return;
            }

            Stage info = StageItemsList.SelectedItem as Stage;
            if (info == null)
            {
                ShowMessage(MessageType.Info, "请先选择关卡再编辑", 2000);
                return;
            }

            method?.Invoke(setNewStageWindow, new object[] { Convert.ChangeType(info, typeof(Stage)), Convert.ChangeType(true, typeof(bool)) });

            double newtop = Top + Height / 2;
            double newleft = Left + Width / 2;
            setNewStageWindow.Top = newtop - setNewStageWindow.Height / 2;
            setNewStageWindow.Left = newleft - setNewStageWindow.Width / 2;
            setNewStageWindow.ShowDialog();
        }

        private void LayoutCurrentStage(object sender, MouseButtonEventArgs e)
        {
            if (StagesItems.Count == 0)
            {
                ShowMessage(MessageType.Info, "列表中未有关卡信息", 2000);
                return;
            }

            Stage info = StageItemsList.SelectedItem as Stage;
            if (info == null)
            {
                ShowMessage(MessageType.Info, "请先选择关卡再布局", 2000);
                return;
            }

            _stageInfo = info;
            ResetLayout();

            ShowMessage(MessageType.Success, "布局已初始化", 1500);
        }

        private void DelCurrenStage(object sender, MouseButtonEventArgs e)
        {
            if (StagesItems.Count == 0)
            {
                ShowMessage(MessageType.Info, "列表中未有关卡信息", 1500);
                return;
            }

            if (StageItemsList.SelectedItem is Stage info)
            {
                StagesItems.Remove(info);
                ClearText();
                ShowMessage(MessageType.Success, "已移除此关卡数据", 2000);
            }
        }

        private void SelectFile(object sender, MouseButtonEventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                ShowMessage(MessageType.Loading, $"正在解析已选择的关卡地图", 0);

                foreach (string file in openFileDialog.FileNames)
                {
                    try
                    {
                        Stage _model = Core.StaticObjManager.MapParser.LoadXSB(file);

                        if (_model != null && !StagesItems.Any(p => string.Compare(p.Hash, _model.Hash, StringComparison.Ordinal) == 0))
                        {
                            StagesItems.Add(_model);
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        Logger.WriteLine(ex.Message, LogViewerLib.StringStyleEnum.errorText);
                    }
                    catch (FileFormatException)
                    {
                        Logger.WriteLine($"关卡文件[{System.IO.Path.GetFileName(file)}] " + Properties.Resources.InvalidMapData, LogViewerLib.StringStyleEnum.errorText);
                    }
                    catch (Exception)
                    {
                        Logger.WriteLine($"关卡文件[{System.IO.Path.GetFileName(file)}] " + Properties.Resources.InvalidMapData, LogViewerLib.StringStyleEnum.errorText);
                    }
                }

                ShowMessage(MessageType.Success, $"已选择的关卡地图解析完成", 2000);
            }
        }

        private void ClearAll(object sender, MouseButtonEventArgs e)
        {
            StagesItems.Clear();
            ClearText();
        }

        private void CanvasClick(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(selectImage))
            {
                return;
            }

            if (e.Source is Rectangle rectangle)
            {
                string tagstr = rectangle.Tag.ToString();
                ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), selectImage);

                if (e.ChangedButton == MouseButton.Left)
                {
                    if (string.Compare(tagstr, "Empty") == 0)
                    {
                        if (string.Compare(selectImage, "Wall") == 0 || string.Compare(selectImage, "Floor") == 0)
                        {
                            AddItem(itemType, rectangle);
                        }
                        else
                        {
                            ShowMessage(MessageType.Warning, "空地区域只能放墙和地砖这两个元素", 1500);
                        }
                    }
                    else
                    {
                        if (string.Compare(tagstr, "Goal") == 0 && string.Compare(selectImage, "Box") == 0) //在目标点放箱子
                        {
                            AddItem(ItemType.BoxGoal, rectangle);
                        }
                        else if (string.Compare(tagstr, "Box") == 0 && string.Compare(selectImage, "Goal") == 0) //在箱子处放目标点
                        {
                            AddItem(itemType, rectangle);

                            Canvas.SetZIndex(rectangle, (int)ItemType.BoxGoal);
                            rectangle.Fill = ItemBrushFactory.GetItemBrush(ItemType.BoxGoal);
                            rectangle.Tag = Enum.GetName(typeof(ItemType), ItemType.BoxGoal);
                        }
                        else if (string.Compare(tagstr, "Goal") == 0 && string.Compare(selectImage, "Man") == 0) //在目标点放人物
                        {
                            AddItem(ItemType.ManGoal, rectangle);
                        }
                        else if (string.Compare(tagstr, "Man") == 0 && string.Compare(selectImage, "Goal") == 0)
                        {
                            AddItem(itemType, rectangle);

                            Canvas.SetZIndex(rectangle, (int)ItemType.ManGoal);
                            rectangle.Fill = ItemBrushFactory.GetItemBrush(ItemType.ManGoal);
                            rectangle.Tag = Enum.GetName(typeof(ItemType), ItemType.ManGoal);
                        }
                        else if (string.Compare(tagstr, "Floor") == 0 &&
                                   (string.Compare(selectImage, "Man") == 0
                                    || string.Compare(selectImage, "Goal") == 0
                                    || string.Compare(selectImage, "Box") == 0))
                        {
                            AddItem(itemType, rectangle);
                        }
                        else
                        {
                            ShowMessage(MessageType.Warning, "此位置已有元素", 1500);
                        }
                    }
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    if (string.Compare(tagstr, "Empty") != 0)
                    {
                        //string _idstr = rectangle.Name.Substring(5);
                        int x = Convert.ToInt32(Canvas.GetLeft(rectangle) / 30);
                        int y = Convert.ToInt32(Canvas.GetTop(rectangle) / 30);
                        byte _byte = 7;
                        if (string.Compare(tagstr, "BoxGoal") == 0 || string.Compare(tagstr, "ManGoal") == 0)
                        {
                            _byte = 1;
                        }
                        _stageInfo.Map[y, x] = _byte;
                        mapcanvas.Children.Remove(rectangle);
                    }
                }
            }
        }

        private void SuCai_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = e.Source as RadioButton;

            if (radioButton.IsChecked == true)
            {
                if (radioButton.Name == "DefaultCursor")
                {
                    mapcanvas.Cursor = Cursors.Arrow;
                    selectImage = string.Empty;
                }
                else
                {
                    selectImage = radioButton.Name;
                    mapcanvas.Cursor = CursorFactory.GetCursor(radioButton.Name);
                }
            }
        }

        private void GetNewStage(Stage stage, bool isEdit)
        {
            if (isEdit)
            {
                StageItemsList.SelectedItem = null;
                StageItemsList.SelectedIndex = -1;

                ShowMessage(MessageType.Success, "数据已更新", 1500);
            }
            else
            {
                StagesItems.Add(stage);
                _stageInfo = stage;
                ShowMessage(MessageType.Success, "新关卡已添加", 1500);
            }
        }

        private void ClearText()
        {
            if (mapcanvas.Children.Count > 0)
            {
                mapcanvas.Children.Clear();
                mapcanvas.ClearValue(WidthProperty);
                mapcanvas.ClearValue(HeightProperty);
            }

            _stageInfo = null;
        }

        private void AddItem(ItemType item, Rectangle rect)
        {
            Rectangle newRectangle = new()
            {
                Width = 30,
                Height = 30,
                Fill = ItemBrushFactory.GetItemBrush(item),
                Tag = Enum.GetName(typeof(ItemType), item)
            };

            Canvas.SetZIndex(newRectangle, (int)item);
            Canvas.SetLeft(newRectangle, Canvas.GetLeft(rect));
            Canvas.SetTop(newRectangle, Canvas.GetTop(rect));
            mapcanvas.Children.Add(newRectangle);

            byte x = Convert.ToByte(Canvas.GetLeft(rect) / 30);
            byte y = Convert.ToByte(Canvas.GetTop(rect) / 30);
            _stageInfo.Map[y, x] = (byte)item;
        }

        private void ResetLayout()
        {
            if (mapcanvas.Children.Count > 0)
            {
                mapcanvas.Children.Clear();
            }
            mapcanvas.ClearValue(WidthProperty);
            mapcanvas.ClearValue(HeightProperty);

            mapcanvas.Width = 30 * _stageInfo.Cols;
            mapcanvas.Height = 30 * _stageInfo.Rows;
            for (int y = 0; y < _stageInfo.Rows; y++)
            {
                for (int x = 0; x < _stageInfo.Cols; x++)
                {
                    Rectangle rectangle = new()
                    {
                        Width = 30,
                        Height = 30,
                        Tag = "Empty",
                        Fill = ItemBrushFactory.GetItemBrush(ItemType.Empty)
                    };
                    Canvas.SetLeft(rectangle, x * 30);
                    Canvas.SetTop(rectangle, y * 30);
                    mapcanvas.Children.Add(rectangle);

                    ItemType item = (ItemType)_stageInfo.Map[y, x];
                    if (item != ItemType.Empty)
                    {
                        if (item == ItemType.Wall || item == ItemType.Floor)
                        {
                            Rectangle rectangle3 = new()
                            {
                                Width = 30,
                                Height = 30,
                                Fill = ItemBrushFactory.GetItemBrush(item),
                                Tag = item.ToString()
                            };
                            Canvas.SetZIndex(rectangle3, _stageInfo.Map[y, x]);
                            Canvas.SetLeft(rectangle3, x * 30);
                            Canvas.SetTop(rectangle3, y * 30);
                            mapcanvas.Children.Add(rectangle3);

                            continue;
                        }
                        else
                        {
                            Rectangle rectanglefloor = new()
                            {
                                Width = 30,
                                Height = 30,
                                Fill = ItemBrushFactory.GetItemBrush(ItemType.Floor)
                            };
                            Canvas.SetZIndex(rectanglefloor, (byte)ItemType.Floor);
                            Canvas.SetLeft(rectanglefloor, x * 30);
                            Canvas.SetTop(rectanglefloor, y * 30);
                            mapcanvas.Children.Add(rectanglefloor);

                            Rectangle rectangle1 = new()
                            {
                                Width = 30,
                                Height = 30,
                                Fill = ItemBrushFactory.GetItemBrush(item),
                                Tag = item.ToString()
                            };
                            Canvas.SetZIndex(rectangle1, _stageInfo.Map[y, x]);
                            Canvas.SetLeft(rectangle1, x * 30);
                            Canvas.SetTop(rectangle1, y * 30);
                            mapcanvas.Children.Add(rectangle1);
                        }
                    }
                }
            }
            MaterialList.IsEnabled = true;
        }

        private void ShowMessage(MessageType type, string text, int duratime)
        {
            HideMessage();
            mask.Visibility = Visibility.Visible;
            MessageManage.Show(text, type, "MainMessageToken1", duratime);

            if (duratime > 0)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(duratime);
                    this.Dispatcher.Invoke(() =>
                    {
                        HideMessage();
                    });
                });
            }
        }

        private void HideMessage()
        {
            mask.Visibility = Visibility.Collapsed;
            MessageManage.Hide("MainMessageToken1");
        }
    }
}
