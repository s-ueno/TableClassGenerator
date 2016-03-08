using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using uEN.Core;
using uEN.Core.Data;

namespace TableClassGenerator.Core
{
    public static class Utils
    {
        internal static T GetValueOrDefault<T>(this object obj, T defaultValue)
        {
            var result = defaultValue;
            try
            {
                var value = Convert.ToString(obj);
                result = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value);
            }
            catch
            {
                //error free
            }
            return result;
        }

        private readonly static Regex reg = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]", RegexOptions.Compiled);
        internal static string SafeString(this string s)
        {
            var value = s;
            var provider = Microsoft.CSharp.CSharpCodeProvider.CreateProvider("C#");
            bool isValid = provider.IsValidIdentifier(value);

            if (!isValid)
            {
                value = reg.Replace(value, "");
            }
            if (!char.IsLetter(value, 0))
            {
                value = "_" + value;
            }
            return value;
        }
    }


}
