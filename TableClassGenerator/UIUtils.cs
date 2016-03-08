using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TableClassGenerator
{
    public static class UIUtils
    {
        public static object BoolToVisibility(object arg1, Type arg2, object arg3, System.Globalization.CultureInfo arg4)
        {
            var ret = arg1 as bool?;
            return ret.HasValue && ret.Value == true ? Visibility.Visible : Visibility.Hidden;
        }
        public static object ReverseBoolToVisibility(object arg1, Type arg2, object arg3, System.Globalization.CultureInfo arg4)
        {
            var ret = arg1 as bool?;
            return ret.HasValue && ret.Value == true ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
