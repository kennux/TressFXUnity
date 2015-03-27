using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TressFXUtils.Util
{
    public class ParseUtils
    {
        public static float ParseFloat(string str)
        {
            return float.Parse(str.Replace('.', ','));
        }
    }
}
