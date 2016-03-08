using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using uEN.Core;
using uEN.UI;
using uEN.UI.AttachedProperties;
using uEN.UI.Validation;

namespace TableClassGenerator
{
    /// <summary>
    /// 
    /// </summary>
    [VisualElements(typeof(MainView))]
    public class MainViewModel : BizViewModel
    {
        public ListCollectionView ViewModels { get; set; }
        public override void Initialize()
        {
            var list = new List<BizViewModel>();

            list.Add(new TablesViewModel(this));

            ViewModels = CreateListCollectionView(list);
        }
    }
}
