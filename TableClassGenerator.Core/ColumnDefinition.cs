using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableClassGenerator.Core
{
    public class ColumnDefinition
    {
        public string TableName { get; set; }
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                if (_description.Contains(Environment.NewLine))
                    _description = _description.Replace(Environment.NewLine, " ");
            }
        }
        string _description;
        public Type MapType { get; set; }
        public ColumnAttribute Column { get { return _column; } }
        private ColumnAttribute _column = new ColumnAttribute();
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ColumnAttribute : Attribute
    {
        public bool CanBeNull { get; set; }
        public string DbType { get; set; }
        public string Expression { get; set; }
        public bool IsDbGenerated { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsVersion { get; set; }
        public string Name { get; set; }
        public int Identity { get; set; }
    }
}
