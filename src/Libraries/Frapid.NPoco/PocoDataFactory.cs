using System;
using System.Collections.Generic;

namespace Frapid.NPoco
{
    public interface IPocoDataFactory
    {
        PocoData ForType(Type type);
        TableInfo TableInfoForType(Type type);
        PocoData ForObject(object o, string primaryKeyName, bool autoIncrement);
    }

    public class FluentPocoDataFactory : IPocoDataFactory
    {
        private readonly Cache<Type, PocoDataBuilder> _pocoDatas = Cache<Type, PocoDataBuilder>.CreateStaticCache();
        public Func<Type, IPocoDataFactory, PocoDataBuilder> Resolver { get; private set; }

        public FluentPocoDataFactory(Func<Type, IPocoDataFactory, PocoDataBuilder> resolver)
        {
            this.Resolver = resolver;
        }
        
        public PocoData ForType(Type type)
        {
            PocoDataFactory.Guard(type);
            PocoDataBuilder pocoDataBuilder = this._pocoDatas.Get(type, () => this.Resolver(type, this));
            return pocoDataBuilder.Build();
        }

        public TableInfo TableInfoForType(Type type)
        {
            PocoDataFactory.Guard(type);
            PocoDataBuilder pocoDataBuilder = this._pocoDatas.Get(type, () => this.Resolver(type, this));
            return pocoDataBuilder.BuildTableInfo();
        }

        public PocoData ForObject(object o, string primaryKeyName, bool autoIncrement)
        {
            return PocoDataFactory.ForObjectStatic(o, primaryKeyName, autoIncrement, this.ForType);
        }
    }

    public class PocoDataFactory : IPocoDataFactory
    {
        private readonly static Cache<Type, PocoDataBuilder> _pocoDatas = Cache<Type, PocoDataBuilder>.CreateStaticCache();
        private readonly MapperCollection _mapper;

        public PocoDataFactory(MapperCollection mapper)
        {
            this._mapper = mapper;
        }

        public PocoData ForType(Type type)
        {
            Guard(type);
            PocoDataBuilder pocoDataBuilder = _pocoDatas.Get(type, () => new PocoDataBuilder(type, this._mapper).Init());
            return pocoDataBuilder.Build();
        }

        public TableInfo TableInfoForType(Type type)
        {
            Guard(type);
            PocoDataBuilder pocoDataBuilder = _pocoDatas.Get(type, () => new PocoDataBuilder(type, this._mapper).Init());
            return pocoDataBuilder.BuildTableInfo();
        }

        public PocoData ForObject(object o, string primaryKeyName, bool autoIncrement)
        {
            return ForObjectStatic(o, primaryKeyName, autoIncrement, this.ForType);
        }

        public static PocoData ForObjectStatic(object o, string primaryKeyName, bool autoIncrement, Func<Type, PocoData> fallback)
        {
            Type t = o.GetType();
#if !NET35
            if (t == typeof (System.Dynamic.ExpandoObject) || t == typeof (PocoExpando))
            {
                PocoData pd = new PocoData();
                pd.TableInfo = new TableInfo();
                pd.Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
                pd.Columns.Add(primaryKeyName, new ExpandoColumn() {ColumnName = primaryKeyName});
                pd.TableInfo.PrimaryKey = primaryKeyName;
                pd.TableInfo.AutoIncrement = autoIncrement;
                foreach (KeyValuePair<string, object> col in ((IDictionary<string, object>) o))
                {
                    if (col.Key != primaryKeyName)
                        pd.Columns.Add(col.Key, new ExpandoColumn()
                        {
                            ColumnName = col.Key,
                            MemberInfoData = new MemberInfoData(col.Key, col.Value.GetTheType() ?? typeof (object), typeof (object)),
                        });
                }
                return pd;
            }
            else
#endif
                return fallback(t);
        }

        public static void Guard(Type type)
        {
#if !NET35
            if (type == typeof(System.Dynamic.ExpandoObject) || type == typeof(PocoExpando))
                throw new InvalidOperationException("Can't use dynamic types with this method");
#endif
        }

    }
}