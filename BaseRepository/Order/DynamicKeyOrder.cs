﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GenericRepository.BaseRepository
{
    internal class DynamicKeyOrder<T> : IOrder<T>
    {
        private readonly string _propertyName;
        private readonly Func<MethodInfo> _orderFunc;

        private static readonly Func<MethodInfo> AscendingOrder = () => typeof(Queryable).GetMethods().Single(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
        private static readonly Func<MethodInfo> DescendingOrder = () => typeof(Queryable).GetMethods().Single(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);

        private DynamicKeyOrder(string propertyName, Func<MethodInfo> orderFunc)
        {
            _propertyName = propertyName;
            _orderFunc = orderFunc;
        }

        public static DynamicKeyOrder<T> Ascending(string propertyName)
        {
            return new DynamicKeyOrder<T>(propertyName, AscendingOrder);
        }

        public static DynamicKeyOrder<T> Descending(string propertyName)
        {
            return new DynamicKeyOrder<T>(propertyName, DescendingOrder);
        }

        public IOrderedQueryable<T> Apply(IQueryable<T> queryable)
        {
            //Create parameter named "o"
            var parameter = Expression.Parameter(typeof(T), "o");
            //Search the member- field or property. You can also pass it BindingFlags.IgnoreCase to do case insensitive search.
            var memberInfo = typeof(T).GetMember(_propertyName, MemberTypes.Property | MemberTypes.Field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();
            if (memberInfo == null)
            {
                throw new ArgumentException(string.Format("Can not find the property or field by name {0} on type {1}", _propertyName, typeof(T).Name));
            }

            //Create o.MemberName expression
            var property = Expression.MakeMemberAccess(parameter, memberInfo);

            var memberType = GetMemberType(memberInfo);
            //Create lamdba expression o=>o.Member e.g. o=>o.Category
            var lamdbaDelegate = typeof(Func<,>).MakeGenericType(typeof(T), memberType);
            var lambdaExpression = Expression.Lambda(lamdbaDelegate, property, parameter);

            var method = _orderFunc();
            //Costruct the method by providing both generic types- TSource and TKey.
            var methodName = method.MakeGenericMethod(typeof(T), memberType);

            //Call the static method - OrderBy/OrderByDescending by passing two arguments- orginal IQueryable<T> (in form of expression) and sorting expression
            //e.g.- OrderBy(IQueryable<T> originalInstance, o=>o.MamberName)
            var callOrderBy = Expression.Call(methodName, queryable.Expression, lambdaExpression);

            //Generate new query out of this expression.
            return (IOrderedQueryable<T>)queryable.Provider.CreateQuery(callOrderBy);
        }

        private Type GetMemberType(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                return ((FieldInfo)memberInfo).FieldType;
            }
            if (memberInfo.MemberType == MemberTypes.Property)
            {
                return ((PropertyInfo)memberInfo).PropertyType;
            }

            throw new NotSupportedException(string.Format("Does not support the member type {0}", memberInfo.MemberType));
        }
    }
}