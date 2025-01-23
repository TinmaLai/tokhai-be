using Dapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using z76_backend.Models;

namespace z76_backend.Infrastructure
{
    public class BaseRepository<T> where T : class
    {
        protected readonly string _connectionString;
        public BaseRepository(string connection)
        {
            _connectionString = connection;
        }
        private string GetTableName()
        {
            var attribute = typeof(T).GetCustomAttribute<TableNameAttribute>();
            return attribute?.Name ?? typeof(T).Name; // Nếu không có Attribute, trả về tên class mặc định
        }
        public static PropertyInfo? GetKeyProperty<T>()
        {
            return typeof(T)
                .GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
        }
        public async Task<IEnumerable<T>> GetAll()
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();
            return await connection.QueryAsync<T>($"SELECT * FROM {tableName}");
        }

        public async Task<T> GetById(Guid id)
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();
            var keyProp = GetKeyProperty<T>();
            return await connection.QueryFirstOrDefaultAsync<T>($"SELECT * FROM {tableName} WHERE {keyProp.Name} = @Id", new { Id = id });
        }

        public async Task<int> Add(T record)
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();
            var insertFields = new List<string>();
            var properties = typeof(T).GetProperties();
            var parameters = new DynamicParameters();

            foreach (var property in properties)
            {
                insertFields.Add(property.Name);
                parameters.Add($"{property.Name}", property.GetValue(record));
            }

            var insertQuery = @$"INSERT INTO {tableName} ({string.Join(",", insertFields)}) 
                VALUES (@{string.Join(",@", insertFields)});";
            return await connection.ExecuteAsync(insertQuery, parameters);
        }

        public async Task<int> Update(IEnumerable<T> records)
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();
            var properties = typeof(T).GetProperties();
            var parameters = new DynamicParameters();
            var updateBatchQuery = new StringBuilder();
            var i = 0;
            foreach (var record in records)
            {
                var insertFields = new List<string>();
                foreach (var property in properties)
                {
                    insertFields.Add(property.Name);
                    parameters.Add($"{property.Name}{i}", property.GetValue(record));
                }
                // Xây dựng câu lệnh SQL động
                var setClause = string.Join(", ", insertFields.Select(f => $"{f} = @{f}{i}"));

                var keyProp = GetKeyProperty<T>();
                var id = keyProp.GetValue(record);
                parameters.Add($"Id{i}", id);
                updateBatchQuery.Append($"UPDATE {tableName} SET {setClause} WHERE {keyProp.Name} = @Id{i};");
                i++;
            }
            var effects = await connection.ExecuteAsync(updateBatchQuery.ToString(), parameters);
            return effects;

        }

        public async Task<int> Delete(Guid id)
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();
            var keyProp = GetKeyProperty<T>();
            return await connection.ExecuteAsync($"DELETE FROM {tableName} WHERE {keyProp.Name} = @Id", new { Id = id });
        }
        /// <summary>
        /// Lấy dữ liệu phân trang
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="take"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<List<T>> GetPagingAsync(List<FilterCondition> filters, int take, int limit)
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();

            // Danh sách cột hợp lệ
            var allowedOperators = new Dictionary<string, string>
            {
                { "=", "=" }, { "!=", "<>" }, { "<", "<" }, { ">", ">" },
                { "<=", "<=" }, { ">=", ">=" }
            };

            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            // Xây dựng câu WHERE từ filters
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    string paramName = $"@{filter.Field}_{whereClauses.Count}";
                    whereClauses.Add($"{filter.Field} {allowedOperators[filter.Operator]} {paramName}");
                    parameters.Add(paramName, filter.Value);
                }
            }

            // Xây dựng câu SQL
            var sql = new StringBuilder($"SELECT * FROM {tableName}");

            if (whereClauses.Any())
            {
                sql.Append(" WHERE " + string.Join(" AND ", whereClauses));
            }

            //sql.Append(" ORDER BY Id DESC"); // Sắp xếp theo Id (có thể thay đổi)
            sql.Append(" LIMIT @Limit OFFSET @Offset");

            parameters.Add("@Limit", limit);
            parameters.Add("@Offset", (take - 1) * limit);

            // Thực hiện truy vấn
            var result = await connection.QueryAsync<T>(sql.ToString(), parameters);
            return result.ToList();
        }
    }
}
