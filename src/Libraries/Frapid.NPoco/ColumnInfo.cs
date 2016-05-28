﻿using System;
using System.Linq;
using System.Reflection;
using Frapid.NPoco.FluentMappings;

namespace Frapid.NPoco
{
    public class ColumnInfo
    {
        public string ColumnName { get; set; }
        public string ColumnAlias { get; set; }
        public bool ResultColumn { get; set; }
        public bool ComputedColumn { get; set; }
        public ComputedColumnType ComputedColumnType { get; set; }
        public bool IgnoreColumn { get; set; }
        public bool VersionColumn { get; set; }
        public VersionColumnType VersionColumnType { get; set; }
        public bool ForceToUtc { get; set; }
        public Type ColumnType { get; set; }
        public bool ComplexMapping { get; set; }
        public string ComplexPrefix { get; set; }
        public bool SerializedColumn { get; set; }
        public ReferenceType ReferenceType { get; set; }
        public string ReferenceMemberName { get; set; }
        public MemberInfo MemberInfo { get; internal set; }

        public static ColumnInfo FromMemberInfo(MemberInfo mi)
        {
            var ci = new ColumnInfo
                     {
                         MemberInfo = mi
                     };
            var attrs = ReflectionUtils.GetCustomAttributes(mi);
            var colAttrs = attrs.OfType<ColumnAttribute>();
            var columnTypeAttrs = attrs.OfType<ColumnTypeAttribute>();
            var ignoreAttrs = attrs.OfType<IgnoreAttribute>();
            var complexMapping = attrs.OfType<ComplexMappingAttribute>();
            var serializedColumnAttributes = attrs.OfType<SerializedColumnAttribute>();
            var reference = attrs.OfType<ReferenceAttribute>();
            var aliasColumn = attrs.OfType<AliasAttribute>().FirstOrDefault();

            // Check if declaring poco has [ExplicitColumns] attribute
            bool explicitColumns = mi.DeclaringType.GetTypeInfo().GetCustomAttributes(typeof(ExplicitColumnsAttribute), true).Any();

            // Ignore column if declarying poco has [ExplicitColumns] attribute
            // and property doesn't have an explicit [Column] attribute,
            // or property has an [Ignore] attribute
            if((explicitColumns && !colAttrs.Any()) ||
               ignoreAttrs.Any())
            {
                ci.IgnoreColumn = true;
            }

            if(complexMapping.Any())
            {
                ci.ComplexMapping = true;
                ci.ComplexPrefix = complexMapping.First().CustomPrefix;
            }
            else if(serializedColumnAttributes.Any())
            {
                ci.SerializedColumn = true;
            }
            else if(reference.Any())
            {
                ci.ReferenceType = reference.First().ReferenceType;
                ci.ReferenceMemberName = reference.First().ReferenceMemberName ?? "Id";
                ci.ColumnName = reference.First().ColumnName ?? mi.Name + "Id";
                return ci;
            }
            else if(PocoDataBuilder.IsList(mi))
            {
                ci.ReferenceType = ReferenceType.Many;
                return ci;
            }
            else if(mi.GetMemberInfoType().IsAClass() &&
                    !colAttrs.Any())
            {
                ci.ComplexMapping = true;
            }

            // Read attribute
            if(colAttrs.Any())
            {
                var colattr = colAttrs.First();
                ci.ColumnName = colattr.Name ?? mi.Name;
                ci.ForceToUtc = colattr.ForceToUtc;
                ci.ResultColumn = colattr is ResultColumnAttribute;
                ci.VersionColumn = colattr is VersionColumnAttribute;
                ci.VersionColumnType = ci.VersionColumn ? ((VersionColumnAttribute)colattr).VersionColumnType : ci.VersionColumnType;
                ci.ComputedColumn = colattr is ComputedColumnAttribute;
                ci.ComputedColumnType = ci.ComputedColumn ? ((ComputedColumnAttribute)colattr).ComputedColumnType : ComputedColumnType.Always;
            }
            else
            {
                //Todo:The following changes were done to NPoco
                #region "Changes"
                //ci.ColumnName = mi.Name;
                ci.ColumnName = Inflector.AddUnderscores(mi.Name);
                #endregion
            }

            ci.ColumnAlias = aliasColumn != null ? aliasColumn.Alias : null;

            if(columnTypeAttrs.Any())
            {
                ci.ColumnType = columnTypeAttrs.First().Type;
            }

            return ci;
        }
    }

    public class SerializedColumnAttribute: ColumnAttribute
    {
    }
}