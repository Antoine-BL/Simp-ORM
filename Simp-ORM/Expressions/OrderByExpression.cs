using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SimpORM.Sql
{
    public class OrderByExpression<T, TR>
    {
        public OrderByExpression(Expression<Func<T, TR>> expression, bool ascending = true)
        {
            Expression = expression;
            IsAscending = ascending;
        }

        public Expression<Func<T, TR>> Expression { get; set; }
        public bool IsAscending { get; set; }
    }
}
