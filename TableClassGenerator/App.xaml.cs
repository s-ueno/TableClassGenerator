using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using uEN;
using uEN.UI;

namespace TableClassGenerator
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : BizApplication
    {
        public static readonly string AppTitle = BizUtils.AppSettings("CompanyName", "アプリ");
        protected override void Initialize(Window mainWindow, BizViewModel mainViewModel)
        {
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mainWindow.Width = 850;
            mainWindow.Height = 700;
            MergeResources();
        }

        protected void MergeResources()
        {
            var items = CreateResourcs();
            foreach (var each in CreateResourcs())
            {
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(each) });
            }
        }
        protected IEnumerable<string> CreateResourcs()
        {
            yield return @"pack://application:,,,/TableClassGenerator;component/Generic.xaml";
        }

    }
}
