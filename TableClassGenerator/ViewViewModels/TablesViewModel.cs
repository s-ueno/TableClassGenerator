using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TableClassGenerator.Core;
using uEN;
using uEN.Core;
using uEN.Core.Data;
using uEN.UI;
using uEN.UI.AttachedProperties;
using uEN.UI.Validation;

namespace TableClassGenerator
{
    /// <summary>
    /// 
    /// </summary>
    [TextInputObserver]
    [VisualElements(typeof(TablesView))]
    public class TablesViewModel : BizViewModel
    {
        public override string Description { get { return "Generate Entity Class"; } }
        public TablesViewModel(MainViewModel mainViewModel)
        {
            this.Parent = mainViewModel;
        }
        public MainViewModel Parent { get; private set; }


        public override async void Initialize()
        {
            this.IsEnabled = false;
            IEnumerable<TableInfomation> infoList;
            try
            {
                infoList = await Task.Run(() => ListTables());
            }
            finally
            {
                this.IsEnabled = true;
            }

            TableList = CreateSimpleGrid(infoList);
            TableList.GridSource.Filter = PredicateRows;

            var schema = TableList.GridSource.OfType<TableInfomation>().Select(x => x.Schema).Distinct();
            SchemaList = CreateListCollectionView(schema, true);
            SchemaList.CurrentChanged += (sender, e) =>
            {
                RefreshGrid();
            };
            UpdateTarget();
        }
        private bool PredicateRows(object obj)
        {
            var row = obj as TableInfomation;
            if (row == null) return true;

            if (SchemaList == null) return true;
            var schema = SchemaList.CurrentItem as string;


            if (!string.IsNullOrWhiteSpace(schema))
            {
                if (row.Schema != schema) return false;
            }

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                if (!row.Table.ToLower().Contains(Filter.ToLower())) return false;
            }

            return true;
        }
        public ISimpleGrid TableList { get; set; }
        public ListCollectionView SchemaList { get; set; }
        IEnumerable<TableInfomation> ListTables()
        {
            var list = new List<TableInfomation>();
            var tables = GenerateHelper.ListTableDefinition();
            var idx = 0;
            foreach (var table in tables)
            {
                var newItem = new TableInfomation(table)
                {
                    Identity = ++idx,
                    Schema = table.Schema,
                    Table = table.Table.Name,
                    Description = table.Description,
                };
                list.Add(newItem);
            }
            return list;
        }

        [RequiredAnnotation("Namespace is Required")]
        [TextInputPolicy(TextInputState.Disable)]
        public string Namespace
        {
            get { return this.GetBackingStore("Namespace") as string; }
            set { this.SetBackingStore(value, "Namespace"); }
        }

        public void AllSelect()
        {
            foreach (var each in TableList.GridSource.OfType<TableInfomation>())
            {
                each.Allow = true;
            }
        }

        public void ClearSelection()
        {
            foreach (var each in TableList.GridSource.OfType<TableInfomation>())
            {
                each.Allow = false;
            }
        }

        public void Save()
        {
            //Validation Annotation
            ThrowValidationError();

            //CreationPolicy.Shared
            var sharedGenerator = Repository.GetPriorityExport<CodeGenerator>() as CustomCodeGenerator;
            sharedGenerator.AllowDapperAggregator = AllowDapperAggregator;
            sharedGenerator.GenerateNamespace = Namespace;

            //The subject is not selected
            var targets = TableList.GridSource
                          .OfType<TableInfomation>()
                          .Where(x => x.Allow)
                          .ToArray();
            if (targets.Length == 0)
            {
                throw new BizApplicationException(
                    Properties.Resources.WarningSelectTarget +
                    Environment.NewLine +
                    Properties.Resources.InfomationCheckMark);
            }

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Title = Properties.Resources.MessageSaveClass;
            dialog.Filter = "*.cs|*.cs";
            dialog.FileName = "AutoGenerateModels.cs";

            if (!string.IsNullOrWhiteSpace(FullPath))
            {
                var dir = System.IO.Path.GetDirectoryName(FullPath);
                dialog.InitialDirectory = dir;
            }
            if (dialog.ShowDialog() != true) return;

            FullPath = dialog.FileName;

            var sb = new StringBuilder();
            foreach (var each in targets)
            {
                this.StatusMessage = string.Format("処理中...{0}", each.Table);
                sb.AppendLine(GenerateHelper.GenerateCode(each.TableDefinition));
            }
            System.IO.File.WriteAllText(FullPath, sb.ToString());

            ClearStatusMessage();
            this.ShowOk(Properties.Resources.CompleteMessage, "", () =>
            {
                System.Diagnostics.Process.Start(System.IO.Path.GetDirectoryName(FullPath));
            });
        }
        protected string FullPath
        {
            get { return this.GetBackingStore("FullPath") as string; }
            set { this.SetBackingStore(value, "FullPath"); }
        }

        [TextInputPolicy(TextInputState.Off)]
        public string Filter
        {
            get { return _filter; }
            set { SetProperty(ref _filter, value, "Filter"); }
        }
        string _filter;
        public void ClearFilter()
        {
            Filter = string.Empty;
            RefreshGrid();
        }

        public void Find()
        {
            if (string.IsNullOrWhiteSpace(Filter)) return;
            RefreshGrid();
        }

        void RefreshGrid()
        {
            if (TableList != null)
            {
                TableList.GridSource.Refresh();
            }
        }

        public bool AllowDapperAggregator
        {
            get { return this.GetBackingStore() as bool? ?? true; }
            set { this.SetBackingStore(value); }
        }
        public void AllowDapperAggregatorChanged()
        {
            UpdateSource();
        }
    }
}
