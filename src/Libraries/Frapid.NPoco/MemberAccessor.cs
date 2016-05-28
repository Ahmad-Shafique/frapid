using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Frapid.NPoco
{
    /// <summary>
    /// The PropertyAccessor class provides fast dynamic access
    /// to a property of a specified target class.
    /// </summary>
    public class MemberAccessor
    {
        private readonly Type _targetType;
        private Type _memberType;
        private static Hashtable _mTypeHash = new Hashtable
        {
            [typeof(sbyte)] = OpCodes.Ldind_I1,
            [typeof(byte)] = OpCodes.Ldind_U1,
            [typeof(char)] = OpCodes.Ldind_U2,
            [typeof(short)] = OpCodes.Ldind_I2,
            [typeof(ushort)] = OpCodes.Ldind_U2,
            [typeof(int)] = OpCodes.Ldind_I4,
            [typeof(uint)] = OpCodes.Ldind_U4,
            [typeof(long)] = OpCodes.Ldind_I8,
            [typeof(ulong)] = OpCodes.Ldind_I8,
            [typeof(bool)] = OpCodes.Ldind_I1,
            [typeof(double)] = OpCodes.Ldind_R8,
            [typeof(float)] = OpCodes.Ldind_R4
        };
        private bool _canRead;
        private readonly bool _canWrite;
        private readonly MemberInfo _member;

        /// <summary>
        /// Creates a new property accessor.
        /// </summary>
        /// <param name="targetType">Target object type.</param>
        /// <param name="memberName">Property name.</param>
        public MemberAccessor(Type targetType, string memberName)
        {
            this._targetType = targetType;
            MemberInfo memberInfo = ReflectionUtils.GetFieldsAndPropertiesForClasses(targetType).First(x => x.Name == memberName);

            if (memberInfo == null)
            {
                throw new Exception(string.Format("Property \"{0}\" does not exist for type " + "{1}.", memberName, targetType));
            }

            this._canRead = memberInfo.IsField() || ((PropertyInfo) memberInfo).CanRead;
            this._canWrite = memberInfo.IsField() || ((PropertyInfo) memberInfo).CanWrite;

            // roslyn automatically implemented properties, in particular for get-only properties: <{Name}>k__BackingField;
            if (!this._canWrite)
            {
                string backingFieldName = $"<{memberName}>k__BackingField";
                FieldInfo backingFieldMemberInfo = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(x => x.Name == backingFieldName);
                if (backingFieldMemberInfo != null)
                {
                    memberInfo = backingFieldMemberInfo;
                    this._canWrite = true;
                }
            }

            this._memberType = memberInfo.GetMemberInfoType();
            this._member = memberInfo;

            if (this._canWrite)
            {
                this.SetDelegate = this.GetSetDelegate();
            }

            if (this._canRead)
            {
                this.GetDelegate = this.GetGetDelegate();
            }
        }

        private Func<object, object> GetDelegate = null;

        private Action<object, object> SetDelegate = null;

        /// <summary>
        /// Sets the property for the specified target.
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <param name="value">Value to set.</param>
        public void Set(object target, object value)
        {
            this.SetDelegate?.Invoke(target, value);
        }

        public object Get(object target)
        {
            return this.GetDelegate?.Invoke(target);
        }

        private Action<object, object> GetSetDelegate()
        {
            Type[] setParamTypes = new Type[] { typeof(object), typeof(object) };
            Type setReturnType = null;

            Type owner = this._targetType.GetTypeInfo().IsAbstract || this._targetType.GetTypeInfo().IsInterface ? null : this._targetType;
            DynamicMethod setMethod = owner != null 
                ? new DynamicMethod(Guid.NewGuid().ToString(), setReturnType, setParamTypes, owner, true)
                : new DynamicMethod(Guid.NewGuid().ToString(), setReturnType, setParamTypes, true);
            // From the method, get an ILGenerator. This is used to
            // emit the IL that we want.
            //
            ILGenerator setIL = setMethod.GetILGenerator();
            //
            // Emit the IL. 
            //

            Type paramType = this._memberType;
            setIL.Emit(OpCodes.Ldarg_0); //Load the first argument 
            //(target object)
            //Cast to the source type
            setIL.Emit(OpCodes.Castclass, this._targetType);
            setIL.Emit(OpCodes.Ldarg_1); //Load the second argument 
            //(value object)
            if (paramType.GetTypeInfo().IsValueType)
            {
                setIL.Emit(OpCodes.Unbox, paramType); //Unbox it 
                if (_mTypeHash[paramType] != null) //and load
                {
                    OpCode load = (OpCode)_mTypeHash[paramType];
                    setIL.Emit(load);
                }
                else
                {
                    setIL.Emit(OpCodes.Ldobj, paramType);
                }
            }
            else
            {
                setIL.Emit(OpCodes.Castclass, paramType); //Cast class
            }

            if (this._member.IsField())
            {
                setIL.Emit(OpCodes.Stfld, (FieldInfo)this._member);
            }
            else
            {
                MethodInfo targetSetMethod = ((PropertyInfo)this._member).GetSetMethodOnDeclaringType();
                if (targetSetMethod != null)
                {
                    setIL.Emit(OpCodes.Callvirt, targetSetMethod);
                }
                else
                {
                    setIL.ThrowException(typeof(MissingMethodException));
                }
            }
            setIL.Emit(OpCodes.Ret);

            Delegate del = setMethod.CreateDelegate(Expression.GetActionType(setParamTypes));
            return del as Action<object, object>;
        }

        private Func<object, object> GetGetDelegate()
        {
            Type[] setParamTypes = new[] { typeof(object) };
            Type setReturnType = typeof (object);

            Type owner = this._targetType.GetTypeInfo().IsAbstract || this._targetType.GetTypeInfo().IsInterface ? null : this._targetType;
            DynamicMethod getMethod = owner != null 
                ? new DynamicMethod(Guid.NewGuid().ToString(), setReturnType, setParamTypes, owner, true)
                : new DynamicMethod(Guid.NewGuid().ToString(), setReturnType, setParamTypes, true);

            // From the method, get an ILGenerator. This is used to
            // emit the IL that we want.
            //
            ILGenerator getIL = getMethod.GetILGenerator();
            
            getIL.DeclareLocal(typeof(object));
            getIL.Emit(OpCodes.Ldarg_0); //Load the first argument
            //(target object)
            //Cast to the source type
            getIL.Emit(OpCodes.Castclass, this._targetType);
            //Get the property value

            if (this._member.IsField())
            {
                getIL.Emit(OpCodes.Ldfld, (FieldInfo)this._member);
            }
            else
            {
                MethodInfo targetGetMethod = ((PropertyInfo) this._member).GetGetMethod();
                getIL.Emit(OpCodes.Callvirt, targetGetMethod);
                if (targetGetMethod.ReturnType.GetTypeInfo().IsValueType)
                {
                    getIL.Emit(OpCodes.Box, targetGetMethod.ReturnType);
                    //Box if necessary
                }
            }

            getIL.Emit(OpCodes.Ret);

            Delegate del = getMethod.CreateDelegate(Expression.GetFuncType(setParamTypes.Concat(new[]{setReturnType}).ToArray()));
            return del as Func<object, object>;
        }
    }
}