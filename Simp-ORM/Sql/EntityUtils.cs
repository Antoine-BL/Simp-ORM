using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SimpORM.Sql
{
    static class EntityUtils
    {
        public static PropertyInfo GetIdProp(this Type t)
        {
            PropertyInfo[] properties = t.GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                if (prop.GetCustomAttribute(typeof(Id)) != null)
                {
                    return prop;
                }
            }

            throw new ArgumentException("Entity has no Id property");
        }

        public static PropertyInfo GetIdProp<T>()
        {
            return typeof(T).GetIdProp();
        }

        public static PropertyInfo GetIdProp<T>(this T entity)
        {
            return typeof(T).GetIdProp();
        }

        public static object GetId<T>(this T entity)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                if (prop.GetCustomAttribute(typeof(Id)) != null)
                {
                    return prop.GetMethod.Invoke(entity, new object[0]);
                }
            }

            throw new ArgumentException("Entity has no Id property");
        }

        public static void SetId<T>(this T entity, object value)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                if (prop.GetCustomAttribute(typeof(Id)) != null)
                {
                    prop.SetMethod.Invoke(entity, new[] { value });
                    return;
                }
            }

            throw new ArgumentException("Entity has no Id property");
        }

        public static string GetColMapping(this PropertyInfo property)
        {
            ColumnMapping mapping = (ColumnMapping)property.GetCustomAttribute(typeof(ColumnMapping));
            
            if (mapping == null) throw new ArgumentException("Property has no mapped column", nameof(property));

            return mapping.ColumnName;
        }

        public static string GetTableMapping(this Type type)
        {
            TableMapping mappingAttr = (TableMapping)type.GetCustomAttribute(typeof(TableMapping));

            if (mappingAttr == null) return $"{type.Name}s";

            return (mappingAttr).TableName;
        }

        public static object CastVarToPropType(this PropertyInfo prop, object varToConvert)
        {
            return prop.PropertyType.CastVarToType(varToConvert);
        }

        public static object CastVarToType(this Type type, object varToConvert)
        {
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = Nullable.GetUnderlyingType(type);
            }

            var parameter = Expression.Parameter(typeof(object), "input");

            var cast = Expression.TypeAs(Expression.Convert(parameter, type), typeof(object));

            var lambda = Expression.Lambda<Func<object, object>>(cast, parameter);

            var func = lambda.Compile();

            return func(varToConvert);
        }

        public static object InvokeGetOn(this PropertyInfo prop, object entity)
        {
            return prop.GetMethod.Invoke(entity, new object[0]);
        }

        public static object InvokeSetOn(this PropertyInfo prop, object entity, object value)
        {
            return prop.SetMethod.Invoke(entity, new[] {value});
        }

        public static string GetIdCol(this Type type)
        {
            PropertyInfo[] props = type.GetProperties();

            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute(typeof(Id)) != null)
                    return prop.GetColMapping();
            }

            throw new ArgumentException("No Id Column mapped on entity " + type.FullName);
        }

        public static SqlParameter ToParam(this PropertyInfo prop, object entity, string paramName)
        {
            object value = prop.InvokeGetOn(entity);

            if (value != null)
                return new SqlParameter(paramName, value);
            else
                return new SqlParameter(paramName, DBNull.Value);
        }

        public static SqlParameter ToParam(this PropertyInfo prop, object entity)
        {
            return prop.ToParam(entity, $"@{prop.GetColMapping()}");
        }

        public static Expression<Func<T, bool>> IdEqualsExpression<T>(object val)
        {
            return ToPropComparisonPredicate<T>(typeof(T).GetIdProp(), typeof(T).GetIdProp().CastVarToPropType(val));
        }

        public static Expression<Func<T, bool>> ToPropComparisonPredicate<T>(PropertyInfo prop, object val)
        {
            ParameterExpression inputParam = Expression.Parameter(typeof(T), typeof(T).Name);

            BinaryExpression comparison = prop.ToPropComparisonExpression<T>(val);

            Expression<Func<T, bool>> result = Expression.Lambda<Func<T, bool>>(comparison, inputParam);

            return result;
        }

        public static BinaryExpression ToPropComparisonExpression<T>(this PropertyInfo prop, object val)
        {
            ParameterExpression inputParam = Expression.Parameter(typeof(T), typeof(T).Name);
            Expression propAccess = Expression.Property(inputParam, prop);
            BinaryExpression comparison;

            if (val == null)
            {
                comparison = Expression.Equal(propAccess, Expression.Constant(null, propAccess.Type));
            }
            else
            {
                var castedVal = prop.CastVarToPropType(val);
                comparison = Expression.Equal(propAccess, Expression.Constant(castedVal, propAccess.Type));
            }

            return comparison;
        }

        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
            {
                if (type == typeof(DateTime))
                    return (DateTime)SqlDateTime.MinValue;
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static bool IsIdProp(this PropertyInfo prop)
        {
            return prop.GetCustomAttribute(typeof(Id)) != null;
        }

        public static List<T> ToList<T>(this IDataReader dataReader)
        {
            List<T> list = new List<T>();

            while (dataReader.Read())
            {
                list.Add(ConvertRowToEntity<T>(dataReader));
            }

            return list;
        }

        public static T ConvertRowToEntity<T>(IDataReader row)
        {
            try
            {
                ConstructorInfo defaultConstructor = typeof(T).GetConstructor(new Type[0]);
                T entity = (T) defaultConstructor.Invoke(new object[0]);

                entity = MapColumnValuesToEntity(entity, row);

                return entity;
            }
            catch (NullReferenceException)
            {
                throw new ArgumentException("Specified entity has no default constructor");

            }
        }

        public static T MapColumnValuesToEntity<T>(T entity, IDataReader row)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                string mappedColumn = prop.GetColMapping();

                var paramVal = row[mappedColumn];
                if (paramVal is DBNull) continue;

                object[] parameters = { prop.CastVarToPropType(paramVal) };

                prop.SetMethod.Invoke(entity, parameters);
            }

            return entity;
        }
    }
}
