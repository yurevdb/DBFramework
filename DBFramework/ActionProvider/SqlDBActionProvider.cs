using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DBF
{
    /// <summary>
    /// Implementation for the interaction logic between the <see cref="DBContext"/> and a Sql Server Database Engine
    /// </summary>
    public class SqlDBActionProvider : DBActionProvider
    {
        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SqlDBActionProvider() : base() { }

        /// <summary>
        /// Paramerized constructor
        /// <paramref name="ConnectionString">The Connectionstring to access the database</paramref>
        /// </summary>
        public SqlDBActionProvider(string ConnectionString) : base(ConnectionString) { }

        #endregion

        #region Private Implementations

        /// <summary>
        /// Gets the <see cref="List{T}"/> from a sql database
        /// </summary>
        /// <typeparam name="TModel">The type of data to get from the corresponding table in the <see cref="DBContext"/></typeparam>
        /// <param name="predicate">An <see cref="Action"/> to set the where clause for the select from the sql database</param>
        /// <returns></returns>
        public async override Task<DBSet<TModel>> Fetch<TModel>(Action<TModel> predicate = null)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // Get the name of the database table
                    string table = (typeof(TModel).CustomAttributes.Count() > 0) ? (typeof(TModel).GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : typeof(TModel).Name;

                    // Get the properties of the given object type
                    string columns = "";
                    List<string> dbcolumns = new List<string>();
                    var props = typeof(TModel).GetProperties();
                    foreach (var prop in props)
                        dbcolumns.Add((prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name);
                    var count = 0;
                    foreach (string col in dbcolumns)
                    {
                        columns += $"{col}";
                        count++;
                        if (count < dbcolumns.Count)
                            columns += ", ";
                    }

                    // Get the where state from the predicate if given
                    string where = null;
                    if (predicate != null)
                    {
                        TModel item = new TModel();
                        predicate(item);
                        foreach (var prop in item.GetType().GetProperties())
                        {
                            if (prop.GetValue(item) == null) continue;

                            var value = prop.GetValue(item);

                            // If there is already a where clause
                            // Add a "AND"
                            if (where != null) where += " AND ";

                            var propname = (prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name;
                            var propEquater = "";
                            switch (value.GetType().ToString())
                            {
                                case "System.Guid":
                                case "System.String":
                                    propEquater = $"'{value}'";
                                    break;
                                case "System.Boolean":
                                    var v = (bool)value ? 1 : 0;
                                    propEquater = $"{v}";
                                    break;
                                default:
                                    propEquater = $"{value}";
                                    break;
                            }
                            where += $"{propname} = {propEquater}";
                        }
                    }

                    // Generate the query
                    string query = $"SELECT {columns} FROM {table} WHERE {where ?? "1 = 1"}";

                    DBSet<TModel> retval = new DBSet<TModel>();

                    using (SqlConnection connection = new SqlConnection(_ConnectionString))
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = query;
                        command.Connection = connection;
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TModel item = new TModel();
                                foreach (var prop in props)
                                {
                                    string propname = (prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name;
                                    var value = reader[$"{propname}"];
                                    if (value.GetType() == typeof(string))
                                        value = (value as string).Trim();
                                    if (value.GetType() == typeof(DBNull))
                                        value = null;
                                    prop.SetValue(item, value);
                                }

                                // Add the item to the remote and local lists of the DBSet
                                retval.Add(item);
                            }
                        }
                    }

                    // Return the requested data
                    return retval;
                });
            }
            catch (Exception ex)
            {
                throw new DBActionProviderException("The action provide could not fetch any data", ex);
            }
        }

        /// <summary>
        /// Pushes the given item of type <typeparamref name="TModel"/> to the sql database
        /// </summary>
        /// <typeparam name="TModel">The model corresponding to the database table</typeparam>
        /// <param name="item">The item to push to the sql database</param>
        /// <returns></returns>
        public async override Task Push<TModel>(TModel item)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Get all the properties to insert into the database
                    List<(string, object)> dbvalues = new List<(string, object)>();
                    // TODO: Ignore the ignore properties
                    foreach (var prop in item.GetType().GetProperties())
                        dbvalues.Add(((prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name, prop.GetValue(item)));

                    // TODO: add constraint checking here
                    // ------------------------------------------------------------
                    // If the options don't allow unsafe code and the where is null
                    // Throw an excpetion
                    if (!Schema.HasPrimaryKeyValue(item)) throw new Exception("The DBContext does not allow unsafe code and the action that would be ran is considered unsafe\r\nAction: trying to delete without setting the where with primary key as unique identifier");
                    if (!Schema.RequiredValuesCorrect(item)) throw new Exception("Required values cannot be null");

                    // Generate the insert query
                    string table = (item.GetType().CustomAttributes.Count() > 0) ? (item.GetType().GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : item.GetType().Name;
                    string columns = "";
                    string values = "";
                    int count = 0;
                    foreach (var val in dbvalues)
                    {
                        columns += $"{val.Item1}";
                        if (val.Item2 == null)
                            values += "NULL";
                        else
                            switch (val.Item2.GetType().ToString())
                            {
                                case "System.Guid":
                                case "System.String":
                                    values += $"'{val.Item2}'";
                                    break;
                                case "System.Boolean":
                                    var v = (bool)val.Item2 ? 1 : 0;
                                    values += $"{v}";
                                    break;
                                case "System.DateTimeOffset":
                                    string dtFormat = "yyyy-MM-dd HH:mm:ss.fffffff zzz";
                                    DateTimeOffset dt = DateTimeOffset.Parse(val.Item2.ToString());
                                    values += $"'{dt.ToString(dtFormat)}'";
                                    break;
                                default:
                                    values += $"{val.Item2}";
                                    break;
                            }
                        if (++count == dbvalues.Count) continue;
                        columns += ", ";
                        values += ", ";
                    }

                    string query = $"INSERT INTO {table} ({columns}) VALUES ({values})";

                    using (SqlConnection connection = new SqlConnection(_ConnectionString))
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = query;
                        command.Connection = connection;
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new DBActionProviderException("The action provider could not push the given item to the database", ex);
            }
        }

        /// <summary>
        /// Removes the given item from the sql database based on the primary key value
        /// </summary>
        /// <typeparam name="TModel">The model of the item corresponding to the table from the sql database</typeparam>
        /// <param name="predicate">An <see cref="Action"/> to set the where clause for the removal of the item/items from the sql database</param>
        /// <returns></returns>
        public async override Task Remove<TModel>(Action<TModel> predicate)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Create the item to remove
                    TModel item = new TModel();
                    predicate(item);

                    bool HasPrimaryKey = false;

                    // Get the where state from the predicate if given
                    string where = null;
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        if (prop.GetValue(item) == null) continue;

                        var value = prop.GetValue(item);

                        // Check the primary key
                        if (!HasPrimaryKey) HasPrimaryKey = Schema.IsPrimaryKey(prop, typeof(TModel));

                        // If there is already a where clause
                        // Add a "AND"
                        if (where != null) where += " AND ";

                        var propname = (prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name;
                        var propEquater = "";
                        switch (value.GetType().ToString())
                        {
                            case "System.Guid":
                            case "System.String":
                                propEquater = $"'{value}'";
                                break;
                            case "System.Boolean":
                                var v = (bool)value ? 1 : 0;
                                propEquater = $"{v}";
                                break;
                            default:
                                propEquater = $"{value}";
                                break;
                        }

                        // Update the where clause
                        where += $"{propname} = {propEquater}";

                        // Break from the iterator
                        break;
                    }

                    // If the options don't allow unsafe code and the where is null
                    // Throw an excpetion
                    if (!HasPrimaryKey) throw new Exception("The DBContext does not allow unsafe code and the action that would be ran is considered unsafe\r\nAction: trying to delete without setting the where with primary key as unique identifier");

                    // Get the name of the database table
                    string table = (item.GetType().CustomAttributes.Count() > 0) ? (item.GetType().GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : item.GetType().Name;

                    // Create the query
                    string query = $"DELETE FROM {table} WHERE {where ?? "1 = 1"}";

                    using (SqlConnection connection = new SqlConnection(_ConnectionString))
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = query;
                        command.Connection = connection;
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new DBActionProviderException("The action provider could not remove the item/items based on the given predicate", ex);
            }
        }

        /// <summary>
        /// Updates an item on the sql database based on the predicate
        /// the new value comes from the update <see cref="Action"/>
        /// </summary>
        /// <remarks>
        ///     TODO: Find a better way to update the item on the database to any availlable value.
        /// </remarks>
        /// <typeparam name="TModel">The model corresponding to the database table to update</typeparam>
        /// <param name="predicate">The <see cref="Action"/> defining the where clause for the update statement</param>
        /// <returns></returns>
        public async override Task Update<TModel>(Action<TModel> predicate)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Get the name of the database table
                    string table = (typeof(TModel).CustomAttributes.Count() > 0) ? (typeof(TModel).GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : typeof(TModel).Name;

                    // Generate the new values
                    string newValues = null;
                    TModel item = new TModel();
                    predicate(item);

                    if (!Schema.HasPrimaryKeyValue(item)) throw new Exception("In order to update an item, the item must have a value for the primary key and the primary will be used in the where statement only.");

                    // Updated property value assingment
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        // Get the value
                        var value = prop.GetValue(item);

                        // Check if the value is null or the primary key
                        if (value == null || Schema.IsPrimaryKey(prop, typeof(TModel))) continue;

                        // If there is already a where clause
                        // Add a "AND"
                        if (newValues != null) newValues += ", ";

                        var propname = (prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name;
                        var newValue = "";

                        // Check if the value is the default value for the type, if so 
                        if (value == prop.PropertyType.GetDefault())
                            newValue = "NULL";
                        else
                            switch (value.GetType().ToString())
                            {
                                case "System.Guid":
                                case "System.String":
                                    newValue = $"'{value}'";
                                    break;
                                case "System.Boolean":
                                    var v = (bool)value ? 1 : 0;
                                    newValue = $"{v}";
                                    break;
                                case "System.DateTimeOffset":
                                    string dtFormat = "yyyy-MM-dd HH:mm:ss.fffffff zzz";
                                    DateTimeOffset dt = DateTimeOffset.Parse(value.ToString());
                                    newValue = $"'{dt.ToString(dtFormat)}'";
                                    break;
                                default:
                                    newValue = $"{value}";
                                    break;
                            }
                        newValues += $"{propname} = {newValue}";
                    }

                    // Get the where clause from predicate
                    string where = null;
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        if (!Schema.IsPrimaryKey(prop, typeof(TModel))) continue;
                        if (prop.GetValue(item) == null) continue;

                        var value = prop.GetValue(item);

                        // If there is already a where clause
                        // Add a "AND"
                        if (where != null) where += " AND ";

                        var propname = (prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name;
                        var propEquater = "";
                        switch (value.GetType().ToString())
                        {
                            case "System.Guid":
                            case "System.String":
                                propEquater = $"'{value}'";
                                break;
                            case "System.Boolean":
                                var v = (bool)value ? 1 : 0;
                                propEquater = $"{v}";
                                break;
                            default:
                                propEquater = $"{value}";
                                break;
                        }
                        where += $"{propname} = {propEquater}";
                    }

                    // Update statement must have a where clause
                    if (string.IsNullOrWhiteSpace(where)) throw new Exception("Where clause in Update cannot be null");
                    if (string.IsNullOrWhiteSpace(newValues)) throw new Exception("Set clause in Update cannot be null");

                    // Generate the query
                    string query = $"UPDATE {table} SET {newValues} WHERE {where}";

                    // Execute the action
                    using (SqlConnection connection = new SqlConnection(_ConnectionString))
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = query;
                        command.Connection = connection;
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                });
            }
            catch (Exception ex)
            {
                throw new DBActionProviderException("The action provider could not update the item based on the given predicate", ex);
            }
        }

        #endregion
    }
}
