#nullable enable
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlaScaf.Components.Pages
{
    public partial class DbAdmin
    {
        private const string DefaultCreateTableDefinition = "Id INTEGER PRIMARY KEY AUTOINCREMENT,\nName TEXT";
        private const string DefaultAddColumnDefinition = "NewColumn TEXT";

        [Inject] public BsDbAdminService DbAdminService { get; set; } = default!;
        [Inject] public MessageService MessageService { get; set; } = default!;
        [Inject] public UserService UserService { get; set; } = default!;

        private IReadOnlyList<BsDbAdminTableSchema> tables = Array.Empty<BsDbAdminTableSchema>();
        private BsDbAdminTableSchema? currentTable;
        private QueryRsp<List<BsDbAdminRowDto>> rowsRsp = new() { Value = new List<BsDbAdminRowDto>() };
        private BsDbAdminSqlResult? sqlResult;

        private bool isLoading;
        private bool isSaving;
        private bool isExecutingSql;
        private int pageIndex = 1;
        private readonly int pageSize = 15;

        private bool drawerVisible;
        private bool createTableDrawerVisible;
        private bool structureDrawerVisible;
        private bool sqlDrawerVisible;
        private bool isEditMode;
        private string drawerTitle = string.Empty;
        private BsDbAdminTableSchema? editingTable;
        private Dictionary<string, string?> editingPrimaryKeys = new(StringComparer.OrdinalIgnoreCase);
        private List<DbAdminFieldModel> editFields = new();

        private string createTableName = string.Empty;
        private string createTableDefinition = DefaultCreateTableDefinition;
        private string addColumnDefinition = DefaultAddColumnDefinition;
        private string renameOldColumnName = string.Empty;
        private string renameNewColumnName = string.Empty;
        private string dropColumnName = string.Empty;
        private string alterColumnName = string.Empty;
        private string alterColumnDefinition = string.Empty;
        private string sqlText = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await UserService.LoadUserInfoAsync();
            await RefreshTablesAsync();
        }

        private async Task RefreshTablesAsync()
        {
            var selectedKey = currentTable?.Key;
            tables = DbAdminService.GetTables(true);
            currentTable = selectedKey == null
                ? tables.FirstOrDefault()
                : tables.FirstOrDefault(x => string.Equals(x.Key, selectedKey, StringComparison.OrdinalIgnoreCase))
                    ?? tables.FirstOrDefault();

            pageIndex = 1;
            await LoadRowsAsync();
        }

        private async Task SelectTableAsync(string tableKey)
        {
            currentTable = tables.FirstOrDefault(x => string.Equals(x.Key, tableKey, StringComparison.OrdinalIgnoreCase));
            pageIndex = 1;
            await LoadRowsAsync();
        }

        private async Task HandleTableChanged(ChangeEventArgs args)
        {
            var tableKey = args.Value?.ToString();
            if (string.IsNullOrWhiteSpace(tableKey))
            {
                return;
            }

            await SelectTableAsync(tableKey);
        }

        private async Task ReloadCurrentTable()
        {
            await LoadRowsAsync();
        }

        private async Task LoadRowsAsync()
        {
            if (currentTable == null)
            {
                rowsRsp = new QueryRsp<List<BsDbAdminRowDto>> { Value = new List<BsDbAdminRowDto>() };
                await InvokeAsync(StateHasChanged);
                return;
            }

            try
            {
                isLoading = true;
                currentTable = DbAdminService.GetTable(currentTable.Key, true);
                rowsRsp = DbAdminService.GetRows(currentTable.Key, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                rowsRsp = new QueryRsp<List<BsDbAdminRowDto>> { Value = new List<BsDbAdminRowDto>() };
                await MessageService.ErrorAsync(ex.Message, 5);
            }
            finally
            {
                isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        private void OpenCreateDrawer()
        {
            if (currentTable == null) return;

            isEditMode = false;
            editingTable = currentTable;
            editingPrimaryKeys = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            drawerTitle = $"新增 {currentTable.Name} 记录";
            editFields = BuildFields(currentTable, null, false);
            drawerVisible = true;
        }

        private void OpenCreateTableDrawer()
        {
            createTableName = string.Empty;
            createTableDefinition = DefaultCreateTableDefinition;
            createTableDrawerVisible = true;
        }

        private void CloseCreateTableDrawer()
        {
            createTableDrawerVisible = false;
        }

        private void OpenStructureDrawer()
        {
            if (currentTable == null) return;

            addColumnDefinition = DefaultAddColumnDefinition;
            renameOldColumnName = string.Empty;
            renameNewColumnName = string.Empty;
            dropColumnName = string.Empty;
            alterColumnName = string.Empty;
            alterColumnDefinition = string.Empty;
            structureDrawerVisible = true;
        }

        private void CloseStructureDrawer()
        {
            structureDrawerVisible = false;
        }

        private void OpenSqlDrawer()
        {
            sqlDrawerVisible = true;
        }

        private void CloseSqlDrawer()
        {
            sqlDrawerVisible = false;
        }

        private void OpenEditDrawer(BsDbAdminRowDto row)
        {
            if (currentTable == null) return;

            isEditMode = true;
            editingTable = currentTable;
            editingPrimaryKeys = new Dictionary<string, string?>(row.PrimaryKeyValues, StringComparer.OrdinalIgnoreCase);
            drawerTitle = $"编辑 {currentTable.Name} 记录";
            editFields = BuildFields(currentTable, row, true);
            drawerVisible = true;
        }

        private List<DbAdminFieldModel> BuildFields(BsDbAdminTableSchema table, BsDbAdminRowDto? row, bool isEdit)
        {
            var result = new List<DbAdminFieldModel>();
            foreach (var column in table.Columns)
            {
                if (!isEdit && column.IsIdentity)
                {
                    continue;
                }

                var value = row != null && row.Values.TryGetValue(column.Name, out var existing)
                    ? existing
                    : null;

                result.Add(new DbAdminFieldModel
                {
                    Column = column,
                    Value = value,
                    ReadOnly = isEdit && column.IsPrimary
                });
            }

            return result;
        }

        private async Task SaveAsync()
        {
            if (editingTable == null) return;

            try
            {
                isSaving = true;
                var values = editFields.ToDictionary(x => x.Column.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

                if (isEditMode)
                {
                    DbAdminService.Update(editingTable.Key, editingPrimaryKeys, values);
                    AddOptLog("数据库编辑", editingTable.FullName, string.Join(", ", editingPrimaryKeys.Select(x => $"{x.Key}={x.Value}")));
                    await MessageService.SuccessAsync("记录已更新", 3);
                }
                else
                {
                    var inserted = DbAdminService.Insert(editingTable.Key, values);
                    AddOptLog("数据库新增", editingTable.FullName, string.Join(", ", inserted.PrimaryKeyValues.Select(x => $"{x.Key}={x.Value}")));
                    await MessageService.SuccessAsync("记录已新增", 3);
                }

                drawerVisible = false;
                await LoadRowsAsync();
            }
            catch (Exception ex)
            {
                await MessageService.ErrorAsync(ex.Message, 5);
            }
            finally
            {
                isSaving = false;
            }
        }

        private async Task DeleteRowAsync(BsDbAdminRowDto row)
        {
            if (currentTable == null) return;

            try
            {
                DbAdminService.Delete(currentTable.Key, row.PrimaryKeyValues);
                AddOptLog("数据库删除", currentTable.FullName, string.Join(", ", row.PrimaryKeyValues.Select(x => $"{x.Key}={x.Value}")));
                await MessageService.SuccessAsync("记录已删除", 3);
                await LoadRowsAsync();
            }
            catch (Exception ex)
            {
                await MessageService.ErrorAsync(ex.Message, 5);
            }
        }

        private async Task CreateTableAsync()
        {
            try
            {
                DbAdminService.CreateTable(createTableName, createTableDefinition);
                AddOptLog("数据库建表", createTableName, createTableDefinition);
                await MessageService.SuccessAsync("数据表已创建", 3);
                await RefreshTablesAsync();
                currentTable = tables.FirstOrDefault(x => string.Equals(x.Name, createTableName, StringComparison.OrdinalIgnoreCase) || string.Equals(x.FullName, createTableName, StringComparison.OrdinalIgnoreCase));
                await LoadRowsAsync();
                createTableDrawerVisible = false;
            }
            catch (Exception ex)
            {
                await MessageService.ErrorAsync(ex.Message, 5);
            }
        }

        private async Task DropCurrentTableAsync()
        {
            if (currentTable == null) return;

            try
            {
                var tableName = currentTable.FullName;
                DbAdminService.DropTable(currentTable.Key);
                AddOptLog("数据库删表", tableName, string.Empty);
                await MessageService.SuccessAsync("数据表已删除", 3);
                await RefreshTablesAsync();
            }
            catch (Exception ex)
            {
                await MessageService.ErrorAsync(ex.Message, 5);
            }
        }

        private async Task AddColumnAsync()
        {
            if (currentTable == null) return;

            try
            {
                DbAdminService.AddColumn(currentTable.Key, addColumnDefinition);
                AddOptLog("数据库加字段", currentTable.FullName, addColumnDefinition);
                await MessageService.SuccessAsync("字段已新增", 3);
                await RefreshCurrentTableStructureAsync();
            }
            catch (Exception ex)
            {
                await MessageService.ErrorAsync(ex.Message, 5);
            }
        }

        private async Task RenameColumnAsync()
        {
            if (currentTable == null) return;

            try
            {
                DbAdminService.RenameColumn(currentTable.Key, renameOldColumnName, renameNewColumnName);
                AddOptLog("数据库改字段名", currentTable.FullName, $"{renameOldColumnName} => {renameNewColumnName}");
                await MessageService.SuccessAsync("字段已重命名", 3);
                await RefreshCurrentTableStructureAsync();
            }
            catch (Exception ex)
            {
                await MessageService.ErrorAsync(ex.Message, 5);
            }
        }

        private async Task DropColumnAsync()
        {
            if (currentTable == null) return;

            try
            {
                DbAdminService.DropColumn(currentTable.Key, dropColumnName);
                AddOptLog("数据库删字段", currentTable.FullName, dropColumnName);
                await MessageService.SuccessAsync("字段已删除", 3);
                await RefreshCurrentTableStructureAsync();
            }
            catch (Exception ex)
            {
                await MessageService.ErrorAsync(ex.Message, 5);
            }
        }

        private void GenerateCreateTableSql()
        {
            WriteSqlToConsole(DbAdminService.BuildCreateTableSql(createTableName, createTableDefinition));
        }

        private void GenerateAddColumnSql()
        {
            if (currentTable == null) return;
            WriteSqlToConsole(DbAdminService.BuildAddColumnSql(currentTable.Key, addColumnDefinition));
        }

        private void GenerateRenameColumnSql()
        {
            if (currentTable == null) return;
            WriteSqlToConsole(DbAdminService.BuildRenameColumnSql(currentTable.Key, renameOldColumnName, renameNewColumnName));
        }

        private void GenerateDropColumnSql()
        {
            if (currentTable == null) return;
            WriteSqlToConsole(DbAdminService.BuildDropColumnSql(currentTable.Key, dropColumnName));
        }

        private void GenerateAlterColumnSql()
        {
            if (currentTable == null) return;
            WriteSqlToConsole(DbAdminService.BuildAlterColumnTemplate(currentTable.Key, alterColumnName, alterColumnDefinition));
        }

        private async Task ExecuteSqlAsync()
        {
            try
            {
                isExecutingSql = true;
                sqlResult = DbAdminService.ExecuteSql(sqlText);
                AddOptLog("数据库执行SQL", currentTable?.FullName ?? "SQL", sqlText.Length > 300 ? sqlText[..300] : sqlText);
                await MessageService.SuccessAsync(sqlResult.Message, 3);

                if (currentTable != null)
                {
                    await RefreshCurrentTableStructureAsync();
                }
            }
            catch (Exception ex)
            {
                await MessageService.ErrorAsync(ex.Message, 5);
            }
            finally
            {
                isExecutingSql = false;
            }
        }

        private void ClearSqlResult()
        {
            sqlResult = null;
        }

        private void WriteSqlToConsole(string sql)
        {
            sqlText = sql;
            sqlResult = null;
            sqlDrawerVisible = true;
        }

        private async Task RefreshCurrentTableStructureAsync()
        {
            if (currentTable == null)
            {
                await RefreshTablesAsync();
                return;
            }

            var currentKey = currentTable.Key;
            tables = DbAdminService.GetTables(true);
            currentTable = tables.FirstOrDefault(x => string.Equals(x.Key, currentKey, StringComparison.OrdinalIgnoreCase))
                ?? tables.FirstOrDefault();
            pageIndex = 1;
            await LoadRowsAsync();
        }

        private async Task PreviousPage()
        {
            if (pageIndex <= 1) return;
            pageIndex--;
            await LoadRowsAsync();
        }

        private async Task NextPage()
        {
            if (pageIndex >= GetTotalPages()) return;
            pageIndex++;
            await LoadRowsAsync();
        }

        private int GetTotalPages()
        {
            return Math.Max(1, (int)Math.Ceiling(rowsRsp.Total / (double)pageSize));
        }

        private static string GetColumnFlags(BsDbAdminTableColumnSchema column)
        {
            var tags = new List<string>();
            if (column.IsPrimary) tags.Add("PK");
            if (column.IsIdentity) tags.Add("Identity");
            tags.Add(column.IsNullable ? "Nullable" : "Not Null");
            return string.Join(" / ", tags);
        }

        private IEnumerable<string> GetEntityOnlyColumns(BsDbAdminEntitySchema entity)
        {
            if (currentTable == null)
            {
                return Array.Empty<string>();
            }

            return entity.Columns
                .Where(x => currentTable.Columns.All(y => !string.Equals(y.Name, x.DbColumnName, StringComparison.OrdinalIgnoreCase)))
                .Select(x => $"{x.DbColumnName} <- {x.PropertyName}")
                .ToList();
        }

        private string GetCellValue(BsDbAdminRowDto row, BsDbAdminTableColumnSchema column)
        {
            if (!row.Values.TryGetValue(column.Name, out var value) || string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (column.IsBoolean)
            {
                return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1" ? "是" : "否";
            }

            if (column.IsDateTime && DateTimeOffset.TryParse(value, out var dateTime))
            {
                return dateTime.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return value.Length > 120 ? $"{value[..120]}..." : value;
        }

        private static string GetSqlCellValue(Dictionary<string, string?> row, string column)
        {
            return row.TryGetValue(column, out var value) && !string.IsNullOrEmpty(value)
                ? (value.Length > 120 ? $"{value[..120]}..." : value)
                : string.Empty;
        }

        private void AddOptLog(string optType, string summary, string detail)
        {
            BsConfig.AddOptLog?.Invoke(new BsOptLog
            {
                UserId = UserService.UserId,
                UserName = UserService.UserName,
                OptType = optType,
                Summary = summary,
                OptData = detail,
                OptObjId = 0
            });
        }

        private void CloseDrawer()
        {
            drawerVisible = false;
            editingTable = null;
            editingPrimaryKeys.Clear();
            editFields.Clear();
        }

        private sealed class DbAdminFieldModel
        {
            public BsDbAdminTableColumnSchema Column { get; set; } = default!;

            public string? Value { get; set; }

            public bool ReadOnly { get; set; }

            public bool BoolValue
            {
                get => string.Equals(Value, "true", StringComparison.OrdinalIgnoreCase) || Value == "1";
                set => Value = value ? "true" : "false";
            }
        }
    }
}
