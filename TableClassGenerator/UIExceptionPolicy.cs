using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using uEN;
using uEN.UI;
using uEN.UI.DataBinding;

namespace TableClassGenerator
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata(Repository.Priority, 0)]
    [Export(typeof(IExceptionPolicy))]
    public class UIExceptionPolicy : IExceptionPolicy
    {
        IInputElement keyboardFocusedElement;
        public void Do(object sender, Exception ex)
        {
            var appException = ex as BizApplicationException;
            if (appException != null)
            {
                BizUtils.TraceInformation(ex.Message);

                var uiBehavior = sender as RoutedEventBehavior;
                if (uiBehavior != null)
                {
                    keyboardFocusedElement = Keyboard.FocusedElement;
                    var action = new Action(() =>
                    {
                        if (keyboardFocusedElement != null)
                        {
                            try
                            {
                                keyboardFocusedElement.Focus();
                            }
                            catch
                            {
                                //error free
                            }
                            keyboardFocusedElement = null;
                        }
                    });

                    var vm = uiBehavior.ViewModel as BizViewModel;
                    vm.ShowOk(App.AppTitle, ex.Message, action);
                    return;
                }
                else
                {
                    MessageBox.Show(ex.Message, App.AppTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            BizUtils.TraceError(ex);
            MessageBox.Show(ex.Message, App.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
