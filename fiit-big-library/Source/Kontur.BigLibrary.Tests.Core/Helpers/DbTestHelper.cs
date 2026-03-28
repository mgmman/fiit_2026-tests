using Kontur.BigLibrary.DataAccess;
using Kontur.BigLibrary.Service.Configuration;
using Kontur.BigLibrary.Service.Contracts;
using Microsoft.Data.Sqlite;

namespace Kontur.BigLibrary.Tests.Core.Helpers
{
    public static class DbHelper
    {
        private const string TestDbName = "biglibrary-test.db";
        public const string ConnectionString = @"Data Source=./" + TestDbName + ";Mode=ReadWriteCreate;Cache=Shared";
        public static IDbConnectionFactory CreateConnectionFactory()
        {
            return new DbConnectionFactory(ConnectionString);
        }
        
        public static async Task CreateDbAsync()
        {
            DataAccessConfiguration.Configure();
            await DropDbAsync();
            await CreateDbFile();

            var builder = new SqliteConnectionStringBuilder(ConnectionString);
            await using var connection = new SqliteConnection(builder.ToString());
            await connection.OpenAsync();
        }

        private static Task CreateDbFile()
        {
            File.Copy("biglibrary.db", TestDbName, true);
            return Task.CompletedTask;
        }

        public static async Task DropDbAsync()
        {
            File.Delete(TestDbName);
        }
        
        public static BookFilter CreateFilter(string query = "", string rubric = "", int? limit = null, bool? isBusy = null,
            BookOrder order = BookOrder.ByLastAdding, int offset = 0)
        {
            return new()
            {
                Query = query,
                RubricSynonym = rubric,
                IsBusy = isBusy,
                Limit = limit,
                Order = order,
                Offset = offset
            };
        }
    }
}