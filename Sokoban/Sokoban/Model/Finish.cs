using System;
using System.Xml.Serialization;

using Sokoban.Notify;

namespace Sokoban.Model
{
    [XmlRoot(ElementName = "finished")]
    [Serializable]
    public sealed class Finish : NotifyPropertyBase
    {
        public Finish()
        {
            HistoryStack = new ObservableStack<MoveHistory>();
        }

        private bool cleared;
        [XmlAttribute(AttributeName = "cleared")]
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
        [XmlAttribute(AttributeName = "steps")]
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
        [XmlAttribute(AttributeName = "beststeps")]
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

        [XmlElement(ElementName = "hash")]
        public string Hash
        {
            get;
            set;
        }

        [XmlElement(ElementName = "answerer", IsNullable = true)]
        public string Answerer
        {
            get;
            set;
        }

        private string lurd;
        [XmlElement(ElementName = "lurd")]
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

        [XmlElement(ElementName = "map")]
        public string CompletedMapString
        {
            get;
            set;
        }

        [XmlIgnore]
        public ObservableStack<MoveHistory> HistoryStack
        {
            get;
            set;
        }

        public bool ShouldSerializeAnswerer()
        {
            return !string.IsNullOrEmpty(Answerer);
        }
    }
}
