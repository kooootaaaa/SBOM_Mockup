using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SBOMSystemAPI.Models;
using System.Data;

namespace SBOMSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public PartsController(IConfiguration configuration)
        {
            _configuration = configuration;
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

        [HttpGet("structure")]
        public async Task<IActionResult> GetTableStructure()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        SELECT 
                            COLUMN_NAME as ColumnName,
                            DATA_TYPE as DataType,
                            CHARACTER_MAXIMUM_LENGTH as MaxLength,
                            IS_NULLABLE as IsNullable,
                            COLUMN_DEFAULT as DefaultValue
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'T_部品マスタ'
                        ORDER BY ORDINAL_POSITION";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            
                            var columns = dataTable.AsEnumerable().Select(row => new
                            {
                                ColumnName = row["ColumnName"].ToString(),
                                DataType = row["DataType"].ToString(),
                                MaxLength = row["MaxLength"]?.ToString(),
                                IsNullable = row["IsNullable"].ToString(),
                                DefaultValue = row["DefaultValue"]?.ToString()
                            }).ToList();
                            
                            return Ok(columns);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("sample")]
        public async Task<IActionResult> GetSampleData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = "SELECT TOP 5 * FROM T_部品マスタ";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            
                            var data = new List<Dictionary<string, object>>();
                            
                            foreach (DataRow row in dataTable.Rows)
                            {
                                var rowData = new Dictionary<string, object>();
                                foreach (DataColumn column in dataTable.Columns)
                                {
                                    rowData[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                                }
                                data.Add(rowData);
                            }
                            
                            return Ok(data);
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
        public async Task<IActionResult> GetParts([FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string baseQuery = "SELECT * FROM T_部品マスタ";
                    string whereClause = "";
                    
                    if (!string.IsNullOrEmpty(search))
                    {
                        whereClause = " WHERE 品番 LIKE @search OR 品名 LIKE @search";
                    }
                    
                    string query = baseQuery + whereClause + $" ORDER BY 部品ID OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(search))
                        {
                            command.Parameters.AddWithValue("@search", $"%{search}%");
                        }
                        
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            
                            var data = new List<Dictionary<string, object>>();
                            
                            foreach (DataRow row in dataTable.Rows)
                            {
                                var rowData = new Dictionary<string, object>();
                                foreach (DataColumn column in dataTable.Columns)
                                {
                                    rowData[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                                }
                                data.Add(rowData);
                            }
                            
                            return Ok(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterIndividualParts([FromBody] IndividualPartsRegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var now = DateTime.Now;
                        int registeredPartsCount = 0;
                        int registeredChildRelationsCount = 0;

                        // 1. 既存の個体部品データを廃止（廃止日を設定）
                        string deletePartsQuery = @"
                            UPDATE T_個体部品サブ 
                            SET 廃止日 = @Now 
                            WHERE 機械管理ID = @MachineId AND 廃止日 IS NULL";

                        using (SqlCommand deletePartsCmd = new SqlCommand(deletePartsQuery, connection, transaction))
                        {
                            deletePartsCmd.Parameters.AddWithValue("@Now", now);
                            deletePartsCmd.Parameters.AddWithValue("@MachineId", request.MachineId);
                            await deletePartsCmd.ExecuteNonQueryAsync();
                        }

                        // 2. 既存の個体部品子部品データを廃止
                        string deleteChildPartsQuery = @"
                            UPDATE T_個体部品子部品サブ 
                            SET 廃止日 = @Now 
                            WHERE 機械管理ID = @MachineId AND 廃止日 IS NULL";

                        using (SqlCommand deleteChildPartsCmd = new SqlCommand(deleteChildPartsQuery, connection, transaction))
                        {
                            deleteChildPartsCmd.Parameters.AddWithValue("@Now", now);
                            deleteChildPartsCmd.Parameters.AddWithValue("@MachineId", request.MachineId);
                            await deleteChildPartsCmd.ExecuteNonQueryAsync();
                        }

                        // 3. T_個体部品サブに最上位部品を登録
                        if (request.DirectParts.Count > 0)
                        {
                            string insertPartsQuery = @"
                                INSERT INTO T_個体部品サブ
                                (個体部品ID, 個体ID, 機械管理ID, 部品ID, 個体IDごと連番, 個数, 登録日)
                                VALUES
                                (@個体部品ID, @個体ID, @機械管理ID, @部品ID, @個体IDごと連番, @個数, @登録日)";

                            foreach (var part in request.DirectParts)
                            {
                                // 個体部品ID = 機械管理ID-部品ID-連番(00形式)
                                string 個体部品ID = $"{request.MachineId}-{part.PartId}-{part.SequenceNo:D2}";

                                using (SqlCommand insertPartsCmd = new SqlCommand(insertPartsQuery, connection, transaction))
                                {
                                    insertPartsCmd.Parameters.AddWithValue("@個体部品ID", 個体部品ID);
                                    insertPartsCmd.Parameters.AddWithValue("@個体ID", DBNull.Value);
                                    insertPartsCmd.Parameters.AddWithValue("@機械管理ID", request.MachineId);
                                    insertPartsCmd.Parameters.AddWithValue("@部品ID", part.PartId);
                                    insertPartsCmd.Parameters.AddWithValue("@個体IDごと連番", (short)part.SequenceNo);
                                    insertPartsCmd.Parameters.AddWithValue("@個数", (short)part.Quantity);
                                    insertPartsCmd.Parameters.AddWithValue("@登録日", now);

                                    await insertPartsCmd.ExecuteNonQueryAsync();
                                    registeredPartsCount++;
                                }
                            }
                        }

                        // 4. T_個体部品子部品サブに親子関係を登録
                        if (request.ChildRelations.Count > 0)
                        {
                            string insertChildQuery = @"
                                INSERT INTO T_個体部品子部品サブ
                                (親子ID, 個体ID, 機械管理ID, 親部品コード, 子部品コード, 連番, 個数, 登録日)
                                VALUES
                                (@親子ID, @個体ID, @機械管理ID, @親部品コード, @子部品コード, @連番, @個数, @登録日)";

                            foreach (var childRelation in request.ChildRelations)
                            {
                                // 親子ID = 機械管理ID-親部品コード-子部品コード-連番(00形式)
                                string 親子ID = $"{request.MachineId}-{childRelation.ParentPartCode}-{childRelation.ChildPartCode}-{childRelation.SequenceNo:D2}";

                                using (SqlCommand insertChildCmd = new SqlCommand(insertChildQuery, connection, transaction))
                                {
                                    insertChildCmd.Parameters.AddWithValue("@親子ID", 親子ID);
                                    insertChildCmd.Parameters.AddWithValue("@個体ID", DBNull.Value);
                                    insertChildCmd.Parameters.AddWithValue("@機械管理ID", request.MachineId);
                                    insertChildCmd.Parameters.AddWithValue("@親部品コード", childRelation.ParentPartCode);
                                    insertChildCmd.Parameters.AddWithValue("@子部品コード", childRelation.ChildPartCode);
                                    insertChildCmd.Parameters.AddWithValue("@連番", (short)childRelation.SequenceNo);
                                    insertChildCmd.Parameters.AddWithValue("@個数", (short)childRelation.Quantity);
                                    insertChildCmd.Parameters.AddWithValue("@登録日", now);

                                    await insertChildCmd.ExecuteNonQueryAsync();
                                    registeredChildRelationsCount++;
                                }
                            }
                        }

                        transaction.Commit();

                        return Ok(new IndividualPartsRegisterResponse
                        {
                            Success = true,
                            Message = "個体部品情報の登録が完了しました。",
                            RegisteredPartsCount = registeredPartsCount,
                            RegisteredChildRelationsCount = registeredChildRelationsCount,
                            RegisteredAt = now
                        });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return BadRequest(new { error = $"登録処理でエラーが発生しました: {ex.Message}" });
                    }
                }
            }
        }
    }
}