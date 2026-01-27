using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MISA.Fresher.Core.Enities;
using MISA.Fresher.Core.MISAAttribute;
using MySqlConnector;
using System.Globalization;
using System.Linq;
using System.Reflection;
using static Dapper.SqlMapper;


namespace MISA.Fresher.Infrastructer.Repository
{
    public class BaseRepo<T>
    {
        public readonly string _connectionString;
        MySqlConnection   connection;
        public BaseRepo(IConfiguration config, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                _connectionString = config.GetConnectionString("Development");
            }
            else
            {
                _connectionString = config.GetConnectionString("Production");
            }
        }
        /// <summary>
        /// lấy tên cột trong db từ property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>

        public string GetTableName()
        {
            var tableName = typeof(T).Name.ToLower();
            var tableAttr = typeof(T).GetCustomAttribute<MISATableName>();
            if (tableAttr != null)
            {
                tableName = tableAttr?.TableName ?? typeof(T).Name;
            }
            return tableName;
        }

        private static bool TrySetByColumnName(T item, string columnName, object? value)
        {
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                var col = GetColumnName(prop); // đã có sẵn trong code bạn
                if (col.Equals(columnName, StringComparison.OrdinalIgnoreCase) && prop.CanWrite)
                {
                    prop.SetValue(item, value);
                    return true;
                }
            }
            return false;
        }

        private static string GetColumnName(PropertyInfo prop)
        {
            var colAttr = prop.GetCustomAttributes(typeof(MISAColumnName), true)
                              .FirstOrDefault() as MISAColumnName;

            return colAttr?.Name ?? prop.Name;
        }
        public List<T> GetAll()
        {
            //Xac dinh ten bang
            var tableName = GetTableName();
            var sql = $"SELECT * FROM {tableName}";
            using (connection = new MySqlConnection(_connectionString))
            {
                var data = connection.Query<T>(sql);
                return data.ToList() ;
            }

        }
        public T GetById(Guid id)
        {
            var tableName = GetTableName();
            var sql = $"SELECT * FROM {tableName} WHERE {tableName}_id = @Id";
            using (connection = new MySqlConnection(_connectionString))
            {
                var data = connection.QueryFirstOrDefault<T>(sql, new { Id = id });
                return data;
            }
        }
        public T Insert(T item)
        {
            /// Tăng tiến của code asset đển tăng 1 sau mỗi lần thêm mới
            /// và tự động điền vào mã tài sản
            //tạo query đối tượng mới
            var columns = new List<string>();
                var columnParamName = new List<string>();
                var parameters = new DynamicParameters();
                var tableName = GetTableName();

                 //lấy tất cả các property của đối tượng
                var props = typeof(T).GetProperties();
                // duyêt từng property
                foreach (var prop in props)
                {
                    //lấy tên của property
                    var propName = prop.Name;
                    //lấy giá trị của property
                    var colAttr = prop.GetCustomAttributes<MISAColumnName>();
                    if (colAttr != null)
                    {
                        propName = colAttr.FirstOrDefault()?.Name ?? propName;
                    }
                    columns.Add(propName);
                    columnParamName.Add($"@{propName}");
                    parameters.Add($"@{propName}", prop.GetValue(item));

                }
                var sqlInsert = $@"Insert {tableName} ({string.Join(",", columns)}) VALUE ({string.Join(",", columnParamName)})";
           
                using (connection = new MySqlConnection(_connectionString))
                {
                    var res = connection.Execute(sqlInsert, param: parameters);
                    return item;
                }

        }
        public T Update(Guid id, T item)
        {
           var tableName = GetTableName();
            var updateColumns = new List<string>();
            var parameters = new DynamicParameters();
            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                var propName = prop.Name;
                var colAttr = prop.GetCustomAttributes<MISAColumnName>();
                if (colAttr != null)
                {
                    propName = colAttr.FirstOrDefault()?.Name ?? propName;
                }
                if (propName.ToLower() == $"{tableName}_id")
                {
                    id = (Guid)prop.GetValue(item);
                }
                else
                {
                    updateColumns.Add($"{propName} = @{propName}");
                    parameters.Add($"@{propName}", prop.GetValue(item));
                }
            }
            parameters.Add($"@{tableName}_id", id);
            var sqlUpdate = $@"UPDATE {tableName} SET {string.Join(",", updateColumns)} WHERE {tableName}_id = @{tableName}_id";
            using (connection = new MySqlConnection(_connectionString))
            {
                var res = connection.Execute(sqlUpdate, param: parameters);
                return item;
            }
        }
        public void Delete(Guid id)
        {
            var tableName = GetTableName();
            var sqlDelete = $@"DELETE FROM {tableName} WHERE {tableName}_id = @Id";
            using (connection = new MySqlConnection(_connectionString))
            {
                var res = connection.Execute(sqlDelete, new { Id = id });
            }
        }

        public int DeleteMany(IEnumerable<Guid> ids)
        {
            var tableName = GetTableName();
            var idList = ids?.Distinct().ToList() ?? new List<Guid>();
            if (idList.Count == 0) return 0;

            var sql = $@"DELETE FROM {tableName}
                 WHERE {tableName}_id IN @Ids";

            using (connection = new MySqlConnection(_connectionString))
            {
                return connection.Execute(sql, new { Ids = idList });
            }
        }
        public T Create(Guid sourceId, T entity)
        {
            var tableName = GetTableName();

            // 1) Lấy bản ghi gốc
            var sqlGet = $"SELECT * FROM {tableName} WHERE {tableName}_id = @Id";
            T source;

            using (connection = new MySqlConnection(_connectionString))
            {
                source = connection.QueryFirstOrDefault<T>(sqlGet, new { Id = sourceId });
            }

            if (source == null) return default!;

            // 2) Clone object và set Id mới
            var newItem = Activator.CreateInstance<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                var colName = GetColumnName(prop);

                // Nếu là khóa chính {tableName}_id -> new Guid
                if (colName.Equals($"{tableName}_id", StringComparison.OrdinalIgnoreCase))
                {
                    if (prop.PropertyType == typeof(Guid)) prop.SetValue(newItem, Guid.NewGuid());
                    else if (prop.PropertyType == typeof(Guid?)) prop.SetValue(newItem, (Guid?)Guid.NewGuid());
                    continue;
                }

                // Copy y như bản gốc
                prop.SetValue(newItem, prop.GetValue(source));
            }

            // 4) Insert bằng đúng logic Insert() của bạn
            return Insert(newItem);
        }

    }
}
