﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using uEN;

namespace TableClassGenerator.Core
{
    public abstract class CodeGenerator
    {
        protected virtual DBSchemaProvider Provider { get { return null; } }
        public virtual string PrecautionaryStatement() { return string.Empty; }
        public virtual IEnumerable<string> Using() { yield break; }
        public abstract string Namespace();
        public virtual string ClassComment(TableDefinition definition) { return string.Empty; }
        public virtual IEnumerable<string> ClassAttributes(TableDefinition definition) { yield break; }
        public virtual string ClassName(TableDefinition definition) { return string.Empty; }
        public virtual IEnumerable<Type> BaseClassOrInterface() { yield break; }
        public virtual string ColumnComment(ColumnDefinition definition) { return string.Empty; }
        public virtual IEnumerable<string> ColumnAttributes(ColumnDefinition definition) { yield break; }
        public virtual string ColumnPropertyName(ColumnDefinition definition) { return string.Empty; }
        public virtual void ExtendedGenerate(CodeBuilder builder) { }
        protected string ToPascal(string s)
        {
            var split = s.Split(new string[] { " ", "_", "-" }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var each in split)
            {
                sb.Append(new string(each.Take(1).ToArray()).ToUpper() + new string(each.Skip(1).ToArray()));
            }
            return sb.ToString().SafeString();
        }
        public string Delimit(string input)
        {
            if (Provider == null) return input;
            return Provider.Delimit(input);
        }
    }

    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(CodeGenerator))]
    [ExportMetadata(Repository.Priority, int.MaxValue)]
    public class DefaultCodeGenerator : CodeGenerator
    {
        public override string PrecautionaryStatement()
        {
            return @"//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated by a tool.
//
//    Changes to this file may cause incorrect behavior and will be lost if
//    the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------";
        }

        public override IEnumerable<string> Using()
        {
            yield return @"using System;";
            yield return @"using System.Collections.Generic;";
            yield return @"using System.Linq;";
            yield return @"using System.Text;";
        }
        public override string Namespace() { return string.Empty; }

        public override string ClassComment(TableDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition.Description))
            {
                return definition.Description;
            }
            return string.Format("{0} entity class", definition.Table.Name);
        }
        public override IEnumerable<string> ClassAttributes(TableDefinition definition)
        {
            yield return string.Format(@"[System.Serializable]");
        }
        public override string ClassName(TableDefinition definition)
        {
            return ToPascal(definition.Table.Name);
        }
        public override string ColumnComment(ColumnDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition.Description))
            {
                return string.Format(@"{0}<para/>{1}{2}", definition.Description, Environment.NewLine, definition.Column.DbType);
            }
            return string.Format(@"{0}:{1}", definition.Column.Name, definition.Column.DbType);
        }
        public override string ColumnPropertyName(ColumnDefinition definition)
        {
            var propertyName = ToPascal(definition.Column.Name);
            var className = ToPascal(definition.TableName);
            if (propertyName != className)
            {
                return propertyName;
            }
            return propertyName + ColumnSuffix;
        }
        protected virtual string ColumnSuffix { get { return "Column"; } }
        public override IEnumerable<Type> BaseClassOrInterface()
        {
            yield return typeof(INotifyPropertyChanged);
        }

        public override void ExtendedGenerate(CodeBuilder builder)
        {
            builder.AppendLine(@"public T GetValue<T>(string key, T defaultValue = default(T))");
            builder.AppendLine(@"{");
            builder.SetIndent(3);
            builder.AppendLine(@"return _dic.ContainsKey(key) ? (T)_dic[key] : defaultValue;");
            builder.SetIndent(2);
            builder.AppendLine(@"}");
            builder.AppendLine(@"public void SetValue<T>(string key, T value)");
            builder.AppendLine(@"{");
            builder.SetIndent(3);
            builder.AppendLine(@"var isSame = EqualityComparer<T>.Default.Equals(GetValue<T>(key), value);");
            builder.AppendLine(@"if (!isSame)");
            builder.AppendLine(@"{");
            builder.SetIndent(4);
            builder.AppendLine(@"_isDuty = true;");
            builder.AppendLine(@"_dic[key] = value;");
            builder.AppendLine(@"OnPropertyChanged(key);");
            builder.SetIndent(3);
            builder.AppendLine(@"}");
            builder.SetIndent(2);
            builder.AppendLine(@"}");
            builder.AppendLine(@"private bool _isDuty = false;");
            builder.AppendLine(@"private Dictionary<string, object> _dic = new Dictionary<string, object>();");
            builder.AppendLine(@"[field: NonSerialized]");
            builder.AppendLine(@"public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;");
            builder.AppendLine(@"public void OnPropertyChanged(string propertyName = null)");
            builder.AppendLine(@"{");
            builder.SetIndent(3);
            builder.AppendLine(@"if (PropertyChanged != null)");
            builder.SetIndent(4);
            builder.AppendLine(@"PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));");
            builder.SetIndent(2);
            builder.AppendLine(@"}");
            builder.AppendSummaryComment("Gets a value that indicates whether or not there is a change.");
            builder.AppendLine(@"public bool HasChanges() { return _isDuty; }");
            builder.AppendSummaryComment("Commit the changes. ");
            builder.AppendLine(@"public void AcceptChanges () { _isDuty = false; }");
            builder.AppendSummaryComment("To create a duplicate. ");
            builder.AppendLine(string.Format(@"public virtual {0} Copy()", builder.GeneratedClassName));
            builder.AppendLine(@"{");
            builder.SetIndent(3);
            builder.AppendLine(string.Format(@"var newRow = new {0}()", builder.GeneratedClassName));
            builder.AppendLine(@"{");
            builder.SetIndent(4);
            foreach (var each in builder.GeneratedColumns.OrderBy(x => x.Key.Column.Identity))
            {
                builder.AppendLine(string.Format(@"{0} = this.{0},", each.Value));
            }
            builder.SetIndent(3);
            builder.AppendLine(@"};");
            builder.AppendLine(@"newRow.AcceptChanges();");
            builder.AppendLine(@"return newRow;");

            builder.SetIndent(2);
            builder.AppendLine(@"}");

            var pks = builder.GeneratedColumns.Keys.Where(x => x.Column.IsPrimaryKey).ToArray();
            if (pks.Any())
            {
                builder.AppendSummaryComment("Compares entity with the same primary key.");
                builder.AppendLine(string.Format(@"public bool HasSamePk({0} other)", builder.GeneratedClassName));
                builder.AppendLine(@"{");
                builder.SetIndent(3);
                builder.AppendLine(@"if (other == null) return false;");
                builder.AppendLine(@"if (object.ReferenceEquals(this, other)) return true;");

                var list = new List<string>();
                var dic = builder.GeneratedColumns;
                foreach (var pk in pks)
                {
                    var symbol = dic[pk];
                    list.Add(string.Format("this.{0} == other.{0}", symbol));
                }
                builder.AppendLine(string.Format(@"return {0} ;", string.Join(" && ", list)));
                builder.SetIndent(2);
                builder.AppendLine(@"}");
            }


            builder.AppendSummaryComment("Set argments to myself.");
            builder.AppendLine(string.Format(@"public void Merge({0} source)", builder.GeneratedClassName));
            builder.AppendLine(@"{");
            builder.SetIndent(3);
            foreach (var each in builder.GeneratedColumns.OrderBy(x => x.Key.Column.Identity))
            {
                builder.AppendLine(string.Format(@"this.{0} = source.{0};", each.Value));
            }
            builder.SetIndent(2);
            builder.AppendLine(@"}");
        }
    }
}
