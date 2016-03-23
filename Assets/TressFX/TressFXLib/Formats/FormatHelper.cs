using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TressFXLib.Formats
{
    /// <summary>
    /// This class contains some helper functions for data import / export.
    /// </summary>
    public static class FormatHelper
    {
        public static List<T> Shuffle<T>(List<T> list)
        {
            var rnd = new Random();
            return list.OrderBy(item => rnd.Next()).ToList();
        }
    }
}
