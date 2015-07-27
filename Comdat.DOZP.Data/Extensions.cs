using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Comdat.DOZP.Data
{
    public static class Extensions
    {
        public static bool In<T>(this T source, params T[] list)
        {
            return (list as IList<T>).Contains(source);
        }

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> list, string sortExpression)
        {
            sortExpression += "";
            string[] parts = sortExpression.Split(' ');
            string property = "";
            bool descending = false;

            if (parts.Length > 0 && parts[0] != "")
            {
                property = parts[0];

                if (parts.Length > 1)
                {
                    descending = parts[1].ToUpper().Contains("DESC");
                }

                PropertyInfo prop = typeof(T).GetProperty(property);

                if (prop == null)
                {
                    throw new Exception(String.Format("No property '{0}' in + '{1}'", property, typeof(T).Name));
                }

                if (descending)
                    return list.OrderByDescending(x => prop.GetValue(x, null));
                else
                    return list.OrderBy(x => prop.GetValue(x, null));
            }

            return list;
        }

        //public static void DeleteObjects<T>(this DbContext context, IEnumerable<T> query)
        //{
        //    IEnumerator enumerator = query.GetEnumerator();
        //    while (enumerator.MoveNext())
        //    {
        //        T obj = (T)enumerator.Current;
        //        context.Entry(obj).State = EntityState.Deleted;
        //    }
        //}

        //public static void DeleteObjects<T>(this DbContext context, IList<T> query)
        //{
        //    IEnumerator enumerator = query.GetEnumerator();
        //    while (enumerator.MoveNext())
        //    {
        //        T obj = (T)enumerator.Current;
        //        context.DeleteObject(obj);
        //    }
        //}

        //public static void DeleteObjects<T>(this DbContext context, IQueryable<T> query)
        //{
        //    IEnumerator enumerator = query.GetEnumerator();
        //    while (enumerator.MoveNext())
        //    {
        //        T obj = (T)enumerator.Current;
        //        context.DeleteObject(obj);
        //    }
        //}
    }
}
