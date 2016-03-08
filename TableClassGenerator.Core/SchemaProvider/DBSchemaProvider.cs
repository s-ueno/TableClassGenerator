using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uEN.Core.Data;

namespace TableClassGenerator.Core
{
    public abstract class DBSchemaProvider
    {
        public virtual string Context { get; set; }
        protected DbConnectionHelper DbHelper { get { return DbConnectionRepository.CreateDbHelper(Context); } }
        protected DbConnection Connection { get { return DbHelper.DbConnection; } }
        protected DbDataAdapter CreateDataAdapter()
        {
            return DbHelper.CreateDataAdapter();
        }


        public virtual string Catalog
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_catalog))
                {
                    _catalog = Connection.Database;
                }
                return _catalog;
            }
        }
        private string _catalog;

        /// <summary>
        /// Item1 is Schema
        /// <para/>
        /// Item2 is TableName
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Tuple<string, string>> GetTableNames()
        {
            var list = new List<Tuple<string, string>>();
            var tables = Connection.GetSchema("Tables");
            foreach (var each in tables.Rows.OfType<DataRow>())
            {
                var catalog = each["TABLE_CATALOG"].GetValueOrDefault("");
                if (Catalog != catalog) continue;

                var schema = each["TABLE_SCHEMA"].GetValueOrDefault("");
                var name = each["TABLE_NAME"].GetValueOrDefault("");
                list.Add(Tuple.Create(schema, name));
            }
            return list.OrderBy(x => x.Item2);
        }
        public virtual string GetTableDescription(string schema, string tableName)
        {
            var tables = Connection.GetSchema("Tables", new string[] { null, null, tableName });
            if (tables.Columns.Contains("DESCRIPTION"))
            {
                return tables.Rows[0]["DESCRIPTION"].GetValueOrDefault("");
            }
            return string.Empty;
        }
        public virtual IEnumerable<ColumnDefinition> GetColumns(string schema, string tableName)
        {
            var index = Connection.GetSchema("Indexes", new string[] { Catalog, null, tableName });
            var cindex = Connection.GetSchema("IndexColumns", new string[] { Catalog, null, tableName });

            var list = new List<ColumnDefinition>();
            var columns = Connection.GetSchema("Columns", new string[] { Catalog, null, tableName });
            var dic = columns.Columns.OfType<DataColumn>().ToDictionary(x => x.ColumnName.ToUpper());
            Func<DataRow, string, string> getValue = (row, cname) => dic.ContainsKey(cname) ? row[cname].GetValueOrDefault("") : string.Empty;

            var id = 10;
            foreach (var each in columns.Rows.OfType<DataRow>())
            {
                var column = new ColumnDefinition();
                column.TableName = tableName;
                column.Column.Name = getValue(each, "COLUMN_NAME");
                column.Column.CanBeNull = getValue(each, "IS_NULLABLE") == "YES";
                column.Column.DbType = getValue(each, "DATA_TYPE");
                column.Column.IsPrimaryKey = ColumnIsPrimaryKey(schema, tableName, column.Column.Name);

                column.Column.Identity = id;
                id += 10;

                column.Description = getValue(each, "DESCRIPTION");
                column.MapType = ResolveMapType(column.Column.DbType);
                list.Add(column);
            }
            DialectColumn(schema, tableName, list);
            return list;
        }
        protected abstract Type ResolveMapType(string dbType);
        protected virtual void DialectColumn(string schema, string tableName, List<ColumnDefinition> list) { }
        protected virtual bool ColumnIsPrimaryKey(string schema, string tableName, string columnName)
        {
            if (pks.ContainsKey(tableName))
            {
                return pks[tableName].Any(x => x.ColumnName == columnName);
            }
            using (var cmd = CreateCommand())
            {
                var ds = new DataSet();
                cmd.CommandText = string.Format("select * from {0}.{1} where 0 = 1", schema, tableName);

                var adapter = CreateDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.FillSchema(ds, SchemaType.Source);

                var table = ds.Tables[0];
                pks[tableName] = table.PrimaryKey.ToArray();
                return table.PrimaryKey.Any(x => x.ColumnName == columnName);
            }
        }
        private static readonly ConcurrentDictionary<string, DataColumn[]> pks = new ConcurrentDictionary<string, DataColumn[]>();

        public virtual string Delimit(string input)
        {
            return string.Format(@"""""{0}""""", input);
        }

        protected virtual void AddDbParameter(DbCommand command, string inputValue, string parameterName)
        {
            var parameter = command.CreateParameter();
            parameter.DbType = DbType.String;
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = parameterName;
            parameter.Value = inputValue;
            command.Parameters.Add(parameter);
        }
        protected virtual DbCommand CreateCommand()
        {
            var cmd = Connection.CreateCommand();
            cmd.Transaction = DbHelper.Transaction;
            return cmd;
        }
        protected virtual void Fill(DataSet ds, DbCommand cmd)
        {
            var adapter = CreateDataAdapter();
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);
        }
    }
}
