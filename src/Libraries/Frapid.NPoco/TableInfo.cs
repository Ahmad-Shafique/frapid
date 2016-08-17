using System;
using System.Linq;
using System.Reflection;

namespace Frapid.NPoco
{
    public class TableInfo
    {
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
        public bool AutoIncrement { get; set; }
        public string SequenceName { get; set; }
        public string AutoAlias { get; set; }
        public bool UseOutputClause { get; set; }

        public TableInfo Clone()
        {
            return new TableInfo()
            {
                AutoAlias = this.AutoAlias,
                AutoIncrement = this.AutoIncrement,
                TableName = this.TableName,
                PrimaryKey = this.PrimaryKey,
                SequenceName = this.SequenceName,                
                UseOutputClause = this.UseOutputClause
            };
        }

        public static TableInfo FromPoco(Type t)
        {
            TableInfo tableInfo = new TableInfo();

            // Get the table name
            object[] a = t.GetTypeInfo().GetCustomAttributes(typeof(TableNameAttribute), true).ToArray();
            tableInfo.TableName = a.Length == 0 ? t.Name : (a[0] as TableNameAttribute).Value;

            // Get the primary key
            a = t.GetTypeInfo().GetCustomAttributes(typeof(PrimaryKeyAttribute), true).ToArray();
            tableInfo.PrimaryKey = a.Length == 0 ? "ID" : (a[0] as PrimaryKeyAttribute).Value;
            tableInfo.SequenceName = a.Length == 0 ? null : (a[0] as PrimaryKeyAttribute).SequenceName;
            tableInfo.AutoIncrement = a.Length == 0 ? true : (a[0] as PrimaryKeyAttribute).AutoIncrement;
            tableInfo.UseOutputClause = a.Length == 0 ? true : (a[0] as PrimaryKeyAttribute).UseOutputClause;

            // Set autoincrement false if primary key has multiple columns
            tableInfo.AutoIncrement = tableInfo.AutoIncrement ? !tableInfo.PrimaryKey.Contains(',') : tableInfo.AutoIncrement;

            return tableInfo;
        }
    }
}