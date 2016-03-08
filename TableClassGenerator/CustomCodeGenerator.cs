using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableClassGenerator.Core;
using uEN;

namespace TableClassGenerator
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(CodeGenerator))]
    [ExportMetadata(Repository.Priority, 1)]
    public class CustomCodeGenerator : DefaultCodeGenerator
    {
        public bool AllowDapperAggregator { get; set; }
        public string GenerateNamespace { get; set; }
        protected override DBSchemaProvider Provider
        {
            get
            {
                return _provider;
            }
        }
        DBSchemaProvider _provider = GenerateHelper.CreateSchemaProvider();
        public override string Namespace()
        {
            return string.IsNullOrWhiteSpace(GenerateNamespace) ? base.Namespace() : GenerateNamespace;
        }
        public override IEnumerable<string> Using()
        {
            foreach (var each in base.Using())
            {
                yield return each;
            }

            if (AllowDapperAggregator)
                yield return "using Dapper.Aggregator;";
        }
        public override IEnumerable<Type> BaseClassOrInterface()
        {
            foreach (var each in base.BaseClassOrInterface())
            {
                yield return each;
            }

            if (AllowDapperAggregator)
                yield return typeof(Dapper.Aggregator.IContainerHolder);
        }
        public override IEnumerable<string> ClassAttributes(TableDefinition definition)
        {
            foreach (var each in base.ClassAttributes(definition))
            {
                yield return each;
            }

            if (AllowDapperAggregator)
                yield return string.Format(@"[Dapper.Aggregator.Table(@""{0}.{1}"", Description = @""{2}""), {3}]",
                                        this.Delimit(definition.Schema),
                                        this.Delimit(definition.Table.Name),
                                        definition.Description,
                                        typeof(Dapper.Aggregator.NumericVersionPolicyAttribute).Name.Replace("Attribute", ""));
        }
        public override IEnumerable<string> ColumnAttributes(ColumnDefinition definition)
        {
            foreach (var each in base.ColumnAttributes(definition))
            {
                yield return each;
            }

            if (AllowDapperAggregator)
                yield return string.Format(@"[Dapper.Aggregator.Column(Name = @""{0}"", DbType = ""{1}"", IsPrimaryKey = {2}, CanBeNull = {3}, IsVersion = {4}, Description = @""{5}"")]",
                                        this.Delimit(definition.Column.Name),
                                        definition.Column.DbType,
                                        definition.Column.IsPrimaryKey.ToString().ToLower(),
                                        definition.Column.CanBeNull.ToString().ToLower(),
                                        (string.Compare(definition.Column.Name, "Lockversion", true) == 0).ToString().ToLower(),
                                        definition.Description);
        }

        public override void ExtendedGenerate(CodeBuilder builder)
        {
            base.ExtendedGenerate(builder);
            if (AllowDapperAggregator)
            {
                builder.AppendLine(@"[Dapper.Aggregator.Column(Ignore = true)]");
                builder.AppendLine(@"public DataContainer Container { get; set; }");
            }


        }
    }
}
