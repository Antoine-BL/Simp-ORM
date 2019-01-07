using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using SimpORM.Sql;

namespace SimpORM.Dao
{
    public class GenericDao<T> : IDao<T, int>
    {
        private IDbAdapter dbAdapter;

        public GenericDao(IDbAdapter dbAdapter)
        {
            this.dbAdapter = dbAdapter;
        }

        public IList<T> FindAll()
        {
            return dbAdapter.SelectAllInTable<T>();
        }

        public IList<T> Find(T example)
        {
            Expression<Func<T, bool>> whereClause = WhereFromExample(example);

            return dbAdapter.SelectWhere(whereClause);
        }

        public T Save(T entity)
        {
            if (Exists(entity))
            {
                return dbAdapter.UpdateRow(entity);
            }
            else
            {
                return dbAdapter.InsertRow(entity);
            }
        }

        public bool Delete(T entity)
        {
            entity = cloneAndSetId(entity);

            return dbAdapter.DeleteWhere(WhereFromExample(entity));
        }

        public bool Exists(T example)
        {
            example = cloneAndSetId(example);
            if (hasNullId(example)) return false;

            IList<T> correspondingEntities = Find(example);

            return correspondingEntities.Count > 0;
        }

        private bool hasNullId(T example)
        {
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                object propVal = prop.InvokeGetOn(example);
                if (prop.IsIdProp() && (propVal == null || propVal == prop.PropertyType.GetDefault()))
                {
                    return true;
                }
            }

            return false;
        }

        private T cloneAndSetId(T entity)
        {
            ConstructorInfo constructor = typeof(T).GetConstructor(new Type[0]);
            T newEntity = (T)constructor.Invoke(new object[0]);

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (prop.IsIdProp())
                {
                    prop.InvokeSetOn(newEntity, prop.InvokeGetOn(entity));
                }
            }

            return newEntity;
        }

        private Expression<Func<T, bool>> WhereFromExample(T example)
        {
            Expression whereBody = null;

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                object propValue = prop.InvokeGetOn(example);

                if (propValue != null && !propValue.Equals(prop.PropertyType.GetDefault()))
                {
                    if (whereBody == null)
                    {
                        whereBody = prop.ToPropComparisonExpression<T>(propValue);
                    }
                    else
                    {
                        whereBody = Expression.AndAlso(whereBody, prop.ToPropComparisonExpression<T>(propValue));
                    }
                }
            }

            if (whereBody == null) return ent => true;

            ParameterExpression inputParam = Expression.Parameter(typeof(T), typeof(T).Name);
            Expression<Func<T, bool>> whereClause = Expression.Lambda<Func<T, bool>>(whereBody, inputParam);

            return whereClause;
        }
    }
}
