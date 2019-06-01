using System;

namespace DBF
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Get the default value of a type as an object
        /// </summary>
        /// <param name="type">The type to get the default value for</param>
        /// <returns></returns>
        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            if (type == typeof(string))
                return "";
            if (type == typeof(bool?))
                return null;
            return null;
        }
    }
}
