#nullable enable

namespace BlaScaf
{
    public class BsDbAdminEntitySchema
    {
        public string Key { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public string TableName { get; set; } = string.Empty;

        public string FullTableName { get; set; } = string.Empty;

        public string? Schema { get; set; }

        public string? Comment { get; set; }

        public Type EntityType { get; set; } = typeof(object);

        public List<BsDbAdminEntityColumnSchema> Columns { get; set; } = new();
    }

    public class BsDbAdminEntityColumnSchema
    {
        public string EntityName { get; set; } = string.Empty;

        public string PropertyName { get; set; } = string.Empty;

        public string DbColumnName { get; set; } = string.Empty;

        public string ClrType { get; set; } = string.Empty;

        public Type ClrTypeInfo { get; set; } = typeof(string);

        public string DbType { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsNullable { get; set; }

        public bool CanInsert { get; set; }

        public bool CanUpdate { get; set; }
    }

    public class BsDbAdminTableSchema
    {
        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? Schema { get; set; }

        public string? Comment { get; set; }

        public string Type { get; set; } = string.Empty;

        public List<BsDbAdminTableColumnSchema> Columns { get; set; } = new();

        public List<BsDbAdminEntitySchema> Entities { get; set; } = new();
    }

    public class BsDbAdminTableColumnSchema
    {
        public string Name { get; set; } = string.Empty;

        public string ClrType { get; set; } = string.Empty;

        public Type ClrTypeInfo { get; set; } = typeof(string);

        public string DbType { get; set; } = string.Empty;

        public string DbTypeFull { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsNullable { get; set; }

        public bool IsBoolean { get; set; }

        public bool IsDateTime { get; set; }

        public bool IsNumeric { get; set; }

        public string? DefaultValue { get; set; }

        public string? Comment { get; set; }

        public int Position { get; set; }

        public List<BsDbAdminEntityColumnSchema> EntityBindings { get; set; } = new();
    }

    public class BsDbAdminRowDto
    {
        public Dictionary<string, string?> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string?> PrimaryKeyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public class BsDbAdminSqlResult
    {
        public bool IsQuery { get; set; }

        public int AffectedRows { get; set; }

        public string Message { get; set; } = string.Empty;

        public List<string> Columns { get; set; } = new();

        public List<Dictionary<string, string?>> Rows { get; set; } = new();
    }
}
