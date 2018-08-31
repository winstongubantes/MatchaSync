using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Matcha.Sync.Mobile
{
    public static class WhereBuilder
    {
        public static WherePart ToOdata<T>(Expression<Func<T, bool>> expression)
        {
            var i = 1;
            return Recurse(ref i, expression.Body, isUnary: true);
        }

        private static WherePart Recurse(ref int i, Expression expression, bool isUnary = false, string prefix = null, string postfix = null)
        {
            if (expression is UnaryExpression)
            {
                var unary = (UnaryExpression)expression;
                return WherePart.Concat(NodeTypeToString(unary.NodeType), Recurse(ref i, unary.Operand, true));
            }
            if (expression is BinaryExpression)
            {
                var body = (BinaryExpression)expression;
                return WherePart.Concat(Recurse(ref i, body.Left), NodeTypeToString(body.NodeType), Recurse(ref i, body.Right));
            }
            if (expression is ConstantExpression)
            {
                var constant = (ConstantExpression)expression;
                var value = constant.Value;
                if (value is int)
                {
                    return WherePart.IsOData(value.ToString());
                }
                if (value is string)
                {
                    value = $"'{(string)value}'";
                }
                if (value is bool && isUnary)
                {
                    return WherePart.Concat(WherePart.IsParameter(i++, value), "eq", WherePart.IsOData("1"));
                }
                return WherePart.IsParameter(i++, value);
            }
            if (expression is MemberExpression)
            {
                var member = (MemberExpression)expression;

                if (member.Member is PropertyInfo)
                {
                    var property = (PropertyInfo)member.Member;
                    var colName = property.Name;
                    if (isUnary && member.Type == typeof(bool))
                    {
                        return WherePart.Concat(Recurse(ref i, expression), "eq", WherePart.IsParameter(i++, "true"));
                    }

                    return WherePart.IsOData($"{colName}");
                }
                if (member.Member is FieldInfo)
                {
                    var value = GetValue(member);
                    if (value is string)
                    {
                        value = prefix + (string)value + postfix;
                    }
                    return WherePart.IsParameter(i++, value);
                }
                throw new Exception($"Expression does not refer to a property or field: {expression}");
            }
            if (expression is MethodCallExpression)
            {
                var methodCall = (MethodCallExpression)expression;
                // LIKE queries:
                if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                {
                    return WherePart.Concat(Recurse(ref i, methodCall.Object), "contains", Recurse(ref i, methodCall.Arguments[0], prefix: "(", postfix: ")"));
                }
                if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                {
                    return WherePart.Concat(Recurse(ref i, methodCall.Object), "startswith", Recurse(ref i, methodCall.Arguments[0], prefix: "(", postfix: ")"));
                }
                if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                {
                    return WherePart.Concat(Recurse(ref i, methodCall.Object), "endswith", Recurse(ref i, methodCall.Arguments[0], prefix: "(", postfix: ")"));
                }
                // IN queries:
                if (methodCall.Method.Name == "Contains")
                {
                    Expression collection;
                    Expression property;
                    if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
                    {
                        collection = methodCall.Arguments[0];
                        property = methodCall.Arguments[1];
                    }
                    else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
                    {
                        collection = methodCall.Object;
                        property = methodCall.Arguments[0];
                    }
                    else
                    {
                        throw new Exception("Unsupported method call: " + methodCall.Method.Name);
                    }

                    var values = (IEnumerable)GetValue(collection);
                    return WherePart.Concat(Recurse(ref i, property), "IN", WherePart.IsCollection(ref i, values));
                }
                throw new Exception("Unsupported method call: " + methodCall.Method.Name);
            }
            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        public static string ValueToString(object value, bool isUnary, bool quote)
        {
            if (value is bool)
            {
                if (isUnary)
                {
                    return (bool)value ? "(1=1)" : "(1=0)";
                }
                return (bool)value ? "1" : "0";
            }
            return value.ToString();
        }

        private static object GetValue(Expression member)
        {
            // source: http://stackoverflow.com/a/2616980/291955
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        private static string NodeTypeToString(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.And:
                    return "and";
                case ExpressionType.AndAlso:
                    return "and";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return "eq";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.GreaterThan:
                    return "gt";
                case ExpressionType.GreaterThanOrEqual:
                    return "gt";
                case ExpressionType.LessThan:
                    return "lt";
                case ExpressionType.LessThanOrEqual:
                    return "le";
                case ExpressionType.Modulo:
                    return "mod";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Negate:
                    return "-";
                case ExpressionType.Not:
                    return "not";
                case ExpressionType.NotEqual:
                    return "ne";
                case ExpressionType.Or:
                    return "or";
                case ExpressionType.OrElse:
                    return "or";
                case ExpressionType.Subtract:
                    return "-";
            }
            throw new Exception($"Unsupported operation type: {nodeType}");
        }
    }

    public class WherePart
    {
        public string ODataQuery { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public static WherePart IsOData(string odata)
        {
            return new WherePart()
            {
                Parameters = new Dictionary<string, object>(),
                ODataQuery = odata
            };
        }

        public static WherePart IsParameter(int count, object value)
        {
            return new WherePart()
            {
                Parameters = { {count.ToString(), value } },
                ODataQuery = $"@{count}"
            };
        }

        public static WherePart IsCollection(ref int countStart, IEnumerable values)
        {
            var parameters = new Dictionary<string, object>();
            var sql = new StringBuilder();
            foreach (var value in values)
            {
                parameters.Add((countStart).ToString(), value);
                sql.Append($"@{countStart},");
                countStart++;
            }
            if (sql.Length == 1)
            {
                sql.Append("null,");
            }
            //sql[sql.Length - 1] = '';
            return new WherePart()
            {
                Parameters = parameters,
                ODataQuery = sql.ToString()
            };
        }

        public static WherePart Concat(string @operator, WherePart operand)
        {
            return new WherePart()
            {
                Parameters = operand.Parameters,
                ODataQuery = $"{@operator} ({operand.ODataQuery})"
            };
        }

        public static WherePart Concat(WherePart left, string @operator, WherePart right)
        {
            return new WherePart()
            {
                Parameters = left.Parameters.Union(right.Parameters).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                ODataQuery = $"{left.ODataQuery} {@operator} {right.ODataQuery}"
            };
        }

        public override string ToString()
        {
            var query = ODataQuery;
            foreach (var parameter in Parameters)
            {
                query = query.Replace($"@{parameter.Key}", parameter.Value.ToString());
            }

            return query;
        }
    }
}
