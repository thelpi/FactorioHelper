using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace FactorioHelper
{
    internal class MySqlDataProvider : IDataProvider
    {
        private const string ConnectionStringFormat = "Server={0};Database={1};Uid={2};Pwd={3};";

        private readonly string _connectionString;

        public MySqlDataProvider(string server, string database, string uid, string pwd)
        {
            _connectionString = string.Format(ConnectionStringFormat, server, database, uid, pwd);
        }

        public T GetData<T>(string query, Func<IDataReader, T> converter)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return converter(reader);
                        }
                    }
                }
            }

            return default;
        }

        public IReadOnlyCollection<T> GetDatas<T>(string query, Func<IDataReader, T> converter)
        {
            var datas = new List<T>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            datas.Add(converter(reader));
                        }
                    }
                }
            }

            return datas;
        }
    }

    internal static class SqlExtensions
    {
        public static T Get<T>(this IDataReader reader, string columnName, T defaultValue = default)
        {
            var value = reader[columnName];
            if (value == null || value == DBNull.Value)
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
