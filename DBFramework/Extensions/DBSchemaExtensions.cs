﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DBF
{ 
    /// <summary>
    /// Object to hold all the extension functions for the <see cref="PropertyInfo"/> needed in <see cref="DBF"/>
    /// </summary>
    public static class DBSchemaExtensions
    {
        #region Primary Key Extensions

        /// <summary>
        /// Gets wether the <see cref="PropertyInfo"/> given is the Primary Key in the given Schema
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> to check</param>
        /// <param name="Schema">The Schema to use</param>
        /// <param name="type">The model for wich we're running the action</param>
        /// <returns></returns>
        public static bool IsPrimaryKey(this DBSchema Schema, PropertyInfo propertyInfo, Type model)
        {
            var output = Schema.GetType().GetMethod("Model").MakeGenericMethod(model).Invoke(Schema, null);
            if ((PropertyInfo)output.GetType().GetProperty("PrimaryKey").GetValue(output) == propertyInfo)
                return true;

            return false;
        }

        /// <summary>
        /// Get the primary key of a given <see cref="Type"/>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static PropertyInfo GetPrimaryKey(this DBSchema schema, Type model)
        {
            // Get the DBTable containing the constraints for the dbcontext
            var table = schema.GetType().GetMethod("Model").MakeGenericMethod(model).Invoke(schema, null);

            // Return the type of the primary key
            return (PropertyInfo)table.GetType().GetProperty("PrimaryKey").GetValue(table);
        }

        /// <summary>
        /// Gets wether the value of the primary key of the given object is set based on a schema
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <param name="Schema">The schema to check against</param>
        /// <returns></returns>
        public static bool HasPrimaryKeyValue(this DBSchema Schema, object obj)
        {
            foreach (var prop in obj.GetType().GetProperties())
                if (Schema.IsPrimaryKey(prop, obj.GetType()))
                    if (prop.GetValue(obj) != null)
                        return true;

            return false;
        }

        /// <summary>
        /// Gets the value of the primary key property on a given object based on a Schema
        /// </summary>
        /// <param name="obj">The object to get the property value of the primary key from</param>
        /// <param name="Schema">The schema to check the primary key against</param>
        /// <returns><see cref="null"/> if the given object's primary key is the value of <see cref="null"/></returns>
        public static object GetPrimaryKeyValue(this DBSchema Schema, object obj)
        {
            foreach (var prop in obj.GetType().GetProperties())
                if (Schema.IsPrimaryKey(prop, obj.GetType()))
                    return prop.GetValue(obj);

            // Should never be reached
            return null;
        }

        #endregion

        #region Is Required Extensions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Schema"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool RequiredValuesCorrect(this DBSchema Schema, object obj)
        {
            bool retval = true;

            var output = Schema.GetType().GetMethod("Model").MakeGenericMethod(obj.GetType()).Invoke(Schema, null);
            HashSet<MemberInfo> requiredProps = (HashSet<MemberInfo>)output.GetType().GetProperty("RequiredProperties").GetValue(output);
            foreach (var prop in obj.GetType().GetProperties())
                if (requiredProps.Contains(prop))
                    if((requiredProps.Where(m => m == prop).First() as PropertyInfo)?.GetValue(obj) == null)
                        retval = false;

            return retval;
        }

        #endregion
    }
}
