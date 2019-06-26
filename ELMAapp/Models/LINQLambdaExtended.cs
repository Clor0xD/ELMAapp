using System;
using System.Linq;
using System.Linq.Expressions;

namespace ELMAapp.Models
{
    /// <summary>
    /// Предоставляет рефлексию и селективность запроса, повышает читаемость запроса и сокращает ветвления
    /// </summary>
    public static class LinqLambdaExtended
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering)
        {
            var type = typeof(T);
            var property = type.GetProperty(ordering);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderBy",
                new Type[] {type, property.PropertyType}, source.Expression, Expression.Quote(orderByExp));
            return (IOrderedQueryable<T>) source.Provider.CreateQuery<T>(resultExp);
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string ordering)
        {
            var type = typeof(T);
            var property = type.GetProperty(ordering);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderByDescending",
                new Type[] {type, property.PropertyType}, source.Expression, Expression.Quote(orderByExp));
            return (IOrderedQueryable<T>) source.Provider.CreateQuery<T>(resultExp);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, bool direct)
        {
            return direct ? source.OrderBy(ordering) : source.OrderByDescending(ordering);
        }

        /// <summary>
        /// Реализует возможность пропуска вторичной сортировки т.к NHibernate не допускает дважды сортировать по одному полю
        /// </summary>
        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
            this IOrderedQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector, bool enable)
        {
            if (enable)
            {
                return (IOrderedQueryable<TSource>) source.Provider.CreateQuery<TSource>(
                    Expression.Call(typeof(Queryable), "ThenBy",
                        new Type[] {typeof(TSource), typeof(TKey)},
                        source.Expression, Expression.Quote(keySelector)));
            }

            return source;
        }
    }
}