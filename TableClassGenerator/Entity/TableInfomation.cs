using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableClassGenerator.Core;

namespace TableClassGenerator
{

    public class TableInfomation : uEN.Core.NotifiableImp
    {

        public TableInfomation(TableDefinition table)
        {
            this.TableDefinition = table;
        }
        public TableDefinition TableDefinition { get; set; }

        [uEN.Core.DataGridColumnAnnotation(1, "✅", 40, IsReadOnly = false)]
        public bool Allow
        {
            get { return _allow; }
            set { SetProperty(ref  _allow, value, "Allow"); }
        }
        private bool _allow;

        [uEN.Core.DataGridColumnAnnotation(10, "No", 40)]
        public int Identity { get; set; }
        [uEN.Core.DataGridColumnAnnotation(20, "Schema", 150)]
        public string Schema { get; set; }
        [uEN.Core.DataGridColumnAnnotation(30, "Table Name", 200)]
        public string Table { get; set; }
        [uEN.Core.DataGridColumnAnnotation(40, "Description", 0, IsStar = true)]
        public string Description { get; set; }
    }
}
