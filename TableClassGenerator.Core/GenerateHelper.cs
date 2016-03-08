using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uEN;
using uEN.Core.Data;

namespace TableClassGenerator.Core
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportMetadata(Repository.Priority, int.MaxValue)]
    [Export(typeof(GenerateHelper))]
    public class GenerateHelper : uEN.Core.BizService
    {
        public static DBSchemaProvider CreateSchemaProvider()
        {
            return CreateSchemaProvider(DbConnectionRepository.DefaultContext);
        }
        public static DBSchemaProvider CreateSchemaProvider(string dbContext)
        {
            var providerName = DbConnectionRepository.GetProviderName(dbContext);
            var provider = Repository.GetContextExport<DBSchemaProvider>(x => x == providerName);
            if (provider == null)
            {
                throw new InvalidOperationException("Not compatible DBSchemaProvider. make sure connectionStrings providerName");
            }
            provider.Context = dbContext;
            return provider;
        }

        #region ListTableDefinition

        public static IEnumerable<TableDefinition> ListTableDefinition()
        {
            return ListTableDefinition(DbConnectionRepository.DefaultContext);
        }
        public static IEnumerable<TableDefinition> ListTableDefinition(string dbContext)
        {
            var generator = Repository.GetPriorityExport<GenerateHelper>();
            return generator.List(dbContext);
        }
        protected virtual IEnumerable<TableDefinition> List()
        {
            return List(DbConnectionRepository.DefaultContext);
        }
        protected virtual IEnumerable<TableDefinition> List(string dbContext)
        {
            var provider = GenerateHelper.CreateSchemaProvider(dbContext);
            var list = new List<TableDefinition>();
            foreach (var tuple in provider.GetTableNames())
            {
                var newItem = new TableDefinition()
                {
                    Catalog = provider.Catalog,
                    Schema = tuple.Item1,
                    Description = provider.GetTableDescription(tuple.Item1, tuple.Item2),
                };
                newItem.Table.Name = tuple.Item2;
                newItem.Columuns.AddRange(provider.GetColumns(tuple.Item1, tuple.Item2));
                list.Add(newItem);
            }
            return list.OrderBy(x => x.Schema).ThenBy(x => x.Table.Name);
        }

        #endregion

        #region GenerateCode

        public static string GenerateCode(TableDefinition table)
        {
            return GenerateCode(table, DbConnectionRepository.DefaultContext);
        }
        public static string GenerateCode(TableDefinition table, string context)
        {
            var generator = Repository.GetPriorityExport<GenerateHelper>();
            return generator.Generate(table, context);
        }

        protected string Generate(TableDefinition tableDefinition, string context)
        {
            var builder = new CodeBuilder();

            builder.AppendGenerate(x => x.PrecautionaryStatement());
            var buff = builder.Peek(x => x.Namespace()).Replace(Environment.NewLine, string.Empty);
            if (string.IsNullOrWhiteSpace(buff))
            {
                buff = "SampleProject";
            }
            builder.AppendLine(string.Format("namespace {0}", buff));
            builder.AppendLine("{");
            builder.SetIndent(1);

            builder.AppendGenerate(x => x.Using());

            builder.AppendSummaryComment(x => x.ClassComment(tableDefinition));
            builder.AppendGenerate(x => x.ClassAttributes(tableDefinition));
            builder.AppentBaseClassOrInterface(tableDefinition);
            builder.SetIndent(2);
            foreach (var columun in tableDefinition.Columuns)
            {
                builder.AppendSummaryComment(x => x.ColumnComment(columun));
                builder.AppendGenerate(x => x.ColumnAttributes(columun));
                builder.AppentColumnProperty(columun);
            }
            builder.AppentColumnsClass(tableDefinition);
            builder.ExtendedAction();
            builder.SetIndent(1);
            builder.AppendLine("}");
            builder.SetIndent(0);
            builder.AppendLine("}");
            return builder.ToString();
        }

        #endregion
    }
}
