using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Sandwych.Jasper {

    public static class QueryableExtensions {

        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source,
            string json, IReadOnlyDictionary<string, object> symbols = null) {
            var doc = JsonDocument.Parse(json);
            var visitor = new JsonDocumentVisitor(typeof(TSource));
            var predicate = visitor.ToPredicate(doc);

            var linqWhereType = Where_TSource_2(typeof(TSource));
            var callWhere = Expression.Call(null, linqWhereType, source.Expression, Expression.Quote(predicate));
            return source.Provider.CreateQuery<TSource>(callWhere);
        }

        private static MethodInfo Where_TSource_2(Type TSource) {
            var method =
                    new Func<IQueryable<object>, Expression<Func<object, bool>>, IQueryable<object>>(Queryable.Where)
                    .GetMethodInfo()
                    .GetGenericMethodDefinition();
            return method.MakeGenericMethod(TSource);
        }


    }
}
