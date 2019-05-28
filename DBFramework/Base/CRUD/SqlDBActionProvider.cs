//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DBF.Base.CRUD
//{
//    public class SqlDBActionProvider : DBActionProvider
//    {


//        public SqlDBActionProvider()
//        {

//        }

//        #region Private Implementations

//        /// <summary>
//        /// Gets the <see cref="List{T}"/> from a sql database
//        /// </summary>
//        /// <typeparam name="T">The type of data to get from the corresponding table in the <see cref="DBContext"/></typeparam>
//        /// <param name="predicate">An <see cref="Action"/> to set the where clause for the select from the sql database</param>
//        /// <returns></returns>
//        public override async Task<DBSet<T>> Fetch<T>(Action<T> predicate = null)
//        {
//            return await Task.Run(() =>
//            {
//                // Get the name of the database table
//                string table = (typeof(T).CustomAttributes.Count() > 0) ? (typeof(T).GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : typeof(T).Name;

//                // Get the properties of the given object type
//                string columns = "";
//                List<string> dbcolumns = new List<string>();
//                var props = typeof(T).GetProperties();
//                foreach (var prop in props)
//                    dbcolumns.Add((prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name);
//                var count = 0;
//                foreach (string col in dbcolumns)
//                {
//                    columns += $"{col}";
//                    count++;
//                    if (count < dbcolumns.Count)
//                        columns += ", ";
//                }

//                // Get the where state from the predicate if given
//                string where = null;
//                if (predicate != null)
//                {
//                    T item = new T();
//                    predicate(item);
//                    foreach (var prop in item.GetType().GetProperties())
//                    {
//                        if (prop.GetValue(item) == null) continue;

//                        var value = prop.GetValue(item);

//                        // If there is already a where clause
//                        // Add a "AND"
//                        if (where != null) where += " AND ";

//                        var propname = (prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name;
//                        var propEquater = "";
//                        switch (value.GetType().ToString())
//                        {
//                            case "System.Guid":
//                            case "System.String":
//                                propEquater = $"'{value}'";
//                                break;
//                            case "System.Boolean":
//                                var v = (bool)value ? 1 : 0;
//                                propEquater = $"{v}";
//                                break;
//                            default:
//                                propEquater = $"{value}";
//                                break;
//                        }
//                        where += $"{propname} = {propEquater}";
//                    }
//                }

//                // Generate the query
//                string query = $"SELECT {columns} FROM {table} WHERE {where ?? "1 = 1"}";

//                DBSet<T> retval = new DBSet<T>();

//                using (SqlConnection connection = new SqlConnection(_ConnectionString))
//                using (SqlCommand command = new SqlCommand())
//                {
//                    command.CommandType = CommandType.Text;
//                    command.CommandText = query;
//                    command.Connection = connection;
//                    connection.Open();
//                    using (SqlDataReader reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            T item = new T();
//                            foreach (var prop in props)
//                            {
//                                string propname = (prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name;
//                                var value = reader[$"{propname}"];
//                                if (value.GetType() == typeof(string))
//                                    value = (value as string).Trim();
//                                prop.SetValue(item, value);
//                            }
//                            retval.Add(item);
//                        }
//                    }
//                }

//                // Set the DBSet if availlable
//                if (Options.SynchronizeDB && predicate == null)
//                    foreach (var prop in GetType().GetProperties())
//                        if (prop.PropertyType == typeof(DBSet<T>) && prop.Name == table)
//                            prop.SetValue(this, retval);

//                // Return the requested data
//                return retval;
//            });
//        }

//        /// <summary>
//        /// Pushes the given item of type <typeparamref name="T"/> to the sql database
//        /// </summary>
//        /// <typeparam name="T">The model corresponding to the database table</typeparam>
//        /// <param name="item">The item to push to the sql database</param>
//        /// <returns></returns>
//        public override async Task Push<T>(T item)
//        {
//            await Task.Run(() =>
//            {
//                // Get all the properties to insert into the database
//                List<(string, object)> dbvalues = new List<(string, object)>();
//                // TODO: Ignore the ignore properties
//                foreach (var prop in typeof(T).GetProperties())
//                    dbvalues.Add(((prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name, prop.GetValue(item)));

//                // TODO: add constraint checking here
//                // ------------------------------------------------------------
//                // If the options don't allow unsafe code and the where is null
//                // Throw an excpetion
//                if (!item.HasPrimaryKeyValue(Schema) && !Options.AllowUnsafe) throw new Exception("The DBContext does not allow unsafe code and the action that would be ran is considered unsafe\r\nAction: trying to delete without setting the where with primary key as unique identifier");
//                if (!item.RequiredValuesCorrect(Schema)) throw new Exception("Required values cannot be null");

//                // Generate the insert query
//                string table = (typeof(T).CustomAttributes.Count() > 0) ? (typeof(T).GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : typeof(T).Name;
//                string columns = "";
//                string values = "";
//                int count = 0;
//                foreach (var val in dbvalues)
//                {
//                    columns += $"{val.Item1}";
//                    switch (val.Item2.GetType().ToString())
//                    {
//                        case "System.Guid":
//                        case "System.String":
//                            values += $"'{val.Item2}'";
//                            break;
//                        case "System.Boolean":
//                            var v = (bool)val.Item2 ? 1 : 0;
//                            values += $"{v}";
//                            break;
//                        case "System.DateTimeOffset":
//                            string dtFormat = "yyyy-MM-dd HH:mm:ss.fffffff zzz";
//                            DateTimeOffset dt = DateTimeOffset.Parse(val.Item2.ToString());
//                            values += $"'{dt.ToString(dtFormat)}'";
//                            break;
//                        default:
//                            values += $"{val.Item2}";
//                            break;
//                    }
//                    if (++count == dbvalues.Count) continue;
//                    columns += ", ";
//                    values += ", ";
//                }

//                string query = $"INSERT INTO {table} ({columns}) VALUES ({values})";

//                using (SqlConnection connection = new SqlConnection(_ConnectionString))
//                using (SqlCommand command = new SqlCommand())
//                {
//                    command.CommandType = CommandType.Text;
//                    command.CommandText = query;
//                    command.Connection = connection;
//                    connection.Open();
//                    command.ExecuteNonQuery();
//                }

//                // Synchronize the database
//                if (Options.SynchronizeDB)
//                {
//                    var method = GetType().GetMethod("Fetch");
//                    var genMethod = method.MakeGenericMethod(typeof(T));
//                    // Execute the generic fetch method for the specified type
//                    if (Options.AsyncSynchronization)
//                        Task.Run(() => genMethod.Invoke(this, new object[] { null }));
//                    else
//                        (genMethod.Invoke(this, new object[] { null }) as Task)?.Wait();
//                }
//            });
//        }

//        /// <summary>
//        /// Removes the given item from the sql database
//        /// </summary>
//        /// <typeparam name="T">The model of the item corresponding to the table from the sql database</typeparam>
//        /// <param name="predicate">An <see cref="Action"/> to set the where clause for the removal of the item/items from the sql database</param>
//        /// <returns></returns>
//        public override async Task Remove<T>(Action<T> predicate)
//        {
//            await Task.Run(() =>
//            {
//                // Create the item to remove
//                T item = new T();
//                predicate(item);

//                bool HasPrimaryKey = false;

//                // Get the where state from the predicate if given
//                string where = null;
//                foreach (var prop in item.GetType().GetProperties())
//                {
//                    if (prop.GetValue(item) == null) continue;

//                    var value = prop.GetValue(item);

//                    if (!HasPrimaryKey) HasPrimaryKey = prop.IsPrimaryKey(Schema, typeof(T));

//                    // If there is already a where clause
//                    // Add a "AND"
//                    if (where != null) where += " AND ";

//                    var propname = (prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name;
//                    var propEquater = "";
//                    switch (value.GetType().ToString())
//                    {
//                        case "System.Guid":
//                        case "System.String":
//                            propEquater = $"'{value}'";
//                            break;
//                        case "System.Boolean":
//                            var v = (bool)value ? 1 : 0;
//                            propEquater = $"{v}";
//                            break;
//                        default:
//                            propEquater = $"{value}";
//                            break;
//                    }
//                    where += $"{propname} = {propEquater}";
//                }

//                // If the options don't allow unsafe code and the where is null
//                // Throw an excpetion
//                if (!HasPrimaryKey && !Options.AllowUnsafe) throw new Exception("The DBContext does not allow unsafe code and the action that would be ran is considered unsafe\r\nAction: trying to delete without setting the where with primary key as unique identifier");

//                // Get the name of the database table
//                string table = (typeof(T).CustomAttributes.Count() > 0) ? (typeof(T).GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : typeof(T).Name;

//                // Create the query
//                string query = $"DELETE FROM {table} WHERE {where ?? "1 = 1"}";

//                using (SqlConnection connection = new SqlConnection(_ConnectionString))
//                using (SqlCommand command = new SqlCommand())
//                {
//                    command.CommandType = CommandType.Text;
//                    command.CommandText = query;
//                    command.Connection = connection;
//                    connection.Open();
//                    command.ExecuteNonQuery();
//                }

//                // Synchronize the database
//                if (Options.SynchronizeDB)
//                {
//                    var method = GetType().GetMethod("Fetch");
//                    var genMethod = method.MakeGenericMethod(typeof(T));
//                    // Execute the generic fetch method for the specified type
//                    if (Options.AsyncSynchronization)
//                        Task.Run(() => genMethod.Invoke(this, new object[] { null }));
//                    else
//                        (genMethod.Invoke(this, new object[] { null }) as Task)?.Wait();
//                }
//            });
//        }

//        /// <summary>
//        /// Updates an item on the sql database based on the predicate
//        /// the new value comes from the update <see cref="Action"/>
//        /// </summary>
//        /// <typeparam name="T">The model corresponding to the database table to update</typeparam>
//        /// <param name="predicate">The <see cref="Action"/> defining the where clause for the update statement</param>
//        /// <param name="update">The <see cref="Action"/> setting the new values for the item</param>
//        /// <returns></returns>
//        public override async Task Update<T>(Action<T> predicate)
//        {
//            await Task.Run(() =>
//            {
//                // Get the name of the database table
//                string table = (typeof(T).CustomAttributes.Count() > 0) ? (typeof(T).GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : typeof(T).Name;

//                // Generate the new values
//                string newValues = null;
//                T item = new T();
//                predicate(item);

//                if (!item.HasPrimaryKeyValue(Schema)) throw new Exception("In order to update an item, the item must have a value for the primary key and the primary will be used in the where statement only.");

//                foreach (var prop in item.GetType().GetProperties())
//                {
//                    if (prop.GetValue(item) == null || prop.IsPrimaryKey(Schema, typeof(T))) continue;

//                    var value = prop.GetValue(item);

//                    // If there is already a where clause
//                    // Add a "AND"
//                    if (newValues != null) newValues += ", ";

//                    var propname = (prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name;
//                    var newValue = "";
//                    switch (value.GetType().ToString())
//                    {
//                        case "System.Guid":
//                        case "System.String":
//                            newValue = $"'{value}'";
//                            break;
//                        case "System.Boolean":
//                            var v = (bool)value ? 1 : 0;
//                            newValue = $"{v}";
//                            break;
//                        default:
//                            newValue = $"{value}";
//                            break;
//                    }
//                    newValues += $"{propname} = {newValue}";
//                }

//                // Get the where clause from predicate
//                string where = null;
//                foreach (var prop in item.GetType().GetProperties())
//                {
//                    if (!prop.IsPrimaryKey(Schema, typeof(T))) continue;
//                    if (prop.GetValue(item) == null) continue;

//                    var value = prop.GetValue(item);

//                    // If there is already a where clause
//                    // Add a "AND"
//                    if (where != null) where += " AND ";

//                    var propname = (prop.CustomAttributes.Count() > 0) ? (prop.GetCustomAttributes(typeof(DBName), false).First() as DBName)?.Name : prop.Name;
//                    var propEquater = "";
//                    switch (value.GetType().ToString())
//                    {
//                        case "System.Guid":
//                        case "System.String":
//                            propEquater = $"'{value}'";
//                            break;
//                        case "System.Boolean":
//                            var v = (bool)value ? 1 : 0;
//                            propEquater = $"{v}";
//                            break;
//                        default:
//                            propEquater = $"{value}";
//                            break;
//                    }
//                    where += $"{propname} = {propEquater}";
//                }

//                // Update statement must have a where clause
//                if (where == null) throw new Exception("Where clause in Update cannot be null");

//                // Generate the query
//                string query = $"UPDATE {table} SET {newValues} WHERE {where}";

//                // Execute the action
//                using (SqlConnection connection = new SqlConnection(_ConnectionString))
//                using (SqlCommand command = new SqlCommand())
//                {
//                    command.CommandType = CommandType.Text;
//                    command.CommandText = query;
//                    command.Connection = connection;
//                    connection.Open();
//                    command.ExecuteNonQuery();
//                }

//                // Synchronize the database
//                if (Options.SynchronizeDB)
//                {
//                    var method = GetType().GetMethod("Fetch");
//                    var genMethod = method.MakeGenericMethod(typeof(T));
//                    // Execute the generic fetch method for the specified type
//                    if (Options.AsyncSynchronization)
//                        Task.Run(() => genMethod.Invoke(this, new object[] { null }));
//                    else
//                        (genMethod.Invoke(this, new object[] { null }) as Task)?.Wait();
//                }
//            });
//        }

//        #endregion
//    }
//}
