using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpORM.Sql
{
    public static class ExpressionsToSql
    {
        private static readonly Dictionary<string, string> conversionDictionary = new Dictionary<string, string>
        {
            { "==", "=" },
            { "AndAlso", "AND" },
            { "OrElse", "OR" },
            { ".Value", "" },
            { "True", "1" },
            { "False", "0" },
            { "\"", "\'" },
        };

        private static readonly Dictionary<string, string> SupportedDateTimePatterns = new Dictionary<string, string>
        {
            {@"\d{4}-\d{2}-\d{2} \d{1,2}:\d{2}:\d{2} ([AP]M)", "yyyy-MM-dd h:mm:ss tt"},
            {@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}:\d{2}", "dd/MM/yyyy HH:mm:ss"},
            {@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", "yyyy-MM-dd HH:mm:ss"}
        };

        public static string ToWhereClause<T>(this Expression<Func<T, bool>> predicate)
        {
            var literalized = (Expression<Func<T, bool>>)new Literalizer().Visit(predicate);

            string whereClause = literalized.Body.ToString();
            string paramName = predicate.Parameters.First().Name;

            string pattern = $@"\b{paramName}\b";
            Regex rexp = new Regex(pattern);

            foreach (var combo in SupportedDateTimePatterns)
            {
                string regex = combo.Key;
                string correspondingFormat = combo.Value;

                if (Regex.IsMatch(whereClause, regex))
                {
                    whereClause = Regex.Replace(whereClause, regex, match => ReformatDateAndWrapWithApostrophes(match, correspondingFormat));
                    break;
                }
            }

            whereClause = rexp.Replace(whereClause, typeof(T).GetTableMapping());
            whereClause = MakeStandardConversions(new StringBuilder(whereClause)).ToString();

            return whereClause;
        }

        private static string ReformatDateAndWrapWithApostrophes(Match match, string format)
        {
            DateTime date = DateTime.ParseExact(match.Value, format, CultureInfo.InvariantCulture);

            return "'" + date.ToString("yyyyMMdd hh:mm:ss tt", CultureInfo.InvariantCulture) + "'";
        }

        private static StringBuilder MakeStandardConversions(StringBuilder builder)
        {
            foreach (var entry in conversionDictionary)
            {
                builder.Replace(entry.Key, entry.Value);
            }

            return builder;
        }

        public static string ToOrderBy<T, TR>(this OrderByExpression<T, TR> orderByExpression)
        {
            var literalized = (Expression<Func<T, object>>)new Literalizer().Visit(orderByExpression.Expression);
            LambdaExpression convertStripped = StripConvert(literalized);

            StringBuilder orderBy = new StringBuilder(convertStripped.Body.ToString());
            string paramName = orderByExpression.Expression.Parameters.First().Name;

            orderBy.Replace(paramName, typeof(T).GetTableMapping());
            orderBy = MakeStandardConversions(orderBy);
            orderBy.Append(orderByExpression.IsAscending ? " ASC" : " DESC");

            return orderBy.ToString();
        }

        internal class Literalizer : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member.DeclaringType.IsDefined(typeof(CompilerGeneratedAttribute), false)
                    && node.Expression.NodeType == ExpressionType.Constant)
                {
                    object target = ((ConstantExpression)node.Expression).Value;
                    object value;
                    switch (node.Member.MemberType)
                    {
                        case MemberTypes.Property:
                            PropertyInfo propInf = (PropertyInfo)node.Member;
                            value = propInf.GetValue(target, null);
                            break;
                        case MemberTypes.Field:
                            value = ((FieldInfo)node.Member).GetValue(target);
                            break;
                        default:
                            value = target = null;
                            break;
                    }
                    if (target != null) return Expression.Constant(value, node.Type);
                }

                return base.VisitMember(node);
            }
        }

        public static LambdaExpression StripConvert<T>(Expression<Func<T, object>> source)
        {
            Expression result = source.Body;
            while (((result.NodeType == ExpressionType.Convert)
                    || (result.NodeType == ExpressionType.ConvertChecked))
                   && (result.Type == typeof(object)))
            {
                result = ((UnaryExpression)result).Operand;
            }
            return Expression.Lambda(result, source.Parameters);
        }
    }
}
