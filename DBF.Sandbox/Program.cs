using System;
using System.Linq;

namespace DBF.SandBox
{
    class Program
    {
        static void Main()
        {
            Tests();
        }

        private static async void Tests()
        {
            using (var context = new TestContext())
            {
                //context.Users.Remove(context.Users.Where(u => u.Id == Guid.Parse("656FA219-E0CA-4117-A99F-C80C56BD44D5")).First());
                //context.Users.Remove(context.Users.Where(u => u.Id == Guid.Parse("C4870235-9DBF-4112-BF03-130C1A30A1B8")).First());
                //context.Users.Remove(context.Users.Where(u => u.Id == Guid.Parse("7792DE0E-DC89-4BC6-BE93-5FC58EE14168")).First());

                //context.Users.Add(new User
                //{
                //    Id = Guid.NewGuid(),
                //    FirstName = "Test",
                //    IsEnabled = true,
                //    CreatedDateUTC = DateTimeOffset.UtcNow
                //});
                //context.Users.Add(new User
                //{
                //    Id = Guid.NewGuid(),
                //    IsEnabled = true,
                //    CreatedDateUTC = DateTimeOffset.UtcNow
                //});
                //context.Users.Add(new User
                //{
                //    Id = Guid.NewGuid(),
                //    FirstName = "Test 3",
                //    LastName = "Bibi Jones",
                //    UserName = "Britney Beth",

                //    IsEnabled = true,
                //    CreatedDateUTC = DateTimeOffset.UtcNow
                //});

                //context.Users.Where(u => u.Id == Guid.Parse("656FA219-E0CA-4117-A99F-C80C56BD44D5")).First().FirstName = "Test 2";
                //context.Users.Where(u => u.Id == Guid.Parse("656FA219-E0CA-4117-A99F-C80C56BD44D5")).First().LastName = "Failed";
                //context.Users.Where(u => u.Id == Guid.Parse("656FA219-E0CA-4117-A99F-C80C56BD44D5")).First().UserName = "Magic";

                //context.Users.Where(u => u.Id == Guid.Parse("C4870235-9DBF-4112-BF03-130C1A30A1B8")).First().IsEnabled = false;

                //context.Users.Where(u => u.Id == Guid.Parse("7792DE0E-DC89-4BC6-BE93-5FC58EE14168")).First().FirstName = null;

                await context.Commit();
            }

            Console.WriteLine("Done");
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
            Options.Use<SqlDBActionProvider>();

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
