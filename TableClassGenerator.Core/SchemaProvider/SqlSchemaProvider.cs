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
    [ExportMetadata(Repository.Context, "System.Data.SqlClient")]
    [Export(typeof(DBSchemaProvider))]
    public class SqlSchemaProvider : DBSchemaProvider
    {
        public override IEnumerable<Tuple<string, string>> GetTableNames()
        {
            var ds = new DataSet();
            var sql = @"SELECT * FROM INFORMATION_SCHEMA.TABLES";
            using (var cmd = CreateCommand())
            {
                cmd.CommandText = sql;
                Fill(ds, cmd);
            }

            var table = ds.Tables[0];
            foreach (var row in table.Rows.OfType<DataRow>())
            {
                yield return Tuple.Create(
                    row["TABLE_SCHEMA"].GetValueOrDefault(""),
                    row["TABLE_NAME"].GetValueOrDefault(""));
            }
        }

        public override string Delimit(string input)
        {
            return string.Format("[{0}]", input);
        }

        protected DataTable TableExtendedProperties
        {
            get
            {
                if (_tableExtendedProperties == null)
                {
                    _tableExtendedProperties = ListExtendedProperties();
                }
                return _tableExtendedProperties;
            }
        }
        DataTable _tableExtendedProperties;

        protected DataTable ListExtendedProperties()
        {
            var ds = new DataSet();
            var sql = @"
                        select
                          T1.TABLE_CATALOG	as [Catalog]
                         ,T1.TABLE_SCHEMA	as [Schema]
                         ,T1.TABLE_NAME		as [TableOrViewName]
                         ,T3.name			as [ExrtendetKey]
                         ,T3.value			as [ExrtendetValue]
                        from
                         INFORMATION_SCHEMA.TABLES T1
                        inner join
                         sysobjects T2 on T1.TABLE_NAME = T2.name
                        left join
                         sys.extended_properties T3 on T2.id = T3.major_id and T3.minor_id = 0";
            using (var cmd = CreateCommand())
            {
                cmd.CommandText = sql;
                Fill(ds, cmd);
            }
            return ds.Tables[0];
        }
        protected DataView TableView
        {
            get
            {
                if (_tableView == null)
                {
                    _tableView = new DataView(TableExtendedProperties, null, "Schema ASC, TableOrViewName ASC", DataViewRowState.CurrentRows);
                }
                return _tableView;
            }
        }
        DataView _tableView;


        public override string GetTableDescription(string schema, string tableName)
        {
            var rows = TableView.FindRows(new object[] { schema, tableName })
                                .Select(x => x.Row)
                                .ToArray();
            var heightPriorityValue = rows.Where(x => x["ExrtendetKey"].GetValueOrDefault<string>(null) == "MS_Description")
                                          .Select(x => x["ExrtendetValue"].GetValueOrDefault<string>(null))
                                          .FirstOrDefault();
            var lowPriorityValue = rows.Select(x => x["ExrtendetValue"].GetValueOrDefault<string>(null))
                                       .FirstOrDefault();
            return heightPriorityValue ?? lowPriorityValue;
        }

        public override IEnumerable<ColumnDefinition> GetColumns(string schema, string tableName)
        {
            var ds = new DataSet();
            var sql = @"
                    select
                      TBL.TABLE_CATALOG	as [Catalog]
                     ,TBL.TABLE_SCHEMA	as [Schema]
                     ,TBL.TABLE_NAME	as [TableOrViewName]
                     ,COL.name			as [ColumnName]
                     ,COL.column_id		as [ColumnID]
                     ,EXT.name			as [ExrtendetKey]
                     ,EXT.value			as [ExrtendetValue]
                     ,COL.is_nullable	as [IsNullable]
                     ,INFO.DATA_TYPE	as [DataType]
                     ,INFO.CHARACTER_MAXIMUM_LENGTH as [CharLength]
                     ,INFO.NUMERIC_PRECISION		as [Presision]
                     ,INFO.NUMERIC_SCALE			as [Scale]
                     ,CASE (
		                    select COUNT(*)
		                    from sys.index_columns T1
		                    inner join sys.indexes T2 on T1.object_id = T2.object_id AND T1.index_id = T2.index_id
		                    where T1.object_id = COL.object_id AND T1.column_id = COL.column_id
		                    ) 
	                    WHEN 0 THEN 0
	                    ELSE 1
                      END as [IsPrimary]
                    from
                     INFORMATION_SCHEMA.TABLES TBL
                    inner join
                     sysobjects OBJ on TBL.TABLE_NAME = OBJ.name
                    inner join
                     sys.columns COL on COL.object_id = OBJ.id
                    inner join
                     INFORMATION_SCHEMA.COLUMNS INFO on INFO.TABLE_CATALOG = TBL.TABLE_CATALOG 
								                    AND INFO.TABLE_SCHEMA = TBL.TABLE_SCHEMA
								                    AND INFO.TABLE_NAME = TBL.TABLE_NAME
                    left join
                     sys.extended_properties EXT on COL.object_id = EXT.major_id AND COL.column_id = EXT.minor_id
                    where
                     COL.name = INFO.COLUMN_NAME
                    and
                      TBL.TABLE_SCHEMA = @TABLE_SCHEMA
                    and
                      TBL.TABLE_NAME = @TABLE_NAME
                    order by 1,2,3,5";
            using (var cmd = CreateCommand())
            {
                cmd.CommandText = sql;
                AddDbParameter(cmd, schema, "@TABLE_SCHEMA");
                AddDbParameter(cmd, tableName, "@TABLE_NAME");
                Fill(ds, cmd);
            }

            var table = ds.Tables[0];
            var list = new List<ColumnDefinition>();
            var index = 10;
            foreach (DataRow each in table.Rows)
            {
                var colmunName = each["ColumnName"].GetValueOrDefault(string.Empty);
                var description = each["ExrtendetValue"].GetValueOrDefault(string.Empty);

                var row = list.FirstOrDefault(x => x.Column.Name == colmunName);
                if (row != null)
                {
                    row.Description += Environment.NewLine + description;
                }
                else
                {
                    var newInfo = new ColumnDefinition()
                    {
                        TableName = tableName,
                        Description = description,
                    };
                    newInfo.Column.Name = colmunName;
                    newInfo.Column.CanBeNull = each["IsNullable"].GetValueOrDefault(string.Empty) == "1";
                    newInfo.Column.DbType = ParseTypeString(
                                                each["DataType"].GetValueOrDefault(string.Empty),
                                                each["CharLength"].GetValueOrDefault(string.Empty),
                                                each["Presision"].GetValueOrDefault(string.Empty),
                                                each["Scale"].GetValueOrDefault(string.Empty));
                    newInfo.Column.IsPrimaryKey = each["IsPrimary"].GetValueOrDefault(string.Empty) == "1";
                    newInfo.Column.Identity = index;
                    newInfo.MapType = ResolveMapType(each["DataType"].GetValueOrDefault(string.Empty));


                    list.Add(newInfo);
                    index += 10;
                }
            }
            return list;
        }

        string ParseTypeString(string dataType, string charLength, string precision, string scale)
        {
            if (!string.IsNullOrWhiteSpace(charLength))
            {
                var len = charLength == "-1" ? "max" : charLength;
                return string.Format("{0}({1})", dataType, len);
            }
            if (!string.IsNullOrWhiteSpace(precision))
            {
                return string.Format("{0}({1},{2})", dataType, precision, scale);
            }
            return dataType;
        }


        protected override Type ResolveMapType(string dbType)
        {
            switch (dbType)
            {
                case "text":
                case "ntext":
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                    return typeof(string);
                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                    return typeof(decimal);
                case "int":
                    return typeof(Int32);
                case "smallint":
                    return typeof(Int16);
                case "bigint":
                    return typeof(Int64);
                case "binary":
                case "varbinary":
                case "image":
                case "rowversion":
                case "timestamp":
                case "bit":
                    return typeof(bool);
                case "date":
                case "datetime":
                case "datetime2":
                    return typeof(DateTime);
                case "datetimeoffset":
                    return typeof(DateTimeOffset);
                case "tinyint":
                    return typeof(Byte);
                case "float":
                    return typeof(double);
                case "real":
                    return typeof(Single);
                case "time":
                    return typeof(TimeSpan);
                case "uniqueidentifier":
                    return typeof(Guid);
                default:
                    return typeof(object);
            }
        }
    }
}
