using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableClassGenerator.Core
{
    public class TableDefinition
    {
        public string Catalog { get; set; }
        public string Schema { get; set; }
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
        public TableAttribute Table { get { return tableAtt; } }
        private TableAttribute tableAtt = new TableAttribute();

        public List<ColumnDefinition> Columuns { get { return _columuns; } }
        private List<ColumnDefinition> _columuns = new List<ColumnDefinition>();
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TableAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
