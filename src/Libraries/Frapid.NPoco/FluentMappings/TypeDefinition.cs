using System;
using System.Collections.Generic;

namespace Frapid.NPoco.FluentMappings
{
    public class TypeDefinition
    {
        public TypeDefinition(Type type)
        {
            this.Type = type;
            this.ColumnConfiguration = new Dictionary<string, ColumnDefinition>();
        }

        public Type Type { get; set; }
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
        public string SequenceName { get; set; }
        public bool? AutoIncrement { get; set; }
        public bool? ExplicitColumns { get; set; }
        public Dictionary<string, ColumnDefinition> ColumnConfiguration { get; set; }
        public bool? UseOutputClause { get; set; }
    }
}