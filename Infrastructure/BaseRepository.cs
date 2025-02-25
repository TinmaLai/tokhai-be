using Dapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using z76_backend.Enums;
using z76_backend.Models;

namespace z76_backend.Infrastructure
{
    public class BaseRepository<T> : IBaseRepository<T>
    {
        protected readonly string _connectionString;
        public BaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection"); ;
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
        public static PropertyInfo? GetPropertyValue<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName);
            
        }
        public async Task<List<T>> GetAll()
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();
            var data = await connection.QueryAsync<T>($"SELECT * FROM {tableName}");
            return data.ToList();
        }

        public async Task<T> GetById(Guid id)
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();
            var keyProp = GetKeyProperty<T>();
            return await connection.QueryFirstOrDefaultAsync<T>($"SELECT * FROM {tableName} WHERE {keyProp.Name} = @Id", new { Id = id });
        }

        public async Task<int> Add(List<T> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentException("Danh sách records không được rỗng.");
            }

            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();
            var insertFields = new List<string>();
            var properties = typeof(T).GetProperties();

            // Xác định cột khóa chính (Key)
            var keyProperty = properties.FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyAttribute)));

            if (keyProperty == null)
            {
                throw new InvalidOperationException("Không tìm thấy cột khóa chính.");
            }

            insertFields = properties.Select(p => p.Name).ToList();

            var insertQuery = @$"INSERT INTO {tableName} ({string.Join(",", insertFields)}) 
                         VALUES ({string.Join(",", insertFields.Select(f => "@" + f))});";

            var parametersList = new List<DynamicParameters>();

            foreach (var record in records)
            {
                var parameters = new DynamicParameters();

                // Nếu cột khóa chính chưa có giá trị, gán giá trị mới
                var keyValue = keyProperty.GetValue(record);
                if (keyValue == null || keyValue.ToString() == Guid.Empty.ToString())
                {
                    if (keyProperty.PropertyType == typeof(Guid))
                    {
                        keyProperty.SetValue(record, Guid.NewGuid());
                    }
                    else if (keyProperty.PropertyType == typeof(string))
                    {
                        keyProperty.SetValue(record, Guid.NewGuid().ToString());
                    }
                    else
                    {
                        throw new InvalidOperationException("Key property phải có kiểu Guid hoặc string.");
                    }
                }

                // Thêm dữ liệu của bản ghi vào parameters
                foreach (var property in properties)
                {
                    parameters.Add($"@{property.Name}", property.GetValue(record));
                }

                parametersList.Add(parameters);
            }

            // Thực thi chèn nhiều bản ghi cùng lúc
            return await connection.ExecuteAsync(insertQuery, parametersList);
        }

        public async Task<int> Update(List<T> records, string field)
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();
            var properties = typeof(T).GetProperties();
            var parameters = new DynamicParameters();
            var updateBatchQuery = new StringBuilder();
            var i = 0;
            foreach (var record in records)
            {
                if(typeof(T) == typeof(UserEntity))
                {
                    properties = properties.Where(x => x.Name != nameof(UserEntity.username) && x.Name != nameof(UserEntity.password)).ToArray();
                }
                var insertFields = new List<string>();
                foreach (var property in properties)
                {
                    insertFields.Add(property.Name);
                    parameters.Add($"{property.Name}{i}", property.GetValue(record));
                }
                // Xây dựng câu lệnh SQL động
                var setClause = string.Join(", ", insertFields.Select(f => $"{f} = @{f}{i}"));

                var fieldProp = GetPropertyValue<T>(field);
                var fieldValue = fieldProp.GetValue(record);
                parameters.Add($"{field}{i}", fieldValue);
                updateBatchQuery.Append($"UPDATE {tableName} SET {setClause} WHERE {field} = @{field}{i};");
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

            // Danh sách toán tử hợp lệ
            var allowedOperators = new Dictionary<string, string>
            {
                { "equal", "=" }, { "not equal", "<>" }, { "less", "<" }, { "great", ">" },
                { "less equal", "<=" }, { "great equal", ">=" }, { "contains", "like" }, { "not contains", "not like" }
            };

            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            // Xây dựng câu WHERE từ filters
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    string paramName = $"@{filter.Field}_{whereClauses.Count}";

                    // Kiểm tra toán tử có hợp lệ không
                    if (!allowedOperators.ContainsKey(filter.Operator))
                    {
                        throw new ArgumentException($"Operator '{filter.Operator}' is not supported.");
                    }

                    // Kiểm tra và parse kiểu DateTime
                    object value = filter.Value;
                    if (value is string stringValue && DateTime.TryParse(stringValue, out var parsedDate))
                    {
                        value = parsedDate; // Parse thành công
                    }

                    // Thêm vào câu WHERE
                    whereClauses.Add($"{filter.Field} {allowedOperators[filter.Operator]} {paramName}");

                    if (allowedOperators[filter.Operator] == "like" || allowedOperators[filter.Operator] == "not like")
                    {
                        parameters.Add(paramName, $"%{value}%");
                    }
                    else
                    {
                        parameters.Add(paramName, value);
                    }
                }
            }

            // Xây dựng câu SQL
            var sql = new StringBuilder($"SELECT * FROM {tableName}");

            if (whereClauses.Any())
            {
                sql.Append(" WHERE " + string.Join(" AND ", whereClauses));
            }

            // Phân trang
            sql.Append(" LIMIT @Limit OFFSET @Offset");
            parameters.Add("@Limit", limit);
            parameters.Add("@Offset", (take - 1) * limit);

            // Thực hiện truy vấn
            var result = await connection.QueryAsync<T>(sql.ToString(), parameters);
            return result.ToList();
        }
        public async Task<object> GetPagingSummaryAsync(List<FilterCondition> filters)
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();

            // Danh sách toán tử hợp lệ
            var allowedOperators = new Dictionary<string, string>
            {
                { "equal", "=" }, { "not equal", "<>" }, { "less", "<" }, { "great", ">" },
                { "less equal", "<=" }, { "great equal", ">=" }, { "contains", "like" }, { "not contains", "not like" }
            };

            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            // Xây dựng câu WHERE từ filters
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    string paramName = $"@{filter.Field}_{whereClauses.Count}";

                    // Kiểm tra toán tử có hợp lệ không
                    if (!allowedOperators.ContainsKey(filter.Operator))
                    {
                        throw new ArgumentException($"Operator '{filter.Operator}' is not supported.");
                    }

                    // Kiểm tra và parse kiểu DateTime
                    object value = filter.Value;
                    if (value is string stringValue && DateTime.TryParse(stringValue, out var parsedDate))
                    {
                        value = parsedDate; // Parse thành công
                    }

                    // Thêm vào câu WHERE
                    whereClauses.Add($"{filter.Field} {allowedOperators[filter.Operator]} {paramName}");

                    if (allowedOperators[filter.Operator] == "like" || allowedOperators[filter.Operator] == "not like")
                    {
                        parameters.Add(paramName, $"%{value}%");
                    }
                    else
                    {
                        parameters.Add(paramName, value);
                    }
                }
            }

            // Xây dựng câu SQL
            var sql = new StringBuilder($"SELECT COUNT(*) FROM {tableName}");

            if (whereClauses.Any())
            {
                sql.Append(" WHERE " + string.Join(" AND ", whereClauses));
            }

            // Thực hiện truy vấn
            var result = await connection.ExecuteScalarAsync<int>(sql.ToString(), parameters);
            return new
            {
                Total = result
            };
        }
        /// <summary>
        /// Lấy dữ liệu theo filter
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="take"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<List<T>> GetAsync(List<FilterCondition> filters)
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();

            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            // Xây dựng câu WHERE từ filters
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    string paramName = $"@{filter.Field}_{whereClauses.Count}";

                    // Kiểm tra toán tử có hợp lệ không
                    string operatorSymbol = filter.Operator;

                    // Kiểm tra và parse kiểu DateTime
                    object value = filter.Value;
                    if (value is string stringValue && DateTime.TryParse(stringValue, out var parsedDate))
                    {
                        value = parsedDate;
                    }

                    // Thêm vào câu WHERE
                    whereClauses.Add($"{filter.Field} {operatorSymbol} {paramName}");

                    if (operatorSymbol == FilterOperator.Contains || operatorSymbol == FilterOperator.NotContains)
                    {
                        parameters.Add(paramName, $"%{value}%");
                    }
                    else
                    {
                        parameters.Add(paramName, value);
                    }
                }
            }

            // Xây dựng câu SQL
            var sql = new StringBuilder($"SELECT * FROM {tableName}");
            if (whereClauses.Any())
            {
                sql.Append(" WHERE " + string.Join(" AND ", whereClauses));
            }

            // Thực hiện truy vấn
            var result = await connection.QueryAsync<T>(sql.ToString(), parameters);
            return result.ToList();
        }
        public async Task<List<T>> DeleteAsync(List<FilterCondition> filters)
        {
            using var connection = new MySqlConnection(_connectionString);
            string tableName = GetTableName();

            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            // Xây dựng câu WHERE từ filters
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    string paramName = $"@{filter.Field}_{whereClauses.Count}";

                    // Kiểm tra toán tử có hợp lệ không
                    string operatorSymbol = filter.Operator;

                    // Kiểm tra và parse kiểu DateTime
                    object value = filter.Value;
                    if (value is string stringValue && DateTime.TryParse(stringValue, out var parsedDate))
                    {
                        value = parsedDate;
                    }

                    // Thêm vào câu WHERE
                    whereClauses.Add($"{filter.Field} {operatorSymbol} {paramName}");

                    if (operatorSymbol == FilterOperator.Contains || operatorSymbol == FilterOperator.NotContains)
                    {
                        parameters.Add(paramName, $"%{value}%");
                    }
                    else
                    {
                        parameters.Add(paramName, value);
                    }
                }
            }

            // Xây dựng câu SQL
            var sql = new StringBuilder($"DELETE FROM {tableName}");
            if (whereClauses.Any())
            {
                sql.Append(" WHERE " + string.Join(" AND ", whereClauses));
            }

            // Thực hiện truy vấn
            var result = await connection.QueryAsync<T>(sql.ToString(), parameters);
            return result.ToList();
        }
    }
}
