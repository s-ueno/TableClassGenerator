using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uEN;

namespace TableClassGenerator.Core
{
    public class CodeBuilder
    {
        StringBuilder localCache = new StringBuilder();
        public CodeBuilder()
        {
            Generator = Repository.GetPriorityExport<CodeGenerator>();
        }
        public void SetIndent(int indent)
        {
            Indent = indent;
        }
        protected int Indent { get; private set; }
        protected bool NeedIndent { get { return 0 < Indent; } }
        protected CodeGenerator Generator { get; private set; }
        protected void WriteIndent()
        {
            if (!NeedIndent) return;
            for (int i = 0; i < (Indent * 4); i++)
            {
                localCache.Append(" ");
            }
        }
        public void Append(string s)
        {
            WriteIndent();
            localCache.Append(s);
        }
        public void AppendLine(string s)
        {
            WriteIndent();
            localCache.AppendLine(s);
        }
        public void AppendGenerate(Func<CodeGenerator, string> func)
        {
            var ret = func(Generator);
            if (!string.IsNullOrWhiteSpace(ret)) AppendLine(ret);
        }
        public void AppendGenerate(Func<CodeGenerator, IEnumerable<string>> func)
        {
            var lines = func(Generator);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    AppendLine(line);
            }
        }
        public string Peek(Func<CodeGenerator, string> func)
        {
            return func(Generator);
        }
        public void AppendSummaryComment(string comment)
        {
            AppendLine("/// <summary>");
            var lines = comment.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                AppendLine("/// " + line);
            }
            AppendLine("/// </summary>");
        }
        public void AppendSummaryComment(Func<CodeGenerator, string> func)
        {
            AppendLine("/// <summary>");

            var ret = func(Generator);
            if (!string.IsNullOrWhiteSpace(ret))
            {
                var lines = ret.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    AppendLine("/// " + line);
                }
            }

            AppendLine("/// </summary>");
        }
        public void AppentBaseClassOrInterface(TableDefinition tableDefinition)
        {
            var className = tableDefinition.Table.Name.Replace(" ", "_");
            var c = Generator.ClassName(tableDefinition);
            if (!string.IsNullOrWhiteSpace(c))
            {
                className = c;
            }
            GeneratedClassName = className;


            var list = Generator.BaseClassOrInterface().ToList();
            if (list.Any())
            {
                var types = list.Distinct();
                var rootClass = types.Where(x => x.IsClass || (x.IsAbstract && !x.IsInterface) || x.IsValueType).ToArray();
                if (1 < rootClass.Length)
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format("can not specify more than one class in the basement. \n{0}",
                        string.Join("\n", rootClass.Select(x => x.FullName))));
                }

                var sortedTypes = new List<Type>();
                if (rootClass.Any())
                {
                    sortedTypes.Add(rootClass.First());
                }
                foreach (var each in types.Except(rootClass))
                {
                    sortedTypes.Add(each);
                }

                var buff = string.Join(",", sortedTypes.Select(x => x.FullName));
                AppendLine(string.Format("public partial class {0} : {1}", className, buff));
            }
            else
            {
                AppendLine(string.Format("public partial class {0}", className));
            }
            AppendLine("{");
        }
        public string GeneratedClassName
        {
            get
            {
                return _generatedClassName.SafeString();
            }
            private set
            {
                _generatedClassName = value;
            }
        }
        string _generatedClassName;

        protected virtual string PropertyDefinineClassName { get { return "Properties"; } }
        protected virtual string ColumnDefinineClassName { get { return "Columns"; } }
        public void AppentColumnProperty(ColumnDefinition columnDefinition)
        {
            var name = columnDefinition.Column.Name;
            var buff = Generator.ColumnPropertyName(columnDefinition);
            if (!string.IsNullOrWhiteSpace(buff))
            {
                name = buff;
            }
            columnDic[columnDefinition] = name;


            var columnTypeName = columnDefinition.MapType.FullName;
            if (columnDefinition.MapType.IsValueType && columnDefinition.Column.CanBeNull)
            {
                columnTypeName = columnTypeName + "?";
            }

            AppendLine(string.Format("public {0} {1}", columnTypeName, name));
            AppendLine("{");
            SetIndent(3);
            AppendLine(string.Format(@"get {{ return this.GetValue<{0}>({1}.{2}); }}", columnTypeName, PropertyDefinineClassName, name));
            AppendLine(string.Format(@"set {{ this.SetValue<{0}>({1}.{2}, value); }}", columnTypeName, PropertyDefinineClassName, name));
            SetIndent(2);
            AppendLine("}");
        }
        public Dictionary<ColumnDefinition, string> GeneratedColumns
        {
            get
            {
                var dic = new Dictionary<ColumnDefinition, string>();
                foreach (var each in columnDic)
                {
                    dic[each.Key] = each.Value;
                }
                return dic;
            }
        }
        private ConcurrentDictionary<ColumnDefinition, string> columnDic = new ConcurrentDictionary<ColumnDefinition, string>();
        public void AppentColumnsClass(TableDefinition tableDefinition)
        {
            //properties
            AppendLine(string.Format(@"public static class {0}", PropertyDefinineClassName));
            AppendLine(@"{");
            SetIndent(3);

            var properties = new List<string>();
            foreach (var each in tableDefinition.Columuns)
            {
                var symbol = columnDic[each];

                AppendSummaryComment(Generator.ColumnComment(each));
                AppendLine(string.Format(@"public const string {0} = ""{0}"";", symbol));
                properties.Add(symbol);
            }
            if (!properties.Contains("Values"))
            {
                AppendLine(string.Format(@"public static readonly string[] Values = new string[] {{{0}}};", string.Join(",", properties)));
            }
            SetIndent(2);
            AppendLine(@"}");



            //columns
            AppendLine(string.Format(@"public static class {0}", ColumnDefinineClassName));
            AppendLine(@"{");
            SetIndent(3);

            var columns = new List<string>();
            foreach (var each in tableDefinition.Columuns)
            {
                var symbol = columnDic[each];

                AppendSummaryComment(Generator.ColumnComment(each));
                AppendLine(string.Format(@"public const string {0} = @""{1}"";", symbol, Generator.Delimit(each.Column.Name)));

                columns.Add(string.Format(@"@""{0}""", Generator.Delimit(each.Column.Name)));
            }
            if (!columns.Contains("Values"))
            {
                AppendLine(string.Format(@"public static readonly string[] Values = new string[] {{{0}}};", string.Join(",", columns)));
            }
            SetIndent(2);
            AppendLine(@"}");
        }


        public void ExtendedAction()
        {
            Generator.ExtendedGenerate(this);
        }
        public override string ToString()
        {
            return localCache.ToString();
        }
    }
}
