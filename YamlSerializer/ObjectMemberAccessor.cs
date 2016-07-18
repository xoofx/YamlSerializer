﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Reflection;

namespace YamlSerializer.Serialization
{
    /// <summary>
    /// 
    /// object に代入されたクラスや構造体のメンバーに、リフレクションを
    /// 解して簡単にアクセスできるようにしたクラス
    /// 
    /// アクセス方法をキャッシュするので、繰り返し使用する場合に高速化が
    /// 期待できる
    /// </summary>
    internal class ObjectMemberAccessor
    {
        private readonly static object[] EmptyObjectArray = new object[0];

        /// <summary>
        /// Caches ObjectMemberAccessor instances for reuse.
        /// </summary>
        static Dictionary<Type, ObjectMemberAccessor> MemberAccessors = new Dictionary<Type, ObjectMemberAccessor>();
        /// <summary>
        /// 
        /// 指定した型へのアクセス方法を表すインスタンスを返す
        /// キャッシュに存在すればそれを返す
        /// キャッシュに存在しなければ新しく作って返す
        /// 作った物はキャッシュされる
        /// </summary>
        /// <param name="type">クラスまたは構造体を表す型情報</param>
        /// <returns></returns>
        public static ObjectMemberAccessor FindFor(Type type)
        {
            if ( !MemberAccessors.ContainsKey(type) )
                MemberAccessors[type] = new ObjectMemberAccessor(type);
            return MemberAccessors[type];
        }

        private ObjectMemberAccessor(Type type)
        {
            /*
            if ( !TypeUtils.IsPublic(type) )
                throw new ArgumentException(
                    "Can not serialize non-public type {0}.".DoFormat(type.FullName));
            */

            // public properties
#if NET40
            foreach ( var p in type.GetProperties(
                    BindingFlags.Instance | 
                    BindingFlags.Public
                    ) ) {
#else
            foreach (var p in type.GetTypeInfo().GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public
                    ))
            {
#endif
                var prop = p; // create closures with this local variable
                
                if (Members.ContainsKey(prop.Name))
                {
                    continue;
                }

                // not readable or parameters required to access the property
                if ( !prop.CanRead || prop.GetGetMethod(false) == null || prop.GetIndexParameters().Count() != 0 )
                    continue;
                Func<object, object> get = obj => prop.GetValue(obj, EmptyObjectArray);
                Action<object, object> set = null;
                if ( prop.CanWrite && prop.GetSetMethod(false) != null )
                    set = (obj, value) => prop.SetValue(obj, value, EmptyObjectArray);

                RegisterMember(type, prop, prop.PropertyType, get, set);
            }

            // public fields
#if NET40
            foreach ( var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public) ) {
#else
            foreach (var f in type.GetTypeInfo().GetFields(BindingFlags.Instance |
                    BindingFlags.Public))
            {
#endif
                var field = f;
                if ( !field.IsPublic )
                    continue;
                Func<object, object> get = obj => field.GetValue(obj);
                Action<object, object> set = (obj, value) => field.SetValue(obj, value);
                RegisterMember(type, field, field.FieldType, get, set);
            }

            Type itype;

            // implements IDictionary
            if ( type.GetInterface(typeof(IDictionary)) != null ) {
                IsDictionary = true;
                IsReadOnly = obj => ( (System.Collections.IDictionary)obj ).IsReadOnly;
                // extract Key, Value types from IDictionary<??, ??>
                itype = type.GetInterface(typeof(IDictionary<,>));
                if ( itype != null ) {
                    KeyType = itype.GetGenericArguments()[0];
                    ValueType = itype.GetGenericArguments()[1];
                }

                // If the dictionary has no property we can handle it directly
                IsPureDictionary = IsPure();
            } else
                // implements ICollection<T> 
                if ( ( itype = type.GetInterface(typeof(ICollection<>)) ) != null ) {
                    ValueType = itype.GetGenericArguments()[0];
                    var add = itype.GetMethod("Add", new Type[] { ValueType });
                    CollectionAdd = (obj, value) => add.Invoke(obj, new object[] { value });
                    var clear = itype.GetMethod("Clear", new Type[0]);
                    CollectionClear = obj => clear.Invoke(obj, new object[0]);
                    var isReadOnly = itype.GetProperty("IsReadOnly").GetGetMethod();
                    IsReadOnly = obj => (bool)isReadOnly.Invoke(obj, new object[0]);
                    IsPureList = IsPure("Capacity");
                }
                else
                    // implements IList 
                    if ((itype = type.GetInterface(typeof(IList))) != null)
                    {
                        var add = itype.GetMethod("Add", new Type[] {typeof(object)});
                        CollectionAdd = (obj, value) => add.Invoke(obj, new object[] {value});
                        var clear = itype.GetMethod("Clear", new Type[0]);
                        CollectionClear = obj => clear.Invoke(obj, new object[0]);
                        /* IList<T> implements ICollection<T>
                        // Extract Value Type from IList<T>
                        itype = type.GetInterface("System.Collections.Generic.IList`1");
                        if ( itype != null )
                            ValueType = itype.GetGenericArguments()[0];     
                         */
                        IsReadOnly = obj => ((System.Collections.IList)obj).IsReadOnly;
                        // Remove Capacity
                        IsPureList = IsPure("Capacity");
                    }
        }

        private void RegisterMember(Type type, System.Reflection.MemberInfo m, Type mType, Func<object, object> get, Action<object, object> set)
        {
            // struct that holds access method for property/field
            MemberInfo accessor = new MemberInfo();

            accessor.Type = mType;
            accessor.Get = get;
            accessor.Set = set;

#if NETCORE
            var typeInfo = mType.GetTypeInfo();
#endif

            if(set!=null){ // writeable ?
                accessor.SerializeMethod = YamlSerializeMethod.Assign;
            } else {
                accessor.SerializeMethod = YamlSerializeMethod.Never;
#if !NETCORE
                if ( mType.IsClass )
#else
                if ( typeInfo.IsClass)
#endif
                accessor.SerializeMethod = YamlSerializeMethod.Content;
            }
            var attr1 = m.GetAttribute<YamlSerializeAttribute>();
            if ( attr1 != null ) { // specified
                if ( set == null ) { // read only member
                    if ( attr1.SerializeMethod == YamlSerializeMethod.Assign ||
#if !NETCORE
                         ( mType.IsValueType && accessor.SerializeMethod == YamlSerializeMethod.Content ) )
#else
                         ( typeInfo.IsValueType && accessor.SerializeMethod == YamlSerializeMethod.Content ) )
#endif
                        throw new ArgumentException("{0} {1} is not writeable by {2}."
                            .DoFormat(mType.FullName, m.Name, attr1.SerializeMethod.ToString()));
                }
                accessor.SerializeMethod = attr1.SerializeMethod;
            }
            if ( accessor.SerializeMethod == YamlSerializeMethod.Never )
                return; // no need to register
            if ( accessor.SerializeMethod == YamlSerializeMethod.Binary ) {
                if ( !mType.IsArray )
                    throw new InvalidOperationException("{0} {1} of {2} is not an array. Can not be serialized as binary."
                        .DoFormat(mType.FullName, m.Name, type.FullName));
                if ( !TypeUtils.IsPureValueType(mType.GetElementType()) )
                    throw new InvalidOperationException(
                        "{0} is not a pure ValueType. {1} {2} of {3} can not serialize as binary."
                        .DoFormat(mType.GetElementType(), mType.FullName, m.Name, type.FullName));
            }

            // ShouldSerialize
            //      YamlSerializeAttribute(Never) => false
            //      ShouldSerializeSomeProperty => call it
            //      DefaultValueAttribute(default) => compare to it
            //      otherwise => true
            var shouldSerialize = type.GetMethod("ShouldSerialize" + m.Name,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if ( shouldSerialize != null && shouldSerialize.ReturnType == typeof(bool) && accessor.ShouldSeriealize == null )
                accessor.ShouldSeriealize = obj => (bool)shouldSerialize.Invoke(obj, EmptyObjectArray);
            var attr2 = m.GetAttribute<DefaultValueAttribute>();
            if ( attr2 != null && accessor.ShouldSeriealize == null ) {
                var defaultValue = attr2.Value;
                if ( TypeUtils.IsNumeric(defaultValue) && defaultValue.GetType() != mType )
                    defaultValue = TypeUtils.CastToNumericType(defaultValue, mType);
                accessor.ShouldSeriealize = obj => !TypeUtils.AreEqual(defaultValue, accessor.Get(obj));
            }
            if ( accessor.ShouldSeriealize == null )
                accessor.ShouldSeriealize = obj => true;

            Members.Add(m.Name, accessor);
        }

        private bool IsPure(params string[] excludeField)
        {
            // If the dictionary has no property we can handle it directly
            var isPure = true;
            foreach (var member in Members)
            {
                if (member.Value.Set != null && member.Value.SerializeMethod != YamlSerializeMethod.Never && !excludeField.Contains(member.Key))
                {
                    isPure = false;
                    break;
                }
            }
            return isPure;
        }

        public bool IsDictionary = false;
        public bool IsPureDictionary = false;
        public bool IsPureList = false;
        public Action<object, object> CollectionAdd = null;
        public Action<object> CollectionClear = null;
        public Type KeyType = null;
        public Type ValueType = null;
        public Func<object,bool> IsReadOnly;

        public struct MemberInfo
        {
            public YamlSerializeMethod SerializeMethod;
            public Func<object, object> Get;
            public Action<object, object> Set;
            public Func<object, bool> ShouldSeriealize;
            public Type Type;
        }

        public readonly Dictionary<string, MemberInfo> Members = new Dictionary<string, MemberInfo>();

        public MemberInfo this[string name]
        {
            get { return Members[name]; }
        }
        public bool ContainsKey(string name)
        {
            return Members.ContainsKey(name);
        }

        /// <summary>
        /// メンバへの読み書きを行うことができる
        /// </summary>
        /// <param name="obj">オブジェクト</param>
        /// <param name="name">メンバの名前</param>
        /// <returns></returns>
        public object this[object obj, string name]
        {
            get { return Members[name].Get(obj); }
            set { Members[name].Set(obj, value); }
        }

        /// <summary>
        /// メンバ名と Accessor のペアを巡回する
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, MemberInfo>.Enumerator GetEnumerator()
        {
            return Members.GetEnumerator();
        }
    }
}
