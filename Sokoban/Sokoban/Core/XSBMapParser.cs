using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Sokoban.Model;
using Sokoban.Utils;

namespace Sokoban.Core.Parser
{
    /// <summary>
    /// 关卡地图和答案解析
    /// </summary>
    internal sealed class XSBMapParser
    {
        private readonly List<Coordinate> wallCoordinates;
        private readonly RijndaelManaged rijndael;
        private readonly StringBuilder lurdBuilder;
        private readonly IReadOnlyList<(string Header, int Index, string Pattern)> splitlurd;
        private readonly Encoding encoding;
        private readonly IDictionary<string, int> xsbMetaData;

        public XSBMapParser()
        {
            encoding = new UTF8Encoding(false);
            xsbMetaData = new Dictionary<string, int>()
            {
                { "Title", 6 },
                { "Author", 7 },
            };
            splitlurd = new List<(string Header, int Index, string Pattern)>()
            {
                ValueTuple.Create("RawHash:", 8, @"^([0-9A-Za-z])*$"),
                ValueTuple.Create("CompletedHash:", 14, @"^([0-9A-Za-z])*$"),
                ValueTuple.Create("Map:", 4, string.Empty),
                ValueTuple.Create("Answer:", 7, @"^[lurdLURD]+$"),
                ValueTuple.Create("BestSteps:", 10, @"^(0|[1-9][0-9]*)$"),
            };
            lurdBuilder = new StringBuilder();
            wallCoordinates = new();
            rijndael = new RijndaelManaged()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = new byte[]
                {
                    0x56, 0x53, 0x7A, 0x71, 0x45, 0x3D, 0x54, 0x7C, 0x3F, 0x47, 0x61, 0x74, 0x37, 0x31, 0x2A, 0x55,
                    0x5B, 0x3B, 0x40, 0x4F, 0x51, 0x5F, 0x70, 0x47, 0x35, 0x5B, 0x5F, 0x22, 0x70, 0x5D, 0x75, 0x64
                },
                IV = new byte[]
                {
                    0x44, 0x72, 0x37, 0x68, 0x54, 0x6B, 0x5F, 0x40, 0x27, 0x5B, 0x33, 0x65, 0x36, 0x76, 0x49, 0x66
                }
            };
        }

        public Stage LoadXSB(string filepath)
        {
            string strXML = File.ReadAllText(filepath);

            List<string> textlist = File.ReadAllLines(filepath, encoding).ToList();
            if (textlist == null || textlist.Count == 0)
            {
                throw new ArgumentException(Properties.Resources.NillOrEmpty);
            }

            if (!Regex.IsMatch(textlist[0], @"^;([1-9]|[1-2][0-9]|30),([1-9]|[1-2][0-9]|30),([1-9]|1[0-2])$", RegexOptions.Compiled))
            {
                throw new ArgumentException(Properties.Resources.InvalidMapData);
            }

            //读取第一行注释信息
            string[] remark = textlist[0].Substring(1).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            textlist.RemoveAt(0);

            int mapRows = Convert.ToInt32(remark[0]);
            int mapCols = Convert.ToInt32(remark[1]);

            Stage stage = new();
            Type _type = stage.GetType();
            foreach (KeyValuePair<string, int> item in xsbMetaData)
            {
                int index = textlist.FindIndex(p => p.StartsWith(item.Key + ":", StringComparison.OrdinalIgnoreCase));
                if (index > -1)
                {
                    object v = Convert.ChangeType(textlist[index].Substring(item.Value).Trim(), _type.GetProperty(item.Key).PropertyType);
                    _type.GetProperty(item.Key).SetValue(stage, v, null);
                    textlist.RemoveAt(index);
                }
            }

            int _rows = 0;
            for (int i = textlist.Count - 1; i >= 0; i--)
            {
                if (CheckASCIICharacter(textlist[i]))
                {
                    if (textlist[i].Length != mapCols)
                    {
                        throw new FileFormatException(Properties.Resources.InvalidMapData);
                    }
                    else
                    {
                        _rows++;
                    }
                }
                else
                {
                    textlist.RemoveAt(i);
                }
            }
            if (_rows != mapRows)
            {
                throw new FileFormatException(Properties.Resources.InvalidMapData);
            }

            stage.Cols = mapCols;
            stage.Rows = mapRows;
            stage.Map = new byte[mapRows, mapCols];
            StringBuilder rawBuilder = new();
            foreach (int index in Enumerable.Range(0, textlist.Count))
            {
                rawBuilder.AppendLine(textlist[index]);
                for (int i = 0; i < textlist[index].Length; i++)
                {
                    stage.Map[index, i] = GetItemValue(textlist[index][i].ToString());
                }
            }
            CheckMapData(stage.Map);
            stage.Difficulty = Convert.ToByte(remark[2]);
            stage.RawMapString = rawBuilder.ToString().TrimEnd('\r', '\n');
            stage.Hash = ExtendMethod.ConfuseBytes(Encoding.UTF8.GetBytes(stage.RawMapString));

            return stage;
        }

        public void SaveXSB(Stage stage, string filepath)
        {
            if (stage == null)
            {
                throw new ArgumentException(Properties.Resources.InvalidMapData);
            }

            CheckMapData(stage.Map);

            if (File.Exists(filepath))
            {
                FileInfo fi = new(filepath);
                if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                {
                    fi.Attributes = FileAttributes.Normal;
                }
            }

            StringBuilder sb = new();
            sb.AppendLine($";{stage.Rows.ToString()},{stage.Cols.ToString()},{stage.Difficulty.ToString()}");
            sb.AppendLine($"{stage.RawMapString}");
            sb.AppendLine($"Title:{stage.Title}");
            sb.Append($"Author:{stage.Title}");
            File.WriteAllText(filepath, sb.ToString(), encoding);
            File.SetAttributes(filepath, FileAttributes.ReadOnly);
        }

        public void LoadLURD(string filepath, Stage stage)
        {
            byte[] _lurddata = File.ReadAllBytes(filepath);
            if (_lurddata == null || _lurddata.Length == 0 || stage == null)
            {
                throw new ArgumentException(Properties.Resources.NillOrEmpty);
            }

            IList<string> strings = new List<string>();

            using (ICryptoTransform cTransform = rijndael.CreateDecryptor())
            {
                byte[] lurddata = cTransform.TransformFinalBlock(_lurddata, 0, _lurddata.Length).BytesTrimEnd();
                string[] lurdarrays = Encoding.UTF8.GetString(lurddata).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (lurdarrays.Length != 5)
                {
                    throw new FileFormatException(Properties.Resources.InvalidLURDData);
                }

                bool @checked = true;
                foreach (string lurdarray in lurdarrays)
                {
                    ValueTuple<string, int, string> values = splitlurd.FirstOrDefault(p => lurdarray.StartsWith(p.Header));
                    if (values.Equals(default(ValueTuple<string, int, string>)))
                    {
                        @checked = false;
                        break;
                    }

                    string content = lurdarray.Substring(values.Item2);
                    if (!string.IsNullOrEmpty(values.Item3))
                    {
                        if (!Regex.IsMatch(content, values.Item3))
                        {
                            @checked = false;
                            break;
                        }
                        strings.Add(content);
                    }
                    else
                    {
                        strings.Add(content);
                    }
                }

                if (!@checked)
                {
                    throw new FileFormatException(Properties.Resources.InvalidLURDData);
                }
            }

            string rawmap = Encoding.UTF8.GetString(Convert.FromBase64String(strings[2]));
            if (string.Compare(stage.Hash, strings[0], StringComparison.Ordinal) == 0
                && string.Compare(strings[1], ExtendMethod.ConfuseBytes(Encoding.UTF8.GetBytes(strings[2])), StringComparison.Ordinal) == 0)
            {
                string[] map = rawmap.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var group = map.GroupBy(p => p.Length);
                if (map.Length == stage.Rows && group.Count() == 1 && group.First().Key == stage.Cols)
                {
                    bool _pass = true;
                    int goalcount = 0;
                    int boxcount = 0;
                    foreach (string s in map)
                    {
                        if (s.Contains(GetItemChar(ItemType.Goal)))
                        {
                            goalcount++;
                        }

                        if (s.Contains(GetItemChar(ItemType.Box)))
                        {
                            boxcount++;
                        }

                        if (!CheckASCIICharacter(s))
                        {
                            _pass = false;
                            break;
                        }
                    }

                    if (_pass && (goalcount == 0 || (goalcount > 0 && boxcount == 0)))
                    {
                        stage.Steps = strings[3].Length;
                        stage.BestSteps = Convert.ToInt32(strings[4]);
                        stage.Cleared = true;
                        stage.LURD = strings[3];
                        stage.CompletedMapString = rawmap;
                        stage.HistoryStack = CreateHistroiesFromLURD(stage.Map, stage.LURD);

                        //int _row = 0;
                        //foreach (string s in map)
                        //{
                        //    for (int j = 0; j < s.Length; j++)
                        //    {
                        //        stage.Map[_row, j] = GetItemValue(s[j].ToString());
                        //    }
                        //    _row++;
                        //}
                    }
                }
            }
        }

        public void SaveLURD(Stage stage, string filepath)
        {
            lurdBuilder.Clear();

            lurdBuilder.AppendLine($"RawHash:{stage.Hash}");
            lurdBuilder.AppendLine($"CompletedHash:{ExtendMethod.ConfuseBytes(Encoding.UTF8.GetBytes(stage.CompletedMapString))}");
            lurdBuilder.AppendLine($"Map:{stage.CompletedMapString}");
            lurdBuilder.AppendLine($"Answer:{stage.LURD}");
            lurdBuilder.Append($"BestSteps:{stage.BestSteps.ToString()}");

            using (ICryptoTransform cTransform = rijndael.CreateEncryptor())
            {
                byte[] data = Encoding.UTF8.GetBytes(lurdBuilder.ToString());
                byte[] resultArray = cTransform.TransformFinalBlock(data, 0, data.Length);
                File.WriteAllBytes(filepath, resultArray);
            }
        }

        private void CheckMapData(byte[,] map)
        {
            wallCoordinates.Clear();
            string[] _strings = MapDataToString(map).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Coordinate yMax = new();

            for (byte i = 0; i < _strings.Length; i++)
            {
                if (!CheckASCIICharacter(_strings[i]))
                {
                    throw new FormatException(Properties.Resources.InvalidASCIIFormat);
                }
                else
                {
                    for (byte j = 0; j < _strings[i].Length; j++)
                    {
                        if (map[i, j] == (byte)ItemType.Wall)
                        {
                            if (i >= yMax.Y && j <= yMax.X)
                            {
                                yMax = new Coordinate() { X = j, Y = i, Type = ItemType.Wall };
                            }
                            wallCoordinates.Add(new Coordinate() { X = j, Y = i, Type = ItemType.Wall });
                        }
                    }
                }
            }

            int goalcount = GetItemCount(map, (byte)ItemType.Goal);
            int mancount = GetItemCount(map, (byte)ItemType.Man);
            int mangoalcount = GetItemCount(map, (byte)ItemType.ManGoal);
            int boxcount = GetItemCount(map, (byte)ItemType.Box);
            int boxgoalcount = GetItemCount(map, (byte)ItemType.BoxGoal);

            if (map.GetLength(0) < 3 && map.GetLength(1) < 5
                || map.GetLength(0) < 5 && map.GetLength(1) < 3
                || mancount + mangoalcount != 1 || goalcount == 0
                || goalcount == boxgoalcount && goalcount > 0
                || (boxcount + boxgoalcount < goalcount && goalcount > 0)
                || wallCoordinates.GroupBy(p => p.X).Count() == 1
                || wallCoordinates.GroupBy(p => p.Y).Count() == 1)
            {
                throw new FormatException(Properties.Resources.InvalidStage);
            }
        }

        private Notify.ObservableStack<MoveHistory> CreateHistroiesFromLURD(byte[,] map, string lurdString)
        {
#pragma warning disable IDE0042
            var manpos = XSBMapParser.GetManPos(map);
#pragma warning restore IDE0042

            Coordinate mancoor = new Coordinate()
            {
                X = manpos.ManX,
                Y = manpos.ManY,
                Type = (ItemType)map[manpos.ManY, manpos.ManX]
            };
            IList<MoveHistory> histories = new List<MoveHistory>();

            for (int chindex = 0; chindex < lurdString.Length; chindex++)
            {
                histories.Add(GetMove(map, lurdString[chindex], ref mancoor));
            }

            return new Notify.ObservableStack<MoveHistory>(histories);
        }

        private MoveHistory GetMove(byte[,] map, char ch, ref Coordinate manCoor)
        {
            Direction direction = Direction.Down;
            int stepX = 0;
            int stepY = 0;

            switch (ch)
            {
                case 'l':
                case 'L':
                    stepX = -1;
                    direction = Direction.Left;
                    break;
                case 'r':
                case 'R':
                    stepX = 1;
                    direction = Direction.Right;
                    break;
                case 'u':
                case 'U':
                    stepY = -1;
                    direction = Direction.Up;
                    break;
                case 'd':
                case 'D':
                    stepY = 1;
                    direction = Direction.Down;
                    break;
            }

            if (char.IsLower(ch))
            {
                byte _manx = Convert.ToByte(manCoor.X + stepX);
                byte _many = Convert.ToByte(manCoor.Y + stepY);
                map[_many, _manx] = Convert.ToByte(Math.Abs((byte)ItemType.Man + map[_many, _manx]));
                map[manCoor.Y, manCoor.X] = Convert.ToByte(Math.Abs((byte)ItemType.Man - map[manCoor.Y, manCoor.X]));

                MoveHistory history = new MoveHistory
                (
                    new Coordinate() { X = manCoor.X, Y = manCoor.Y, Type = (ItemType)map[manCoor.Y, manCoor.X] },
                    new Coordinate() { X = _manx, Y = _many, Type = (ItemType)map[_many, _manx] },
                    direction
                );

                manCoor.X = _manx;
                manCoor.Y = _many;
                manCoor.Type = (ItemType)map[_many, _manx];

                return history;
            }
            else
            {
                //移动箱子
                byte rawboxX = Convert.ToByte(manCoor.X + stepX);
                byte rawboxY = Convert.ToByte(manCoor.Y + stepY);
                byte nextboxX = Convert.ToByte(rawboxX + stepX);
                byte nextboxY = Convert.ToByte(rawboxY + stepY);
                map[nextboxY, nextboxX] = Convert.ToByte(Math.Abs((byte)ItemType.Box + map[nextboxY, nextboxX]));
                map[rawboxY, rawboxX] = Convert.ToByte(Math.Abs((byte)ItemType.Box - map[rawboxY, rawboxX]));

                //移动人
                map[rawboxY, rawboxX] = Convert.ToByte(Math.Abs((byte)ItemType.Man + map[rawboxY, rawboxX]));
                map[manCoor.Y, manCoor.X] = Convert.ToByte(Math.Abs((byte)ItemType.Man - map[manCoor.Y, manCoor.X]));

                MoveHistory history = new MoveHistory
                (
                    new Coordinate() { X = rawboxX, Y = rawboxY, Type = (ItemType)map[rawboxY, rawboxX] },
                    new Coordinate() { X = nextboxX, Y = nextboxY, Type = (ItemType)map[nextboxY, nextboxX] },
                    direction
                );

                manCoor.X = rawboxX;
                manCoor.Y = rawboxY;
                manCoor.Type = (ItemType)map[rawboxY, rawboxX];

                return history;
            }
        }

        #region 静态方法
        /// <summary>
        /// 获取指定字节值在二维数组中有相同值的数量
        /// </summary>
        public static int GetItemCount(byte[,] map, byte itemValue)
        {
            if (map == null)
            {
                throw new ArgumentException(Properties.Resources.NillOrEmpty);
            }

            int count = 0;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == itemValue)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// 检验地图元素数据是否包含特定ASCII字符
        /// </summary>
        /// <param name="mapdata">地图元素数据</param>
        public static bool CheckASCIICharacter(string mapdata)
        {
            //^[_\*-\.@\+\$#]+[\r?\n|(?<!\n)\r]?$
            if (Regex.IsMatch(mapdata, @"^[_]*#[-\.@\+\$\*#_]+([_]+|[#]+)$|^#[-\.@\+\$\*#_]+([_]+|[#]+)$", RegexOptions.Compiled))
            {
                return true;
            }

            return false;
        }

        public static (byte ManX, byte ManY) GetManPos(byte[,] map)
        {
            int man = GetItemCount(map, (byte)ItemType.Man);
            int mangoal = GetItemCount(map, (byte)ItemType.ManGoal);
            if (man == 1 && mangoal == 0 || man == 0 && mangoal == 1)
            {
                for (int y = 0; y < map.GetLength(0); y++)
                {
                    for (int x = 0; x < map.GetLength(1); x++)
                    {
                        if (map[y, x] == (byte)ItemType.Man || map[y, x] == (byte)ItemType.ManGoal)
                        {
                            return (Convert.ToByte(x), Convert.ToByte(y));
                        }
                    }
                }
            }

            throw new NotImplementedException(Properties.Resources.InvalidMapData);
        }

        public static char GetItemChar(ItemType item)
        {
            return item switch
            {
                ItemType.Floor => '-',
                ItemType.Goal => '.',
                ItemType.Man => '@',
                ItemType.ManGoal => '+',
                ItemType.Box => '$',
                ItemType.BoxGoal => '*',
                ItemType.Wall => '#',
                _ => '_',
            };
        }

        public static byte GetItemValue(string item)
        {
            return item switch
            {
                "-" => 0,
                "." => 1,
                "@" => 2,
                "+" => 3,
                "$" => 4,
                "*" => 5,
                "#" => 6,
                "_" => 7,
                _ => throw new ArgumentException("ASCII字符错误"),
            };
        }

        /// <summary>
        /// 将矩阵地图数据转换为包含特定ASCII字符的字符串形式表示
        /// </summary>
        public static string MapDataToString(byte[,] mapData)
        {
            if (mapData == null || mapData.GetLength(0) == 0 || mapData.GetLength(1) == 0)
            {
                throw new ArgumentException(Properties.Resources.NillOrEmpty);
            }

            StringBuilder builder = new();

            for (int y = 0; y < mapData.GetLength(0); y++) //行数
            {
                for (int x = 0; x < mapData.GetLength(1); x++) //列数
                {
                    if (x == mapData.GetLength(1) - 1)
                    {
                        if (y == mapData.GetLength(0) - 1)
                        {
                            builder.Append(GetItemChar((ItemType)mapData[y, x]));
                        }
                        else
                        {
                            builder.AppendLine(GetItemChar((ItemType)mapData[y, x]).ToString());
                        }
                    }
                    else
                    {
                        builder.Append(GetItemChar((ItemType)mapData[y, x]));
                    }
                }
            }

            return builder.ToString();
        }

        public static void ResetMap(byte[,] mapData, string[] rawMapStringArrays)
        {
            var group = rawMapStringArrays.GroupBy(p => p.Length);
            if (rawMapStringArrays.Length == mapData.GetLength(0) && group.Count() == 1 && group.First().Key == mapData.GetLength(1))
            {
                int _row = 0;
                foreach (string s in rawMapStringArrays)
                {
                    for (int j = 0; j < s.Length; j++)
                    {
                        mapData[_row, j] = GetItemValue(s[j].ToString());
                    }
                    _row++;
                }
            }
        }
        #endregion
    }
}
