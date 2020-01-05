using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Redirectr.Npgsql
{
    public class NpgsqlRedirectrOptions
    {
        public bool CreateSchema { get; set; } = true;
        public string ConnectionString { get; set; } = null!;
    }

    public class NpgsqlRedirectrStore : IRedirectrStore
    {
        private readonly NpgsqlRedirectrOptions _options;
        private int _initialized = 0;

        public NpgsqlRedirectrStore(NpgsqlRedirectrOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            {
                throw new ArgumentException("A connection string is required.");
            }
        }

        public async ValueTask<string?> GetUrl(string key, CancellationToken token)
        {
            if (_initialized == 0)
            {
                await Initialize(token);
            }

            const string getUrlQuery = "SELECT url FROM shortened_urls WHERE key = @key;";
            await using var connection = new NpgsqlConnection(_options.ConnectionString);
            await connection.OpenAsync(token);
            await using var command = new NpgsqlCommand(getUrlQuery, connection);

            command.Parameters.AddWithValue("key", key);

            return await command.ExecuteScalarAsync(token) as string;
        }

        public async ValueTask<string?> GetKey(string url, CancellationToken token)
        {
            if (_initialized == 0)
            {
                await Initialize(token);
            }

            const string getUrlQuery = "SELECT key FROM shortened_urls WHERE url = @url;";
            await using var connection = new NpgsqlConnection(_options.ConnectionString);
            await connection.OpenAsync(token);
            await using var command = new NpgsqlCommand(getUrlQuery, connection);

            command.Parameters.AddWithValue("url", url);

            return await command.ExecuteScalarAsync(token) as string;
        }

        public async ValueTask RegisterUrl(RegistrationOptions options, CancellationToken token)
        {
            if (_initialized == 0)
            {
                await Initialize(token);
            }

            const string getUrlQuery = "INSERT INTO shortened_urls (key, url, ttl) VALUES (@key, @url, @ttl);";
            await using var connection = new NpgsqlConnection(_options.ConnectionString);
            await connection.OpenAsync(token);
            await using var command = new NpgsqlCommand(getUrlQuery, connection);

            command.Parameters.AddWithValue("key", options.Key);
            command.Parameters.AddWithValue("url", options.Url);
            command.Parameters.AddWithValue("ttl", options.Ttl switch
            {
                { } i => i,
                _ => DBNull.Value
            });

            await command.ExecuteNonQueryAsync(token);
        }

        private async ValueTask Initialize(CancellationToken token)
        {
            if (!_options.CreateSchema)
            {
                Interlocked.CompareExchange(ref _initialized, 1, 0);
                return;
            }

            var schema = new SchemaCreation(_options.ConnectionString);
            await schema.CreateTable(token);

            Interlocked.CompareExchange(ref _initialized, 1, 0);
        }
    }
}
