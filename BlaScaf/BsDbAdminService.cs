#nullable enable
using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace BlaScaf
{
    public class BsDbAdminService
    {
        private readonly IFreeSql _fsql;
        private List<BsDbAdminEntitySchema>? _entities;
        private List<BsDbAdminTableSchema>? _tables;

        public BsDbAdminService(IFreeSql fsql)
        {
            _fsql = fsql;
        }

        public IReadOnlyList<BsDbAdminTableSchema> GetTables(bool refresh = false)
        {
            if (refresh)
            {
                RefreshSchema();
            }

            if (_tables != null)
            {
                return _tables;
            }

            var entities = GetEntitiesInternal();
            var databases = _fsql.DbFirst.GetDatabases();
            var dbTables = _fsql.DbFirst.GetTablesByDatabase(databases?.ToArray() ?? Array.Empty<string>());

            _tables = dbTables
                .Select(x => CreateTableSchema(x, entities))
                .OrderBy(x => x.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return _tables;
        }

        public BsDbAdminTableSchema GetTable(string tableKey, bool refresh = false)
        {
            var table = GetTables(refresh).FirstOrDefault(x =>
                string.Equals(x.Key, tableKey, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.FullName, tableKey, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Name, tableKey, StringComparison.OrdinalIgnoreCase));

            if (table == null)
            {
                throw new InvalidOperationException("未找到对应的数据表");
            }

            return table;
        }

        public QueryRsp<List<BsDbAdminRowDto>> GetRows(string tableKey, int pageIndex, int pageSize)
        {
            var table = GetTable(tableKey);
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 15 : pageSize;

            var total = Convert.ToInt32(_fsql.Ado.ExecuteScalar($"SELECT COUNT(1) FROM {QuoteTable(table)}"), CultureInfo.InvariantCulture);
            var columnsSql = string.Join(", ", table.Columns.Select(x => QuoteIdentifier(x.Name)));
            var orderBySql = BuildOrderBySql(table);
            var pageSql = BuildPageSql(pageIndex, pageSize);

            var sql = $"SELECT {columnsSql} FROM {QuoteTable(table)}{orderBySql}{pageSql}";
            var dataTable = _fsql.Ado.ExecuteDataTable(sql);

            return new QueryRsp<List<BsDbAdminRowDto>>
            {
                Total = total,
                Value = BuildRows(table, dataTable)
            };
        }

        public BsDbAdminRowDto Insert(string tableKey, Dictionary<string, string?> values)
        {
            var table = GetTable(tableKey);
            var insertColumns = table.Columns
                .Where(x => !x.IsIdentity && values.ContainsKey(x.Name))
                .ToList();

            if (insertColumns.Count == 0)
            {
                throw new InvalidOperationException("没有可插入的字段");
            }

            var columnsSql = string.Join(", ", insertColumns.Select(x => QuoteIdentifier(x.Name)));
            var valuesSql = string.Join(", ", insertColumns.Select(x => BuildSqlLiteral(x, values.TryGetValue(x.Name, out var value) ? value : null)));
            var sql = $"INSERT INTO {QuoteTable(table)} ({columnsSql}) VALUES ({valuesSql})";
            _fsql.Ado.ExecuteNonQuery(sql);

            if (table.Columns.Any(x => x.IsIdentity))
            {
                return GetLastInsertedRow(table);
            }

            if (table.Columns.Any(x => x.IsPrimary))
            {
                return GetRowByPrimaryKeys(table, ExtractPrimaryKeys(table, values));
            }

            return new BsDbAdminRowDto { Values = new Dictionary<string, string?>(values, StringComparer.OrdinalIgnoreCase) };
        }

        public BsDbAdminRowDto Update(string tableKey, Dictionary<string, string?> primaryKeys, Dictionary<string, string?> values)
        {
            var table = GetTable(tableKey);
            EnsurePrimaryKeys(table);

            var setColumns = table.Columns.Where(x => !x.IsPrimary && values.ContainsKey(x.Name)).ToList();
            if (setColumns.Count == 0)
            {
                throw new InvalidOperationException("没有可更新的字段");
            }

            var setSql = string.Join(", ", setColumns.Select(x =>
                $"{QuoteIdentifier(x.Name)} = {BuildSqlLiteral(x, values.TryGetValue(x.Name, out var value) ? value : null)}"));

            var sql = $"UPDATE {QuoteTable(table)} SET {setSql} WHERE {BuildPrimaryKeyWhereSql(table, primaryKeys)}";
            var affrows = _fsql.Ado.ExecuteNonQuery(sql);
            if (affrows == 0)
            {
                throw new InvalidOperationException("未找到要更新的记录");
            }

            return GetRowByPrimaryKeys(table, primaryKeys);
        }

        public void Delete(string tableKey, Dictionary<string, string?> primaryKeys)
        {
            var table = GetTable(tableKey);
            EnsurePrimaryKeys(table);

            var sql = $"DELETE FROM {QuoteTable(table)} WHERE {BuildPrimaryKeyWhereSql(table, primaryKeys)}";
            var affrows = _fsql.Ado.ExecuteNonQuery(sql);
            if (affrows == 0)
            {
                throw new InvalidOperationException("未找到要删除的记录");
            }
        }

        public void CreateTable(string tableName, string columnsDefinitionSql)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new InvalidOperationException("表名不能为空");
            }

            if (string.IsNullOrWhiteSpace(columnsDefinitionSql))
            {
                throw new InvalidOperationException("至少需要定义一个字段");
            }

            var sql = $"CREATE TABLE {QuoteTableName(tableName)} ({columnsDefinitionSql})";
            _fsql.Ado.ExecuteNonQuery(sql);
            RefreshSchema();
        }

        public void DropTable(string tableKey)
        {
            var table = GetTable(tableKey);
            _fsql.Ado.ExecuteNonQuery($"DROP TABLE {QuoteTable(table)}");
            RefreshSchema();
        }

        public void AddColumn(string tableKey, string columnDefinitionSql)
        {
            var table = GetTable(tableKey);
            if (string.IsNullOrWhiteSpace(columnDefinitionSql))
            {
                throw new InvalidOperationException("字段定义不能为空");
            }

            _fsql.Ado.ExecuteNonQuery($"ALTER TABLE {QuoteTable(table)} ADD COLUMN {columnDefinitionSql}");
            RefreshSchema();
        }

        public void RenameColumn(string tableKey, string oldName, string newName)
        {
            var table = GetTable(tableKey);
            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
            {
                throw new InvalidOperationException("字段名不能为空");
            }

            _fsql.Ado.ExecuteNonQuery(BuildRenameColumnSql(table, oldName, newName));
            RefreshSchema();
        }

        public void DropColumn(string tableKey, string columnName)
        {
            var table = GetTable(tableKey);
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new InvalidOperationException("字段名不能为空");
            }

            _fsql.Ado.ExecuteNonQuery($"ALTER TABLE {QuoteTable(table)} DROP COLUMN {QuoteIdentifier(columnName)}");
            RefreshSchema();
        }

        public string BuildCreateTableSql(string tableName, string columnsDefinitionSql)
        {
            return $"CREATE TABLE {QuoteTableName(tableName)} ({columnsDefinitionSql})";
        }

        public string BuildAddColumnSql(string tableKey, string columnDefinitionSql)
        {
            var table = GetTable(tableKey);
            return $"ALTER TABLE {QuoteTable(table)} ADD COLUMN {columnDefinitionSql}";
        }

        public string BuildRenameColumnSql(string tableKey, string oldName, string newName)
        {
            return BuildRenameColumnSql(GetTable(tableKey), oldName, newName);
        }

        public string BuildDropColumnSql(string tableKey, string columnName)
        {
            var table = GetTable(tableKey);
            return $"ALTER TABLE {QuoteTable(table)} DROP COLUMN {QuoteIdentifier(columnName)}";
        }

        public string BuildAlterColumnTemplate(string tableKey, string columnName, string newDefinitionSql)
        {
            var table = GetTable(tableKey);
            var quotedTable = QuoteTable(table);
            var quotedColumn = QuoteIdentifier(columnName);

            return _fsql.Ado.DataType switch
            {
                DataType.SqlServer => $"ALTER TABLE {quotedTable} ALTER COLUMN {newDefinitionSql}",
                DataType.MySql => $"ALTER TABLE {quotedTable} MODIFY COLUMN {newDefinitionSql}",
                DataType.PostgreSQL => $"ALTER TABLE {quotedTable} ALTER COLUMN {quotedColumn} TYPE {newDefinitionSql};",
                _ => $"-- 当前数据库建议直接执行自定义 SQL\n-- 例如：ALTER TABLE {quotedTable} ALTER COLUMN {quotedColumn} ...\n"
            };
        }

        public BsDbAdminSqlResult ExecuteSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new InvalidOperationException("SQL 不能为空");
            }

            var trimmed = sql.Trim();
            if (IsQuerySql(trimmed))
            {
                var dataTable = _fsql.Ado.ExecuteDataTable(trimmed);
                return new BsDbAdminSqlResult
                {
                    IsQuery = true,
                    Message = $"查询完成，共 {dataTable.Rows.Count} 行",
                    Columns = dataTable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList(),
                    Rows = dataTable.Rows.Cast<DataRow>().Select(BuildSqlRow).ToList()
                };
            }

            var affrows = _fsql.Ado.ExecuteNonQuery(trimmed);
            if (IsSchemaSql(trimmed))
            {
                RefreshSchema();
            }

            return new BsDbAdminSqlResult
            {
                IsQuery = false,
                AffectedRows = affrows,
                Message = $"执行完成，影响 {affrows} 行"
            };
        }

        public void RefreshSchema()
        {
            _entities = null;
            _tables = null;
        }

        private List<BsDbAdminEntitySchema> GetEntitiesInternal()
        {
            _entities ??= DiscoverEntities()
                .OrderBy(x => x.FullTableName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return _entities;
        }

        private List<BsDbAdminEntitySchema> DiscoverEntities()
        {
            IEnumerable<Type> types = BsConfig.DbAdminEntityTypes.Count > 0
                ? BsConfig.DbAdminEntityTypes
                : AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !x.IsDynamic)
                    .SelectMany(GetLoadableTypes)
                    .Where(x => x.IsClass && !x.IsAbstract && x.GetCustomAttribute<TableAttribute>() != null);

            return types
                .Distinct()
                .Select(CreateEntitySchema)
                .Where(x => x.Columns.Count > 0)
                .ToList();
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null)!;
            }
        }

        private BsDbAdminEntitySchema CreateEntitySchema(Type entityType)
        {
            var table = _fsql.CodeFirst.GetTableByEntity(entityType);
            var schema = GetStringProperty(table, "Schema");
            var tableName = GetStringProperty(table, "DbName") ?? entityType.Name;

            var entity = new BsDbAdminEntitySchema
            {
                Key = entityType.FullName ?? entityType.Name,
                EntityName = entityType.Name,
                TableName = tableName,
                FullTableName = CombineSchemaAndName(schema, tableName),
                Schema = schema,
                Comment = GetStringProperty(table, "Comment"),
                EntityType = entityType,
                Columns = GetEntityColumns(table)
            };

            foreach (var column in entity.Columns)
            {
                column.EntityName = entity.EntityName;
            }

            return entity;
        }

        private static List<BsDbAdminEntityColumnSchema> GetEntityColumns(object table)
        {
            var columns = GetPropertyValue(table, "ColumnsByPosition") as Array;
            if (columns == null)
            {
                return new List<BsDbAdminEntityColumnSchema>();
            }

            return columns.Cast<object>().Select(CreateEntityColumnSchema).ToList();
        }

        private static BsDbAdminEntityColumnSchema CreateEntityColumnSchema(object column)
        {
            var attr = GetPropertyValue(column, "Attribute") ?? throw new InvalidOperationException("无法读取实体字段定义");
            var clrType = (Type)(GetPropertyValue(column, "CsType") ?? typeof(string));
            var realType = Nullable.GetUnderlyingType(clrType) ?? clrType;

            return new BsDbAdminEntityColumnSchema
            {
                PropertyName = GetStringProperty(column, "CsName") ?? string.Empty,
                DbColumnName = GetStringProperty(column, "DbName") ?? GetStringProperty(column, "CsName") ?? string.Empty,
                ClrType = GetFriendlyTypeName(realType),
                ClrTypeInfo = realType,
                DbType = GetStringProperty(column, "DbTypeText") ?? string.Empty,
                IsPrimary = GetBoolProperty(attr, "IsPrimary"),
                IsIdentity = GetBoolProperty(attr, "IsIdentity"),
                IsNullable = GetBoolProperty(attr, "IsNullable"),
                CanInsert = GetBoolProperty(attr, "CanInsert"),
                CanUpdate = GetBoolProperty(attr, "CanUpdate")
            };
        }

        private BsDbAdminTableSchema CreateTableSchema(DbTableInfo table, List<BsDbAdminEntitySchema> entities)
        {
            var fullName = CombineSchemaAndName(table.Schema, table.Name);
            var matchedEntities = entities
                .Where(x =>
                    string.Equals(x.FullTableName, fullName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.TableName, table.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return new BsDbAdminTableSchema
            {
                Key = fullName,
                Name = table.Name,
                FullName = fullName,
                Schema = table.Schema,
                Comment = table.Comment,
                Type = table.Type.ToString(),
                Entities = matchedEntities,
                Columns = table.Columns
                    .OrderBy(x => x.Position)
                    .Select(x => CreateTableColumnSchema(x, matchedEntities))
                    .ToList()
            };
        }

        private static BsDbAdminTableColumnSchema CreateTableColumnSchema(DbColumnInfo column, List<BsDbAdminEntitySchema> entities)
        {
            var realType = Nullable.GetUnderlyingType(column.CsType) ?? column.CsType;
            var bindings = entities
                .SelectMany(x => x.Columns.Where(y => string.Equals(y.DbColumnName, column.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return new BsDbAdminTableColumnSchema
            {
                Name = column.Name,
                ClrType = GetFriendlyTypeName(realType),
                ClrTypeInfo = realType,
                DbType = column.DbTypeText,
                DbTypeFull = string.IsNullOrWhiteSpace(column.DbTypeTextFull) ? column.DbTypeText : column.DbTypeTextFull,
                IsPrimary = column.IsPrimary,
                IsIdentity = column.IsIdentity,
                IsNullable = column.IsNullable,
                IsBoolean = realType == typeof(bool),
                IsDateTime = realType == typeof(DateTime) || realType == typeof(DateTimeOffset),
                IsNumeric = IsNumericType(realType),
                DefaultValue = column.DefaultValue,
                Comment = column.Comment,
                Position = column.Position,
                EntityBindings = bindings
            };
        }

        private List<BsDbAdminRowDto> BuildRows(BsDbAdminTableSchema table, DataTable dataTable)
        {
            var rows = new List<BsDbAdminRowDto>();

            foreach (DataRow row in dataTable.Rows)
            {
                var dto = new BsDbAdminRowDto();
                foreach (var column in table.Columns)
                {
                    var value = dataTable.Columns.Contains(column.Name) ? row[column.Name] : null;
                    var text = ConvertToString(value is DBNull ? null : value);
                    dto.Values[column.Name] = text;
                    if (column.IsPrimary)
                    {
                        dto.PrimaryKeyValues[column.Name] = text;
                    }
                }

                rows.Add(dto);
            }

            return rows;
        }

        private BsDbAdminRowDto GetRowByPrimaryKeys(BsDbAdminTableSchema table, Dictionary<string, string?> primaryKeys)
        {
            EnsurePrimaryKeys(table);
            var columnsSql = string.Join(", ", table.Columns.Select(x => QuoteIdentifier(x.Name)));
            var sql = $"SELECT {columnsSql} FROM {QuoteTable(table)} WHERE {BuildPrimaryKeyWhereSql(table, primaryKeys)}";
            var dataTable = _fsql.Ado.ExecuteDataTable(sql);
            if (dataTable.Rows.Count == 0)
            {
                throw new InvalidOperationException("未找到对应记录");
            }

            return BuildRows(table, dataTable).First();
        }

        private BsDbAdminRowDto GetLastInsertedRow(BsDbAdminTableSchema table)
        {
            var identity = table.Columns.FirstOrDefault(x => x.IsIdentity);
            if (identity == null)
            {
                throw new InvalidOperationException("无法定位最新插入记录");
            }

            var lastIdSql = _fsql.Ado.DataType switch
            {
                DataType.Sqlite => "SELECT last_insert_rowid()",
                DataType.MySql => "SELECT LAST_INSERT_ID()",
                DataType.PostgreSQL => $"SELECT currval(pg_get_serial_sequence('{table.FullName}','{identity.Name}'))",
                DataType.SqlServer => "SELECT SCOPE_IDENTITY()",
                _ => throw new InvalidOperationException("当前数据库暂不支持自动定位新记录，请刷新查看")
            };

            var lastId = _fsql.Ado.ExecuteScalar(lastIdSql);
            var primaryKeys = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                [identity.Name] = ConvertToString(lastId)
            };
            return GetRowByPrimaryKeys(table, primaryKeys);
        }

        private static Dictionary<string, string?> ExtractPrimaryKeys(BsDbAdminTableSchema table, Dictionary<string, string?> values)
        {
            var keys = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var column in table.Columns.Where(x => x.IsPrimary))
            {
                if (values.TryGetValue(column.Name, out var value))
                {
                    keys[column.Name] = value;
                }
            }

            return keys;
        }

        private string BuildPrimaryKeyWhereSql(BsDbAdminTableSchema table, Dictionary<string, string?> primaryKeys)
        {
            var clauses = new List<string>();
            foreach (var column in table.Columns.Where(x => x.IsPrimary))
            {
                if (!primaryKeys.TryGetValue(column.Name, out var value))
                {
                    throw new InvalidOperationException($"缺少主键字段 {column.Name}");
                }

                clauses.Add($"{QuoteIdentifier(column.Name)} = {BuildSqlLiteral(column, value)}");
            }

            return string.Join(" AND ", clauses);
        }

        private void EnsurePrimaryKeys(BsDbAdminTableSchema table)
        {
            if (!table.Columns.Any(x => x.IsPrimary))
            {
                throw new InvalidOperationException($"{table.FullName} 未定义主键，无法精确编辑或删除记录");
            }
        }

        private string BuildRenameColumnSql(BsDbAdminTableSchema table, string oldName, string newName)
        {
            var quotedTable = QuoteTable(table);
            var quotedOldName = QuoteIdentifier(oldName);
            var quotedNewName = QuoteIdentifier(newName);

            return _fsql.Ado.DataType switch
            {
                DataType.SqlServer => $"EXEC sp_rename '{table.FullName}.{oldName}', '{newName}', 'COLUMN'",
                _ => $"ALTER TABLE {quotedTable} RENAME COLUMN {quotedOldName} TO {quotedNewName}"
            };
        }

        private string BuildOrderBySql(BsDbAdminTableSchema table)
        {
            var columns = table.Columns.Where(x => x.IsPrimary).ToList();
            if (columns.Count == 0 && table.Columns.Count > 0)
            {
                columns.Add(table.Columns[0]);
            }

            if (columns.Count == 0)
            {
                return string.Empty;
            }

            return $" ORDER BY {string.Join(", ", columns.Select(x => QuoteIdentifier(x.Name)))}";
        }

        private string BuildPageSql(int pageIndex, int pageSize)
        {
            var offset = (pageIndex - 1) * pageSize;
            return _fsql.Ado.DataType switch
            {
                DataType.SqlServer => $" OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY",
                _ => $" LIMIT {pageSize} OFFSET {offset}"
            };
        }

        private string QuoteTable(BsDbAdminTableSchema table)
        {
            return string.IsNullOrWhiteSpace(table.Schema)
                ? QuoteIdentifier(table.Name)
                : $"{QuoteIdentifier(table.Schema)}.{QuoteIdentifier(table.Name)}";
        }

        private string QuoteTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new InvalidOperationException("表名不能为空");
            }

            var parts = tableName.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return string.Join(".", parts.Select(QuoteIdentifier));
        }

        private string QuoteIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("标识符不能为空");
            }

            return _fsql.Ado.DataType switch
            {
                DataType.SqlServer => $"[{name.Replace("]", "]]", StringComparison.Ordinal)}]",
                DataType.MySql => $"`{name.Replace("`", "``", StringComparison.Ordinal)}`",
                _ => $"\"{name.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            };
        }

        private string BuildSqlLiteral(BsDbAdminTableColumnSchema column, string? value)
        {
            if (value == null)
            {
                return "NULL";
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return column.ClrTypeInfo == typeof(string) ? "''" : "NULL";
            }

            if (column.IsBoolean)
            {
                return value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1" ? "1" : "0";
            }

            if (column.IsNumeric)
            {
                if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    throw new InvalidOperationException($"{column.Name} 不是合法数字");
                }

                return value;
            }

            if (column.ClrTypeInfo == typeof(Guid))
            {
                _ = Guid.Parse(value);
                return $"'{EscapeSqlString(value)}'";
            }

            if (column.IsDateTime)
            {
                if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
                {
                    return $"'{dto:O}'";
                }

                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                {
                    return $"'{dt:O}'";
                }

                throw new InvalidOperationException($"{column.Name} 不是合法时间");
            }

            return $"'{EscapeSqlString(value)}'";
        }

        private static string EscapeSqlString(string value)
        {
            return value.Replace("'", "''", StringComparison.Ordinal);
        }

        private static Dictionary<string, string?> BuildSqlRow(DataRow row)
        {
            var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn column in row.Table.Columns)
            {
                var value = row[column];
                result[column.ColumnName] = ConvertToString(value is DBNull ? null : value);
            }

            return result;
        }

        private static bool IsQuerySql(string sql)
        {
            return Regex.IsMatch(sql, @"^\s*(select|with|pragma|show|desc|describe|explain)\b", RegexOptions.IgnoreCase);
        }

        private static bool IsSchemaSql(string sql)
        {
            return Regex.IsMatch(sql, @"^\s*(create|alter|drop|rename|truncate)\b", RegexOptions.IgnoreCase);
        }

        private static string CombineSchemaAndName(string? schema, string name)
        {
            return string.IsNullOrWhiteSpace(schema) ? name : $"{schema}.{name}";
        }

        private static object? GetPropertyValue(object source, string propertyName)
        {
            return source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(source);
        }

        private static string? GetStringProperty(object source, string propertyName)
        {
            return GetPropertyValue(source, propertyName)?.ToString();
        }

        private static bool GetBoolProperty(object source, string propertyName)
        {
            return GetPropertyValue(source, propertyName) is bool value && value;
        }

        private static bool IsNumericType(Type type)
        {
            return type == typeof(byte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal);
        }

        private static string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int)) return "int";
            if (type == typeof(long)) return "long";
            if (type == typeof(short)) return "short";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(double)) return "double";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(DateTime)) return "DateTime";
            if (type == typeof(DateTimeOffset)) return "DateTimeOffset";
            if (type == typeof(Guid)) return "Guid";
            return type.Name;
        }

        private static string? ConvertToString(object? value)
        {
            if (value == null) return null;
            if (value is string text) return text;
            if (value is bool boolValue) return boolValue ? "true" : "false";
            if (value is DateTime dateTime) return dateTime.ToString("O", CultureInfo.InvariantCulture);
            if (value is DateTimeOffset dateTimeOffset) return dateTimeOffset.ToString("O", CultureInfo.InvariantCulture);
            if (value is IFormattable formattable) return formattable.ToString(null, CultureInfo.InvariantCulture);
            return value.ToString();
        }
    }
}
