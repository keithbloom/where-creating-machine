using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Where.Machine.Core
{
    public class BinaryExpressionVisitor : ExpressionVisitor
    {
        StringBuilder _clause = new StringBuilder();
        readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        public void Start()
        {
            _clause = new StringBuilder();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _clause.Append("(");


            Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    _clause.Append(" = ");
                    break;
                case ExpressionType.GreaterThan:
                    _clause.Append(" > ");
                    break;
                case ExpressionType.OrElse:
                    _clause.Append(" OR ");
                    break;
                case ExpressionType.AndAlso:
                    _clause.Append(" AND ");
                    break;
                case ExpressionType.LessThan:
                    _clause.Append(" < ");
                    break;
                case ExpressionType.NotEqual:
                    _clause.Append(" != ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _clause.Append(" >= ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _clause.AppendLine(" <= ");
                    break;
                default:
                    throw new InvalidOperationException("Don't have this binary expression type : " + node.NodeType);
            }

            Visit(node.Right);

            _clause.Append(")");
            return node;
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (!IsSimpleType(node.Type))
            {
                return node;
            }

            var value = string.Format("@p{0}", _parameters.Count);

            _clause.Append(value);
            AddParam(value, node.Value);
            return node;
        }


        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            // Recurse down to see if we can simplify...
            var expression = Visit(memberExpression.Expression);

            // If we've ended up with a constant, and it's a property or a field,
            // we can simplify ourselves to a constant
            if (expression is ConstantExpression)
            {
                object container = ((ConstantExpression)expression).Value;

                var member = memberExpression.Member;
                if (member is FieldInfo)
                {
                    object value = ((FieldInfo)member).GetValue(container);
                    if (IsSimpleType(memberExpression.Type))
                    {
                        AddClause(value);
                    }
                    return Expression.Constant(value);
                }
                if (member is PropertyInfo)
                {
                    object value = ((PropertyInfo)member).GetValue(container, null);
                    AddClause(value);

                    return Expression.Constant(value);
                }
            }
            else
            {
                _clause.Append(memberExpression.Member.Name);
            }
            return base.VisitMember(memberExpression);

        }

        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(Enum);
        }

        void AddClause(object value)
        {
            var paramName = string.Format("@p{0}", _parameters.Count);

            _clause.Append(paramName);
            AddParam(paramName, value);
        }


        public string MakeClause()
        {
            return _clause.ToString();
        }

        public Dictionary<string, object> GetParameters()
        {
            return _parameters;
        }

        public void AddParam(string key, object value)
        {
            _parameters.Add(key, value);
        } 
    }
}