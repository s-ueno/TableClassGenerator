using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uEN;

namespace TableClassGenerator.Core
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportMetadata(Repository.Context, "System.Data.Npgsql")]
    [Export(typeof(DBSchemaProvider))]
    public class PostgresSchemaProvider : DBSchemaProvider
    {
        public override string GetTableDescription(string schema, string tableName)
        {
            var sql =
                @"
                    select
                     pd.description
                    from
	                     pg_stat_user_tables psut
	                    ,pg_description      pd
                    where
	                    psut.relname=@tableName
	                    and
	                    psut.relid=pd.objoid
	                    and
	                    pd.objsubid=0
                    ";
            using (var cmd = Connection.CreateCommand())
            {
                AddDbParameter(cmd, tableName, "@tableName");

                cmd.CommandText = sql;
                return cmd.ExecuteScalar().GetValueOrDefault("");
            }
        }
        protected override void DialectColumn(string schema, string tableName, List<ColumnDefinition> list)
        {
            var dic = list.ToDictionary(x => x.Column.Name);
            var sql =
            @"
                select
                     T4.table_catalog   AS TABLE_CATALOG
                    ,T1.schemaname      AS SCHEMA_NAME
                    ,T1.relname         AS TABLE_NAME
                    ,T4.column_name     AS COLUMN_NAME
                    ,case T4.is_nullable
                        when 'YES' then true
                        else false 
                     end                AS IS_NULLABLE
                    ,data_type          AS DATA_TYPE
                    ,case data_type
                        when 'bit' then null
                        else character_maximum_length
                     end                AS MAX_LENGTH
                    ,case data_type
                        when 'integer' then null
                        else numeric_precision
                     end                AS NUMERIC_PRECISION
                    ,case data_type
                        when 'integer' then null
                        else numeric_scale
                     end                AS NUMERIC_SCALE
                    ,description        AS DESCRIPTION
                from
                    pg_stat_all_tables T1
                left join
                    pg_description T2 on T1.relid = T2.objoid
                left join
                    pg_attribute T3 on T2.objoid = T3.attrelid and T2.objsubid = T3.attnum
                left join
                    information_schema.columns T4 on T1.relname = T4.table_name 
                                                 and T1.schemaname = T4.table_schema
                where
                    T1.schemaname = @schemaname
                and
                    T1.relname = @tableName
                and
                    T3.attname = T4.column_name
                order by
                    T4.ordinal_position
                    ";
            using (var cmd = Connection.CreateCommand())
            {
                var ds = new DataSet();
                cmd.CommandText = sql;
                AddDbParameter(cmd, schema, "@schemaname");
                AddDbParameter(cmd, tableName, "@tableName");

                var adapter = CreateDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                var table = ds.Tables[0];
                foreach (var row in table.Rows.OfType<DataRow>())
                {
                    var column_name = row["COLUMN_NAME"].GetValueOrDefault("");
                    if (dic.ContainsKey(column_name))
                    {
                        var item = dic[column_name];
                        var data_type = row["DATA_TYPE"].GetValueOrDefault("");
                        var max_length = row["MAX_LENGTH"].GetValueOrDefault("");
                        var precision = row["NUMERIC_PRECISION"].GetValueOrDefault("");
                        var scale = row["NUMERIC_SCALE"].GetValueOrDefault("");
                        var description = row["DESCRIPTION"].GetValueOrDefault("");

                        if (string.IsNullOrWhiteSpace(max_length))
                        {
                            if (!data_type.Contains("int") &&
                                !string.IsNullOrWhiteSpace(precision) &&
                                !string.IsNullOrWhiteSpace(scale))
                            {
                                item.Column.DbType = string.Format("{0}({1},{2})", data_type, precision, scale);
                            }
                            else
                            {
                                item.Column.DbType = data_type;
                            }
                        }
                        else
                        {
                            item.Column.DbType = string.Format("{0}({1})", data_type, max_length);
                        }
                        item.Description = string.Format("{0}{1}", description, item.Column.IsPrimaryKey ? "[PK]" : string.Empty);
                    }
                }
            }
        }

        protected override Type ResolveMapType(string dbType)
        {
            switch (dbType)
            {
                case "int8": return typeof(Decimal);
                case "bool": return typeof(Boolean);
                case "bytea": return typeof(Byte[]);
                case "date": return typeof(DateTime);
                case "float8": return typeof(Decimal);
                case "int4": return typeof(Int32);
                case "money": return typeof(Decimal);
                case "numeric": return typeof(Decimal);
                case "float4": return typeof(Decimal);
                case "int2": return typeof(Decimal);
                case "text": return typeof(String);
                case "time": return typeof(DateTime);
                case "timetz": return typeof(DateTime);
                case "timestamp": return typeof(DateTime);
                case "timestamptz": return typeof(DateTime);
                case "interval": return typeof(TimeSpan);
                case "varchar": return typeof(String);
                case "inet": return typeof(System.Net.IPAddress);
                case "bit": return typeof(Boolean);
                case "boolean": return typeof(Boolean);
                case "uuid": return typeof(Guid);
                case "array": return typeof(Array);
                //---------
                case "bpchar":
                    return typeof(String);
                default:
                    return typeof(object);
            }
        }
    }
}

