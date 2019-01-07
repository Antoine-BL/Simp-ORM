using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SimpORM.Sql
{
    public interface IDbAdapter
    {
        IList<T> SelectAllInTable<T>();
        IList<T> SelectWhere<T>(Expression<Func<T, bool>> predicate);
        T InsertRow<T>(T entity);
        T UpdateRow<T>(T entity);
        bool DeleteWhere<T>(Expression<Func<T, bool>> predicate);
        object ExecuteExpressionQuery<T>(IExpressionQuery<T> query);
    }
}