using System;

using Sokoban.Notify;

namespace Sokoban.Model
{
    /// <summary>
    /// 关卡信息
    /// </summary>
    [Serializable]
    public sealed class Stage : NotifyPropertyBase
    {
        public Stage()
        {
            HistoryStack = new ObservableStack<MoveHistory> ();
        }

        private int index;
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                RaiseChanged(ref index, value);
            }
        }

        private string hash;
        public string Hash
        {
            get
            {
                return hash;
            }
            set
            {
                RaiseChanged(ref hash, value);
            }
        }

        private string title;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                RaiseChanged(ref title, value);
            }
        }

        private string author;
        /// <summary>
        /// 作者
        /// </summary>
        public string Author
        {
            get
            {
                return author;
            }
            set
            {
                RaiseChanged(ref author, value);
            }
        }

        private byte difficulty;
        /// <summary>
        /// 难度
        /// </summary>
        public byte Difficulty
        {
            get
            {
                return difficulty;
            }
            set
            {
                RaiseChanged(ref difficulty, value);
            }
        }

        /// <summary>
        /// 设计地图的行数
        /// </summary>
        public int Rows
        {
            get;
            set;
        }

        /// <summary>
        /// 设计地图的列数
        /// </summary>
        public int Cols
        {
            get;
            set;
        }

        /// <summary>
        /// 存放关卡地图未通关的初始数据
        /// </summary>
        public string RawMapString
        {
            get;
            set;
        }

        public byte[,] Map
        {
            get;
            set;
        }

        private bool cleared;
        public bool Cleared
        {
            get
            {
                return cleared;
            }
            set
            {
                RaiseChanged(ref cleared, value);
            }
        }

        private int steps;
        public int Steps
        {
            get
            {
                return steps;
            }
            set
            {
                RaiseChanged(ref steps, value);
            }
        }

        private int beststeps;
        public int BestSteps
        {
            get
            {
                return beststeps;
            }
            set
            {
                RaiseChanged(ref beststeps, value);
            }
        }

        public string Answerer
        {
            get;
            set;
        }

        private string lurd;
        public string LURD
        {
            get
            {
                return lurd;
            }
            set
            {
                RaiseChanged(ref lurd, value);
                if (string.IsNullOrWhiteSpace(value))
                {
                    Steps = 0;
                }
                else
                {
                    Steps = value.Length;
                }
            }
        }

        /// <summary>
        /// 先保留
        /// </summary>
        public string CompletedMapString
        {
            get;
            set;
        }

        public ObservableStack<MoveHistory> HistoryStack
        {
            get;
            set;
        }

        public void Reset()
        {
            BestSteps = 0;
            LURD = string.Empty;
            CompletedMapString = string.Empty;
            Hash = string.Empty;
            Cleared = false;
            HistoryStack.Clear();
        }
    }
}
