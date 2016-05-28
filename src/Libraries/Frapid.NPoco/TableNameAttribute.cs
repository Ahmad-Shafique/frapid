using System;

namespace Frapid.NPoco
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        public TableNameAttribute(string tableName)
        {
            this.Value = tableName;
        }
        public string Value { get; private set; }
    }
}