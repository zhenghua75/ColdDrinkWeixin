using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColdDrinkWeixin.Common
{
    public class Button
    {
        public string type { get; set; }
        public string name { get; set; }
        public string key { get; set; }
    }
    public class WeixinMenu
    {
        public List<Button> button { get; set; }
    }
}
