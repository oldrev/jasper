using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Sandwych.Jasper {

    public struct PredicateJsonDocumentVisitor {

        private readonly ParameterExpression _lhs;
        private readonly Type _lhsType;

        public PredicateJsonDocumentVisitor(Type lhsType) {
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

        private Expression VisitOperatorExpressionElement(JsonElement element) {
            if (element.GetArrayLength() < 1) {
                throw new ParsingErrorException();
            }
            var opr = element[0].GetString();
            return opr switch {
                "and" => this.VisitAndExpressionElement(element),
                "or" => this.VisitOrExpressionElement(element),
                "not" => this.VisitNotExpressionElement(element),
                "=" => this.VisitEqualExpressionElement(element),
                ">" => this.VisitGreaterExpressionElement(element),
                "<" => this.VisitLesserExpressionElement(element),
                ">=" => this.VisitGreaterEqualExpressionElement(element),
                "<=" => this.VisitLesserEqualExpressionElement(element),
                "in" => this.VisitInListExpressionElement(element),
                "!in" => this.VisitNotInListExpressionElement(element),
                _ => throw new NotSupportedException($"Not supported operator: '{opr}'"),
            };
        }

        private Expression VisitPropertyOperandElement(JsonElement lhs, JsonElement rhs) {
            //TODO 优化和重构，缓存反射数据
            var pi = this.GetPropertyOrFieldInfo(_lhsType, lhs.GetString());

            if (pi.PropertyType == typeof(int)) {
                return Expression.Constant(rhs.GetInt32());
            }
            else if (pi.PropertyType == typeof(string)) {
                return Expression.Constant(rhs.GetString());
            }
            else if (pi.PropertyType == typeof(decimal)) {
                return Expression.Constant(rhs.GetDecimal());
            }
            else if (pi.PropertyType == typeof(float)) {
                return Expression.Constant(rhs.GetSingle());
            }
            else if (pi.PropertyType == typeof(double)) {
                return Expression.Constant(rhs.GetDouble());
            }
            else {
                throw new NotImplementedException();
            }
        }

        private Expression VisitMemberAccessOperandElement(JsonElement e) {
            var props = e.GetString().Split('.');
            return this.AccessProperties(props);
        }

        private Expression VisitAndExpressionElement(JsonElement e) {
            using var iter = e.EnumerateArray();
            var args = iter.Skip(1).Select(VisitOperatorExpressionElement);
            return args.Aggregate((x, y) => Expression.AndAlso(x, y));
        }

        private Expression VisitOrExpressionElement(JsonElement e) {
            using var iter = e.EnumerateArray();
            var args = iter.Skip(1).Select(VisitOperatorExpressionElement);
            return args.Aggregate((x, y) => Expression.OrElse(x, y));
        }

        private Expression VisitNotExpressionElement(JsonElement e) {
            var expr = this.VisitOperatorExpressionElement(e[1]);
            return Expression.Not(expr);
        }

        private Expression VisitEqualExpressionElement(JsonElement e) {
            var lhs = this.VisitMemberAccessOperandElement(e[1]);
            var rhs = this.VisitPropertyOperandElement(e[1], e[2]);
            return Expression.Equal(lhs, rhs);
        }

        private Expression VisitLesserEqualExpressionElement(JsonElement e) {
            var lhs = this.VisitMemberAccessOperandElement(e[1]);
            var rhs = this.VisitPropertyOperandElement(e[1], e[2]);
            return Expression.LessThanOrEqual(lhs, rhs);
        }

        private Expression VisitGreaterEqualExpressionElement(JsonElement e) {
            var lhs = this.VisitMemberAccessOperandElement(e[1]);
            var rhs = this.VisitPropertyOperandElement(e[1], e[2]);
            return Expression.GreaterThanOrEqual(lhs, rhs);
        }

        private Expression VisitLesserExpressionElement(JsonElement e) {
            var lhs = this.VisitMemberAccessOperandElement(e[1]);
            var rhs = this.VisitPropertyOperandElement(e[1], e[2]);
            return Expression.LessThan(lhs, rhs);
        }

        private Expression VisitGreaterExpressionElement(JsonElement e) {
            var lhs = this.VisitMemberAccessOperandElement(e[1]);
            var rhs = this.VisitPropertyOperandElement(e[1], e[2]);
            return Expression.GreaterThan(lhs, rhs);
        }


        private Expression VisitInListExpressionElement(JsonElement e) {
            var lhs = this.VisitMemberAccessOperandElement(e[1]);
            var self = this;
            var args = e[2].EnumerateArray().Select(x =>
                Expression.Equal(lhs, self.VisitConstantExpressionElement(x))
            );
            return args.Aggregate((x, y) => Expression.OrElse(x, y));
            // var rhs = Expression.Constant(e[2].EnumerateArray().ToArray());
            // return Expression.Call(rhs, "Contains", Type.EmptyTypes, lhs);
        }

        private Expression VisitNotInListExpressionElement(JsonElement e) {
            var lhs = this.VisitMemberAccessOperandElement(e[1]);
            var rhs = e[2].EnumerateArray();
            return Expression.Not(Expression.Call(lhs, "Contains", Type.EmptyTypes, lhs));
        }

        private Expression VisitVectorExpressionElement(JsonElement element) {
            using var iter = element.EnumerateArray();
            var constants = iter.Select(VisitConstantExpressionElement);
            var args = iter.Select(x => Expression.Constant(true) as Expression);
            return args.Aggregate((x, y) => Expression.AndAlso(x, y));
        }

        private Expression VisitConstantExpressionElement(JsonElement element ) =>
            element.ValueKind switch {
                JsonValueKind.String => Expression.Constant(element.GetString()),
                JsonValueKind.Number => Expression.Constant(element.GetDecimal()),
                JsonValueKind.True => Expression.Constant(true),
                JsonValueKind.False => Expression.Constant(false),
                JsonValueKind.Null => Expression.Constant(null),
                _ => throw new ParsingErrorException(),
            };

        private Expression AccessProperties(IEnumerable<string> props) {
            var expr = Expression.PropertyOrField(_lhs, props.First());
            foreach (var p in props.Skip(1)) {
                expr = Expression.Property(expr, p);
            }
            return expr;
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
