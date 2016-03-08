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
using uEN.UI.Controls;


namespace TableClassGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainView : BizView
    {
        /// <summary>デフォルトコンストラクタ</summary>
        public MainView()
        {
            InitializeComponent();
        }

        protected override void BuildBinding()
        {
            var builder = CreateBindingBuilder<MainViewModel>();
            builder.Element(MenuContent)
                   .Binding(ListContent.ItemsSourceProperty, x => x.ViewModels);
        }
    }
}
