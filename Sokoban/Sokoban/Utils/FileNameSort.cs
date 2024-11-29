using System.Collections;
using System.Collections.Generic;

namespace Sokoban
{
    //internal sealed class FileNameSort : IComparer
    //{
    //    //前后文件名进行比较。
    //    public int Compare(object name1, object name2)
    //    {
    //        if (null == name1 && null == name2)
    //        {
    //            return 0;
    //        }
    //        if (null == name1)
    //        {
    //            return -1;
    //        }
    //        if (null == name2)
    //        {
    //            return 1;
    //        }

    //        return Utils.ExtendMethod.StrCmpLogicalW(name1.ToString(), name2.ToString());
    //    }
    //}

    internal sealed class FileNameSort : IComparer<System.IO.FileInfo>
    {
        //前后文件名进行比较。
        public int Compare(System.IO.FileInfo name1, System.IO.FileInfo name2)
        {
            if (null == name1 && null == name2)
            {
                return 0;
            }
            if (null == name1)
            {
                return -1;
            }
            if (null == name2)
            {
                return 1;
            }

            return Utils.ExtendMethod.StrCmpLogicalW(name1.ToString(), name2.ToString());
        }
    }
}
