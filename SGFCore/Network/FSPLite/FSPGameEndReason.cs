using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGF.Network.FSPLite
{
    //由于以上有多种可能发GameEnd的情况，所以这里有一个GameEnd的原因定义
    public enum FSPGameEndReason
    {
        Normal = 0, //正常结束
        AllOtherExit = 1, //所有其他人都主动退出了
        AllOtherLost = 2,  //所有其他人都掉线了
    }
}
