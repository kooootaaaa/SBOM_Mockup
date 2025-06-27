using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace SBOMSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineController : ControllerBase
    {
        private readonly string _connectionString;

        public MachineController()
        {
            _connectionString = "Server=192.168.1.19;Database=WeBOMSQL;Integrated Security=true;TrustServerCertificate=true;";
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestConnection()
        {
            try
        {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return Ok(new { message = "SQL Server接続成功", timestamp = DateTime.Now });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("tables")]
        public async Task<IActionResult> GetAllTables()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        SELECT TABLE_NAME as TableName, TABLE_TYPE as TableType
                        FROM INFORMATION_SCHEMA.TABLES 
                        ORDER BY TABLE_NAME";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            
                            var tables = dataTable.AsEnumerable().Select(row => new
                            {
                                TableName = row["TableName"].ToString(),
                                TableType = row["TableType"].ToString()
                            }).ToList();
                            
                            return Ok(tables);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("check-machine-table")]
        public async Task<IActionResult> CheckMachineTable()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // テーブルの存在確認
                    string existQuery = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = 'T_機械管理台帳'";
                    
                    using (SqlCommand existCommand = new SqlCommand(existQuery, connection))
                    {
                        int tableExists = (int)await existCommand.ExecuteScalarAsync();
                        
                        if (tableExists == 0)
                        {
                            return Ok(new { exists = false, message = "T_機械管理台帳テーブルが見つかりません。" });
                        }
                    }
                    
                    // テーブル構造を取得
                    string structureQuery = @"
                        SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'T_機械管理台帳'
                        ORDER BY ORDINAL_POSITION";
                    
                    using (SqlCommand command = new SqlCommand(structureQuery, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            
                            var columns = dataTable.AsEnumerable().Select(row => new
                            {
                                ColumnName = row["COLUMN_NAME"].ToString(),
                                DataType = row["DATA_TYPE"].ToString(),
                                MaxLength = row["CHARACTER_MAXIMUM_LENGTH"]?.ToString(),
                                IsNullable = row["IS_NULLABLE"].ToString()
                            }).ToList();
                            
                            // 主キー情報を取得
                            string primaryKeyQuery = @"
                                SELECT COLUMN_NAME
                                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                                WHERE TABLE_NAME = 'T_機械管理台帳' 
                                AND CONSTRAINT_NAME LIKE 'PK_%'";
                            
                            var primaryKeys = new List<string>();
                            using (SqlCommand pkCommand = new SqlCommand(primaryKeyQuery, connection))
                            {
                                using (SqlDataReader reader = pkCommand.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        primaryKeys.Add(reader.GetString(0));
                                    }
                                }
                            }
                            
                            // IDENTITY列情報を取得
                            string identityQuery = @"
                                SELECT COLUMN_NAME
                                FROM INFORMATION_SCHEMA.COLUMNS
                                WHERE TABLE_NAME = 'T_機械管理台帳'
                                AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1";
                            
                            var identityColumns = new List<string>();
                            using (SqlCommand idCommand = new SqlCommand(identityQuery, connection))
                            {
                                using (SqlDataReader reader = idCommand.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        identityColumns.Add(reader.GetString(0));
                                    }
                                }
                            }
                            
                            return Ok(new { 
                                exists = true, 
                                columns = columns,
                                primaryKeys = primaryKeys,
                                identityColumns = identityColumns
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMachines([FromQuery] int page = 1, [FromQuery] int pageSize = 15)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // 全件数を取得
                    string countQuery = "SELECT COUNT(*) FROM T_機械管理台帳";
                    int totalRecords;
                    using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                    {
                        totalRecords = (int)await countCommand.ExecuteScalarAsync();
                    }
                    
                    // ページネーション付きでデータを取得（RegDate降順）
                    string query = @"
                        SELECT * FROM T_機械管理台帳 
                        ORDER BY RegDate DESC 
                        OFFSET @Offset ROWS 
                        FETCH NEXT @PageSize ROWS ONLY";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@PageSize", pageSize);
                        
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            
                            var machines = new List<Dictionary<string, object>>();
                            
                            foreach (DataRow row in dataTable.Rows)
                            {
                                var machineData = new Dictionary<string, object>();
                                foreach (DataColumn column in dataTable.Columns)
                                {
                                    machineData[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                                }
                                machines.Add(machineData);
                            }
                            
                            return Ok(new
                            {
                                machines = machines,
                                totalRecords = totalRecords,
                                currentPage = page,
                                pageSize = pageSize,
                                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // 製造番号で取得する既存のエンドポイント（互換性のため残す）
        [HttpGet("by-serial/{serialNumber}")]
        public async Task<IActionResult> GetMachineDetailBySerial(string serialNumber)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // まず、全カラムを取得するクエリを実行
                    string query = @"
                        SELECT * 
                        FROM T_機械管理台帳 
                        WHERE 製造番号 = @SerialNumber";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SerialNumber", serialNumber);
                        
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var machine = new Dictionary<string, object>();
                                
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var fieldName = reader.GetName(i);
                                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    machine[fieldName] = value;
                                }
                                
                                return Ok(machine);
                            }
                            else
                            {
                                return NotFound(new { error = $"製造番号 {serialNumber} の機械が見つかりません。" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // 機械管理IDで取得する新しいエンドポイント
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMachineDetail(string id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // まず機械管理IDカラムが存在するか確認
                    string checkColumnQuery = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'T_機械管理台帳' 
                        AND COLUMN_NAME = '機械管理ID'";
                    
                    bool hasManagementId = false;
                    using (SqlCommand checkCommand = new SqlCommand(checkColumnQuery, connection))
                    {
                        hasManagementId = (int)await checkCommand.ExecuteScalarAsync() > 0;
                    }
                    
                    // 機械管理IDカラムが存在する場合はそれを使用、存在しない場合は製造番号を使用
                    string query;
                    if (hasManagementId)
                    {
                        query = @"
                            SELECT * 
                            FROM T_機械管理台帳 
                            WHERE 機械管理ID = @MachineId";
                    }
                    else
                    {
                        // 機械管理IDが存在しない場合は製造番号で検索（後方互換性）
                        query = @"
                            SELECT * 
                            FROM T_機械管理台帳 
                            WHERE 製造番号 = @MachineId";
                    }
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MachineId", id);
                        
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var machine = new Dictionary<string, object>();
                                
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var fieldName = reader.GetName(i);
                                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    machine[fieldName] = value;
                                }
                                
                                return Ok(machine);
                            }
                            else
                            {
                                return NotFound(new { error = $"ID {id} の機械が見つかりません。" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMachine(string id, [FromBody] Dictionary<string, object> machineData)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // まず機械管理IDカラムが存在するか確認
                    string checkColumnQuery = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'T_機械管理台帳' 
                        AND COLUMN_NAME = '機械管理ID'";
                    
                    bool hasManagementId = false;
                    using (SqlCommand checkCommand = new SqlCommand(checkColumnQuery, connection))
                    {
                        hasManagementId = (int)await checkCommand.ExecuteScalarAsync() > 0;
                    }
                    
                    // 更新用のSQLを構築
                    var updateFields = new List<string>();
                    var parameters = new List<SqlParameter>();
                    
                    // IDパラメータを追加
                    parameters.Add(new SqlParameter("@MachineId", id));
                    
                    // 更新可能なフィールドのマッピング
                    var fieldMappings = new Dictionary<string, string>
                    {
                        { "型式", "@Model" },
                        { "ItemSubCode", "@BranchNo" },
                        { "機種名", "@TypeName" },
                        { "ロット番号", "@LotNo" },
                        { "ロット連番", "@LotSerial" },
                        { "号機番号", "@MachineNo" },
                        { "案件番号", "@ProjectNo" },
                        { "中古番号", "@UsedProjectNo" },
                        { "タンク製造番号", "@TankNo" },
                        { "OrderSerialNo", "@OrderId" },
                        { "預けNo", "@DepositNo" },
                        { "河北製造番号", "@KahokuId" },
                        { "過去製造番号", "@PastId" },
                        { "備考1", "@InternalMemo" },
                        { "マスタ外登録CH", "@IsMasterOut" },
                        { "傷有FLG", "@IsDifficult" },
                        { "非在庫品FLG", "@IsNonStock" },
                        { "製造種別", "@ProductionType" },
                        { "現所在地", "@Location" },
                        { "機械状態", "@Status" },
                        { "管理区分", "@ManagementType" },
                        { "完成予定日", "@ScheduledDate" },
                        { "仕掛品完成日", "@InProcessDate" },
                        { "完成日", "@CompletionDate" },
                        { "CR登録日", "@CRDate" },
                        { "メーカーオプション", "@MakerOption" },
                        { "客先仕様", "@CustomerSpec" },
                        { "仕様変更履歴", "@SpecHistory" },
                        { "改造日", "@ModifyDate" },
                        { "製造時備考", "@AssemblyNote" },
                        { "改造内容", "@ModifyContent" }
                    };
                    
                    foreach (var field in fieldMappings)
                    {
                        if (machineData.ContainsKey(field.Key))
                        {
                            updateFields.Add($"{field.Key} = {field.Value}");
                            var value = machineData[field.Key];
                            
                            // JsonElementの場合は適切な型に変換
                            if (value is JsonElement element)
                            {
                                switch (element.ValueKind)
                                {
                                    case JsonValueKind.String:
                                        var stringValue = element.GetString();
                                        if (string.IsNullOrEmpty(stringValue))
                                        {
                                            parameters.Add(new SqlParameter(field.Value, DBNull.Value));
                                        }
                                        else
                                        {
                                            parameters.Add(new SqlParameter(field.Value, stringValue));
                                        }
                                        break;
                                    case JsonValueKind.Number:
                                        if (element.TryGetInt32(out int intValue))
                                        {
                                            parameters.Add(new SqlParameter(field.Value, intValue));
                                        }
                                        else if (element.TryGetDecimal(out decimal decimalValue))
                                        {
                                            parameters.Add(new SqlParameter(field.Value, decimalValue));
                                        }
                                        else
                                        {
                                            parameters.Add(new SqlParameter(field.Value, element.GetDouble()));
                                        }
                                        break;
                                    case JsonValueKind.True:
                                    case JsonValueKind.False:
                                        parameters.Add(new SqlParameter(field.Value, element.GetBoolean() ? 1 : 0));
                                        break;
                                    case JsonValueKind.Null:
                                        parameters.Add(new SqlParameter(field.Value, DBNull.Value));
                                        break;
                                    default:
                                        parameters.Add(new SqlParameter(field.Value, DBNull.Value));
                                        break;
                                }
                            }
                            else if (value == null || (value is string str && string.IsNullOrEmpty(str)))
                            {
                                parameters.Add(new SqlParameter(field.Value, DBNull.Value));
                            }
                            else
                            {
                                parameters.Add(new SqlParameter(field.Value, value));
                            }
                        }
                    }
                    
                    if (updateFields.Count == 0)
                    {
                        return BadRequest(new { error = "更新するフィールドがありません。" });
                    }
                    
                    // WHERE句を構築（機械管理IDカラムの有無で切り替え）
                    string whereClause = hasManagementId ? "WHERE 機械管理ID = @MachineId" : "WHERE 製造番号 = @MachineId";
                    
                    string updateQuery = $@"
                        UPDATE T_機械管理台帳 
                        SET {string.Join(", ", updateFields)}
                        {whereClause}";
                    
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        
                        if (rowsAffected > 0)
                        {
                            return Ok(new { message = "更新に成功しました。", rowsAffected = rowsAffected });
                        }
                        else
                        {
                            return NotFound(new { error = $"ID {id} の機械が見つかりません。" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}