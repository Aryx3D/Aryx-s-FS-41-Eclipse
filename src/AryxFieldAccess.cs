using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Aryx_NavalInterceptor1
{
    internal static class AryxFieldAccess
    {
        //experiment in optimisation for future shit;
        //build once at type initialization
        //runtime reads thus avoid reflection and value-type boxing.
        internal static Func<T, TValue> CreateGetter<T, TValue>(FieldInfo field)
        {
            ParameterExpression instance = Expression.Parameter(typeof(T), "instance");
            return Expression.Lambda<Func<T, TValue>>(Expression.Field(instance, field), instance).Compile();
        }
    }
}
