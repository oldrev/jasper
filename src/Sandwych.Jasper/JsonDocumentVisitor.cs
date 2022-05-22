using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Sandwych.Jasper {

    /*
     * ["and", ["=", "name", "Alice"], [">=", "age", 30], ["in", "dept", [1, 2, 3]]]
     */

    public class JsonDocumentVisitor {

        private readonly ParameterExpression _lhs;
        private readonly Type _lhsType;

        public JsonDocumentVisitor(Type lhsType) {
            _lhsType = lhsType;
            _lhs = Expression.Parameter(lhsType);
        }

        public LambdaExpression ToPredicate(JsonDocument document) {
            var filterExpr = this.VisitDocument(document);
            var lambda = Expression.Lambda(filterExpr, new ParameterExpression[] { _lhs });
            return lambda;
        }

        public Expression VisitDocument(JsonDocument document) {
            return this.VisitRootElement(document.RootElement);
        }

        public Expression VisitRootElement(JsonElement element) {
            if (element.ValueKind != JsonValueKind.Array) {
                return null;
            }

            var expr = this.VisitOperatorExpressionElement(element);
            return expr;
        }

        public (BinaryExpression, int) Foo() { throw new NotImplementedException(); }

        private Expression VisitOperatorExpressionElement(JsonElement element) {
            if (element.GetArrayLength() < 1) {
                throw new ParsingErrorException();
            }
            var opr = element[0].GetString();
            var linqExpr = opr switch {
                "and" => this.VisitAndExpressionElement(element),
                "or" => this.VisitOrExpressionElement(element),
                "not" => this.VisitOrExpressionElement(element),
                "=" => this.VisitEqualExpressionElement(element),
                ">" => this.VisitGreaterExpressionElement(element),
                "<" => this.VisitLesserExpressionElement(element),
                ">=" => this.VisitGreaterEqualExpressionElement(element),
                "<=" => this.VisitLesserEqualExpressionElement(element),
                "in" => this.VisitInListExpressionElement(element),
                _ => null,
            };

            return linqExpr;
        }

        private Expression VisitRightOperandElement(JsonElement lhs, JsonElement rhs) {
            var pi= this.GetPropertyOrFieldInfo(_lhsType, lhs.GetString());
            if (pi.PropertyType == typeof(int)) {
                return Expression.Constant(rhs.GetInt32());
            }
            else if (pi.PropertyType == typeof(string)) { 
                return Expression.Constant(rhs.GetString());
            }
            else {
                throw new NotSupportedException();
            }
        }

        private Expression VisitLeftOperandElement(JsonElement e) {
            var props = e.GetString().Split('.');
            return this.AccessProperties(props);
        }

        private Expression VisitAndExpressionElement(JsonElement e) {
            using var iter = e.EnumerateArray();
            var args = iter.Skip(1).Select(x => this.VisitOperatorExpressionElement(x));
            return args.Aggregate((x, y) => Expression.AndAlso(x, y));
        }

        private Expression VisitOrExpressionElement(JsonElement e) {
            using var iter = e.EnumerateArray();
            var args = iter.Skip(1).Select(x => this.VisitOperatorExpressionElement(x));
            return args.Aggregate((x, y) => Expression.OrElse(x, y));
        }

        private Expression VisitNotExpressionElement(JsonElement e) {
            var expr = this.VisitOperatorExpressionElement(e[1]);
            return Expression.Not(expr);
        }

        private Expression VisitEqualExpressionElement(JsonElement e) {
            var lhs = this.VisitLeftOperandElement(e[1]);
            var rhs = this.VisitRightOperandElement(e[1], e[2]);
            return Expression.Equal(lhs, rhs);
        }

        private Expression VisitLesserEqualExpressionElement(JsonElement e) {
            var lhs = this.VisitLeftOperandElement(e[1]);
            var rhs = this.VisitRightOperandElement(e[1], e[2]);
            return Expression.LessThanOrEqual(lhs, rhs);
        }

        private Expression VisitGreaterEqualExpressionElement(JsonElement e) {
            var lhs = this.VisitLeftOperandElement(e[1]);
            var rhs = this.VisitRightOperandElement(e[1], e[2]);
            return Expression.GreaterThanOrEqual(lhs, rhs);
        }

        private Expression VisitLesserExpressionElement(JsonElement e) {
            var lhs = this.VisitLeftOperandElement(e[1]);
            var rhs = this.VisitRightOperandElement(e[1], e[2]);
            return Expression.LessThan(lhs, rhs);
        }

        private Expression VisitGreaterExpressionElement(JsonElement e) {
            var lhs = this.VisitLeftOperandElement(e[1]);
            var rhs = this.VisitRightOperandElement(e[1], e[2]);
            return Expression.GreaterThan(lhs, rhs);
        }

        private Expression VisitInListExpressionElement(JsonElement e) {
            var lhs = this.VisitLeftOperandElement(e[1]);
            var args = e[2].EnumerateArray().Select(x =>
                Expression.Equal(Expression.Constant((decimal)42), this.VisitConstantExpressionElement(x))
            );
            return args.Aggregate((x, y) => Expression.OrElse(x, y));
        }

        private Expression VisitVectorExpressionElement(JsonElement element) {
            using var iter = element.EnumerateArray();
            var constants = iter.Select(x => this.VisitConstantExpressionElement(x));
            var args = iter.Select(x => Expression.Constant(true) as Expression);
            return args.Aggregate((x, y) => Expression.AndAlso(x, y));
        }

        private Expression VisitConstantExpressionElement(JsonElement element) {
            switch (element.ValueKind) {
                case JsonValueKind.String:
                    return Expression.Constant(element.GetString());
                case JsonValueKind.Number:
                    return Expression.Constant(element.GetDecimal());
                case JsonValueKind.True:
                    return Expression.Constant(true);
                case JsonValueKind.False:
                    return Expression.Constant(false);
                case JsonValueKind.Null:
                    return Expression.Constant(null);

                default:
                    throw new ParsingErrorException();
            }
        }

        private Expression AccessProperties(IEnumerable<string> props) {
            var expr = MakeMemberAccessExpression(_lhs, props.First());
            foreach (var p in props.Skip(1)) {
                expr = MakeMemberAccessExpression(expr, p);
            }
            return expr;
        }

        private Expression MakeMemberAccessExpression(Expression objectExpr, string propertyName) {
            //var propertyExpr = Expression.PropertyOrField(objectExpr, propertyName);
            MemberInfo mi = GetPropertyOrFieldInfo(objectExpr.Type, propertyName);

            return Expression.MakeMemberAccess(objectExpr, mi);
        }

        private PropertyInfo GetPropertyOrFieldInfo(Type objectType, string propertyName) {
            var members = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            var mi = (from it in members
                      where (it is PropertyInfo) && it.Name == propertyName
                      select it).SingleOrDefault();
            if (mi == null) {
                throw new ParsingErrorException(
                    $"The type {objectType.FullName} does not have a accessable field/property that named: {propertyName}");
            }
            return mi;
        }




    }

}
