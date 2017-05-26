using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Weblog.DynamicDomainObject
{
    public class DynamicDomainObject : IDynamicMetaObjectProvider, IDisposable
    {
        private Dictionary<string, object> _internalPropertyStorage;

        // suggestion: SetDynamicProperty?
        public object SetProperty(string key, object value)
        {
            // only create dictionary if needed
            if (_internalPropertyStorage == null)
                _internalPropertyStorage = new Dictionary<string, object>(); 
            _internalPropertyStorage[key] = value;
            return value;
        }

        public object GetProperty(string key)
        {
            object value = null;
            _internalPropertyStorage?.TryGetValue(key, out value);
            return value;
        }

        public virtual void Dispose()
        {
            _internalPropertyStorage?.Clear();   
            _internalPropertyStorage = null;
        }

        public object this[string key]
        {
            get { return GetProperty(key); }
            set { SetProperty(key, value); }
        }

        public DynamicMetaObject GetMetaObject(Expression parameter) => new DynamicDomainMetaObject(parameter, this, GetType());

        private sealed class DynamicDomainMetaObject : DynamicMetaObject
        {
            private readonly Type _type;

            internal DynamicDomainMetaObject(Expression parameter, DynamicDomainObject value, Type type)
                : base(parameter, BindingRestrictions.Empty, value)
            {
                _type = type;
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder)
            {
                var restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

                if (binder.ReturnType.IsAssignableFrom(_type))
                    return new DynamicMetaObject(Expression.Constant(Value), restrictions);
                else
                    return new DynamicMetaObject(Expression.Default(binder.ReturnType), restrictions);
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                var propertyInfo = _type.GetProperty(binder.Name);
                if (propertyInfo == null)
                    return BindGetMemberFromDictionary(binder);
                else
                    return BindGetMemberFromProperty(binder, propertyInfo);
            }

            private DynamicMetaObject BindGetMemberFromDictionary(GetMemberBinder binder)
            {
                var restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
                var self = Expression.Convert(Expression, LimitType);
                var getProperty = typeof(DynamicDomainObject).GetMethod("GetProperty");
                Expression target = Expression.Call(self, getProperty, new Expression[] { Expression.Constant(binder.Name) });
                target = FixReturnType(binder, target);
                return new DynamicMetaObject(target, restrictions);
            }

            private DynamicMetaObject BindGetMemberFromProperty(GetMemberBinder binder, System.Reflection.PropertyInfo propertyInfo)
            {
                var restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
                var self = Expression.Convert(Expression, LimitType);
                Expression target = Expression.Property(self, propertyInfo);
                target = FixReturnType(binder, target);
                return new DynamicMetaObject(target, restrictions);
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                var propertyInfo = _type.GetProperty(binder.Name);
                if (propertyInfo == null)
                    return BindSetMemberToDictionary(binder, value);
                else
                    return BindSetMemberToProperty(binder, value, propertyInfo);
            }

            private DynamicMetaObject BindSetMemberToDictionary(SetMemberBinder binder, DynamicMetaObject value)
            {
                var restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
                var self = Expression.Convert(Expression, LimitType);
                var argument = Expression.Convert(value.Expression, typeof(object));
                var setProperty = typeof(DynamicDomainObject).GetMethod("SetProperty");
                var setCall = Expression.Call(self, setProperty, new Expression[] { Expression.Constant(binder.Name), argument });
                return new DynamicMetaObject(Expression.Block(setCall, Expression.Default(binder.ReturnType)), restrictions);
            }

            private DynamicMetaObject BindSetMemberToProperty(SetMemberBinder binder, DynamicMetaObject value, System.Reflection.PropertyInfo propertyInfo)
            {
                var restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);
                var self = Expression.Convert(Expression, LimitType);
                var argument = Expression.Convert(value.Expression, propertyInfo.PropertyType);
                var setCall = Expression.Call(self, propertyInfo.GetSetMethod(), new Expression[] { argument });
                return new DynamicMetaObject(Expression.Block(setCall, Expression.Default(binder.ReturnType)), restrictions);
            }

            private static Expression FixReturnType(DynamicMetaObjectBinder binder, Expression target)
            {
                if (target.Type == binder.ReturnType)
                    return target;

                if (target.Type == typeof(void))
                    return Expression.Block(target, Expression.Default(binder.ReturnType));

                if (binder.ReturnType == typeof(void))
                    return Expression.Block(target, Expression.Empty());

                return Expression.Convert(target, binder.ReturnType);
            }

            public override IEnumerable<string> GetDynamicMemberNames() => _type.GetProperties().Select(p => p.Name);
        }
    }
}
