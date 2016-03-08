using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableClassGenerator.Core;
using uEN;
using uEN.Core.Data;
using Dapper;
using Dapper.Aggregator;
using System.ComponentModel.Composition;
namespace TableClassGenerator
{
    public static class StartUp
    {
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static void Main()
        {
            var app = new App();
            app.Start(new MainViewModel());
        }
    }
}
