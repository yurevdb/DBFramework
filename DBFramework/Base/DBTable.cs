using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DBF
{
    /// <summary>
    /// Defines the model for the database table
    /// </summary>
    /// <typeparam name="TModel">The model to use</typeparam>
    public class DBTable<TModel>
        where TModel : class, new()
    {
        #region Public Properties

        /// <summary>
        /// The model type for the <see cref="DBTable{TModel}"/>
        /// </summary>
        public TModel Model { get; }

        /// <summary>
        /// The primary key for the model
        /// </summary>
        public MemberInfo PrimaryKey { get; private set; }

        /// <summary>
        /// The properties of the model that are required by the database
        /// </summary>
        public HashSet<MemberInfo> RequiredProperties { get; private set; } = new HashSet<MemberInfo>();

        /// <summary>
        /// The properties of the model that should be ignored for the database
        /// </summary>
        public HashSet<MemberInfo> IgnoredProperties { get; private set; } = new HashSet<MemberInfo>();

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public DBTable()
        {
            // Create an instance of the model
            Model = new TModel();

            // Try to find a property named "ID" or "Id" and if we find such property, set it as the primary key
            foreach (var prop in Model.GetType().GetProperties())
                if (prop.Name == "ID" || prop.Name == "Id")
                {
                    PrimaryKey = prop;
                    break;
                }
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Sets the primary key of the <see cref="DBTable{TModel}"/>
        /// TODO: find a way to enable the use of a composite primary key
        /// </summary>
        /// <param name="primaryKey"></param>
        public void HasKey(Expression<Func<TModel, object>> primaryKey)
        {
            MemberInfo prop = ((primaryKey.Body as UnaryExpression)?.Operand as MemberExpression)?.Member;
            PrimaryKey = prop ?? PrimaryKey;
        }

        /// <summary>
        /// TODO: Implement constraints for the properties
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public MemberInfo Property(Expression<Func<TModel, object>> property)
        {
            return ((property.Body as UnaryExpression)?.Operand as MemberExpression)?.Member;
        }

        /// <summary>
        /// Sets the property as ignored so the it will never be pushed to the database.
        /// Also won't be set when fetched from the database.
        /// </summary>
        /// <param name="property">The property to ignore</param>
        public void Ignore(Expression<Func<TModel, object>> property)
        {
            // Get the property from the expression
            MemberInfo prop = ((property.Body as UnaryExpression)?.Operand as MemberExpression)?.Member;

            // If the property is already set as ignored
            // Return and do nothing
            if (IgnoredProperties.Contains(prop)) return;

            // Add the property to the ignored properties
            IgnoredProperties.Add(prop);
        }

        /// <summary>
        /// <para>
        ///     Sets the property as a required property for the model.
        ///     When a database action is ran it will check for these constraints.
        /// </para>
        /// <para>
        ///     If the property given is set to be ignored, it will not add the given property to the list of required properties
        /// </para>
        /// </summary>
        /// <param name="property">The property to set required</param>
        public void IsRequired(Expression<Func<TModel, object>> property)
        {
            // Get the property from the expression
            MemberInfo prop = ((property.Body as UnaryExpression)?.Operand as MemberExpression)?.Member;

            // If the property is already set as ignored
            // Return and do nothing
            if (IgnoredProperties.Contains(prop)) return;

            // If the property is already set to be required
            // Return and do nothing
            if (RequiredProperties.Contains(prop)) return;

            // Add the property to the ignored properties
            RequiredProperties.Add(prop);
        }

        #endregion
    }
}
