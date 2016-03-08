using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using uEN;
using uEN.Core;
using uEN.UI;
using uEN.UI.AttachedProperties;


namespace TableClassGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TablesView : BizView
    {
        /// <summary>デフォルトコンストラクタ</summary>
        public TablesView()
        {
            InitializeComponent();
        }

        protected override void BuildBinding()
        {
            var builder = CreateBindingBuilder<TablesViewModel>();

            builder.Element(WaitProgress)
                   .Binding(UIElement.VisibilityProperty, x => x.IsEnabled)
                   .Convert(UIUtils.ReverseBoolToVisibility);

            builder.Element(SchemasComboBox)
                   .Binding(ItemsControl.ItemsSourceProperty, x => x.SchemaList);

            builder.Element(NamespaceTextBox)
                   .Binding(TextBox.TextProperty, x => x.Namespace);

            builder.Element(AllSelectButton)
                   .Binding(Button.ClickEvent, x => x.AllSelect);

            builder.Element(ClearSelectButton)
                   .Binding(Button.ClickEvent, x => x.ClearSelection);

            builder.Element(SaveButton)
                   .Binding(Button.ClickEvent, x => x.Save);

            builder.Element(GridContent)
                   .Binding(ContentPresenter.ContentProperty, x => x.TableList);

            builder.Element(FilterTextBox)
                   .Binding(TextBox.TextProperty, x => x.Filter)
                   .Binding(SymbolTextBoxProxy.ClickEvent, x => x.ClearFilter)
                   .Binding(SymbolTextBoxProxy.EnterEvent, x => x.Find);
            builder.Element(FindButton)
                   .Binding(Button.ClickEvent, x => x.Find);

            builder.Element(AllowDapperAggregatorCheckBox)
                   .Binding(CheckBox.IsCheckedProperty, x => x.AllowDapperAggregator)
                   .Binding(CheckBox.CheckedEvent, x => x.AllowDapperAggregatorChanged)
                   .Binding(CheckBox.UncheckedEvent, x => x.AllowDapperAggregatorChanged);

        }
    }
}
