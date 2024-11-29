using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sokoban.Componets;
using Sokoban.Model;

namespace Sokoban
{
    /// <summary>
    /// SetNewStageWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class SetNewStageWindow : Window
    {
        public event Action<Stage, bool> TransferNewStageEvent;
        private bool _isedit;
        private Stage _stage;

        public SetNewStageWindow()
        {
            InitializeComponent();

            BtnSave.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(SaveStage), true);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            Utils.IconHelper.RemoveIcon(this);
        }

        private void SaveStage(object sender, MouseButtonEventArgs e)
        {
            if (CheckTextIsDigit())
            {
                if (_stage == null)
                {
                    _stage = new()
                    {
                        Author = TB_Author.Text,
                        Title = TB_Title.Text,
                        Rows = Convert.ToInt32(TB_Rows.Text),
                        Cols = Convert.ToInt32(TB_Cols.Text),
                        Difficulty = Convert.ToByte(TB_Level.Text)
                    };
                    _stage.Map = new byte[Convert.ToByte(TB_Rows.Text), Convert.ToByte(TB_Rows.Text)];

                    unsafe
                    {
                        fixed (byte* a = &_stage.Map[0, 0])
                        {
                            byte* b = a;

                            for (int i = 0; i < _stage.Map.GetLength(0); i++)
                            {
                                for (int j = 0; j < _stage.Map.GetLength(1); j++)
                                {
                                    *b++ = 7;
                                }
                            }
                        }
                    }
                }
                else
                {
                    _stage.Author = TB_Author.Text;
                    _stage.Title = TB_Title.Text;
                    _stage.Difficulty = Convert.ToByte(TB_Level.Text);
                }

                TransferNewStageEvent?.Invoke(_stage, _isedit);
                Close();
            }
        }

        private bool CheckTextIsDigit()
        {
            //if (!Regex.IsMatch(TB_Index.Text, @"^(?!0)([1-9]\d*)$", RegexOptions.Compiled))
            //{
            //    ShowMessage(MessageType.Warning, "序号应大于或等于1", 2000);
            //    TB_Index.Focus();
            //    return false;
            //}
            if (!Regex.IsMatch(TB_Level.Text, @"^([1-9]|1[0-2])$", RegexOptions.Compiled))
            {
                ShowMessage(MessageType.Warning, "难度值应为1~12之间（包含首尾数字）", 2000);
                TB_Level.Focus();
                return false;
            }
            if (!CheckRowColIsDigit(TB_Rows.Text))
            {
                ShowMessage(MessageType.Warning, "行数值应为1~30之间（包含首尾数字）", 2000);
                TB_Rows.Focus();
                return false;
            }
            if (!CheckRowColIsDigit(TB_Cols.Text))
            {
                ShowMessage(MessageType.Warning, "列数值应为1~30之间（包含首尾数字）", 2000);
                TB_Cols.Focus();
                return false;
            }
            if (TB_Title.Text.Length > 8)
            {
                ShowMessage(MessageType.Warning, "关卡标题字数应不超过8位", 2000);
                TB_Title.Focus();
                return false;
            }
            if (TB_Author.Text.Length > 8)
            {
                ShowMessage(MessageType.Warning, "关卡作者字数应不超过8位", 2000);
                TB_Author.Focus();
                return false;
            }

            return true;
        }

        private void NewOrEdit(Stage stage, bool isEdit)
        {
            _stage = stage;
            _isedit = isEdit;
            TB_Rows.IsEnabled = !isEdit;
            TB_Cols.IsEnabled = !isEdit;
        }

        private void ShowMessage(MessageType type, string text, int duratime)
        {
            HideMessage();
            mask.Visibility = Visibility.Visible;
            MessageManage.Show(text, type, "NewStageMessageToken", duratime);

            if (duratime > 0)
            {
                Task.Run(async () =>
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
            MessageManage.Hide("NewStageMessageToken");
        }

        /// <summary>
        /// 判断行数或列数是否为合法的整数型
        /// </summary>
        /// <param name="rawText">待验证的源字符串</param>
        private static bool CheckRowColIsDigit(string rawText)
        {
            if (Regex.IsMatch(rawText, @"^([1-9]|[1-2][0-9]|30)$", RegexOptions.Compiled))
            {
                return true;
            }

            return false;
        }
    }
}
