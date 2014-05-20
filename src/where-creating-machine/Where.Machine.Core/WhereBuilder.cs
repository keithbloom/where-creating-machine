using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Where.Machine.Core
{
    public class WhereBuilder<T>
    {
        readonly List<Expression<Func<T, bool>>> _whereClauses = new List<Expression<Func<T, bool>>>();
        readonly  BinaryExpressionVisitor _visitor = new BinaryExpressionVisitor();

        public void Add(Expression<Func<T, bool>> expression)
        {
            _whereClauses.Add(expression);
        }

        public override string ToString()
        {
            return Build();
        }

        public string Build()
        {
            var builder = new StringBuilder();
            var things = new List<string>();
            builder.Append("WHERE ");

            foreach (var whereClause in _whereClauses)
            {
                _visitor.Start();
                _visitor.Visit(whereClause.Body);
                things.Add(_visitor.MakeClause());
            }

            if (!things.Any())
            {
                return string.Empty;
            }

            builder.AppendLine(things.Aggregate((x, y) => x + " AND " + y));

            return builder.ToString();
        }

        public IDictionary<string, object> GetParamsForTesting()
        {
            return _visitor.GetParameters();
        }

        public void AddParam(string key, object value)
        {
            _visitor.AddParam(key, value);
        }

        public object Params()
        {
            IDictionary<string, object> dictionary = new ExpandoObject();
            foreach (var parameter in _visitor.GetParameters())
            {
                dictionary.Add(parameter);
            }
            return dictionary;

        }
 
    }
}