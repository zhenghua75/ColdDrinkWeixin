using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColdDrinkWeixin.Common
{
    public class WeixinException : Exception
    {
        public int errcode { get; set; }
        public string errmsg { get; set; }
        public WeixinException(int errcode, string errmsg) : base("errcode:" + errcode + ",errmsg" + errmsg)
        {
            this.errcode = errcode;
            this.errmsg = errmsg;
        }
    }
}
