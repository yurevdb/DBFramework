using System;

namespace DBF.SandBox
{
    class Program
    {
        static void Main()
        {
            Tests();

            Console.ReadKey();
        }

        private static async void Tests()
        {
            using(var context = new TestContext())
            {
                var users = context.Users;

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Bibi",
                    LastName = "Jones",
                    IsEnabled = true,
                    CreatedDateUTC = DateTimeOffset.UtcNow,
                    UserName = "BJ"
                };

                context.Users.Add(newUser);

                await context.Commit();
            }

            using(var context = new TestContext())
            {
                _ = context.Users;
            }
        }
    }

    #region Test Data

    class TestContext : DBContext
    {
        #region Private Members

        const string connString = "Data Source = (local);Initial Catalog = Test; User ID = sa; Password = LX3PLjKv17bTVzi3ur8w;";

        #endregion

        #region Sets

        /// <summary>
        /// The set of users
        /// </summary>
        public DBSet<User> Users { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public TestContext() : base(connString) { }

        #endregion

        #region Virtual Functions

        public override void OnContextCreating(DBContextBuilder contextBuilder)
        {
            // Set the options
            Options.Use<SqlActionProvider>();

            // Call base function
            base.OnContextCreating(contextBuilder);

            // Set constraints
            contextBuilder.Model<User>().HasKey(m => m.Id);
            contextBuilder.Model<User>().IsRequired(m => m.IsEnabled);
            contextBuilder.Model<User>().IsRequired(m => m.CreatedDateUTC);
        }

        #endregion
    }

    /// <summary>
    /// A user for the test database
    /// </summary>
    [DBName("Users")]
    public class User
    {
        /// <summary>
        /// The Id of the user
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// The first name of the user
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the user
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The username of the user
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Signals wether the user is active
        /// </summary>
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// The <see cref="DateTimeOffset"/> for when the user was created
        /// </summary>
        public DateTimeOffset? CreatedDateUTC { get; set; }

        /// <summary>
        /// Overridden <see cref="ToString"/> method to show first and last name of the user
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{FirstName} {LastName}";
    }

    #endregion
}
