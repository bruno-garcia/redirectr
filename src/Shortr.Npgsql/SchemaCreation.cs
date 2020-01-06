using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Shortr.Npgsql
{
    public class SchemaCreation
    {
        private readonly string _connectionString;

        public SchemaCreation(string connectionString)
            => _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        public async ValueTask CreateTable(CancellationToken token)
        {
            const string getUrlQuery =
                "CREATE TABLE IF NOT EXISTS shortened_urls (key varchar(100) PRIMARY KEY, url varchar(2048) NOT NULL, ttl integer NULL);";
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(token);
            await using var command = new NpgsqlCommand(getUrlQuery, connection);

            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
    }
}
