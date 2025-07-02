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

        [HttpGet("individual/{machineId}")]
        public async Task<IActionResult> GetIndividualParts(string machineId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var response = new IndividualPartsGetResponse();

                    // 1. T_個体部品サブとT_部品マスタを結合して機械の個体部品を取得
                    string partsQuery = @"
                        SELECT 
                            i.個体部品ID, i.個体ID, i.機械管理ID, i.部品ID, i.個体IDごと連番, i.個数,
                            i.廃止日, i.廃止時改訂ID, i.登録日, i.登録時改訂ID,
                            p.品番, p.品名, p.メーカー, p.材質, p.型式, p.備考,
                            p.部品種別, p.ユニット種別, p.オプションユニットFL
                        FROM T_個体部品サブ i
                        LEFT JOIN T_部品マスタ p ON i.部品ID = p.部品ID
                        WHERE i.機械管理ID = @machineId AND i.廃止日 IS NULL
                        ORDER BY p.品番, i.個体IDごと連番";

                    using (SqlCommand partsCmd = new SqlCommand(partsQuery, connection))
                    {
                        partsCmd.Parameters.AddWithValue("@machineId", machineId);

                        using (SqlDataAdapter partsAdapter = new SqlDataAdapter(partsCmd))
                        {
                            DataTable partsTable = new DataTable();
                            partsAdapter.Fill(partsTable);

                            foreach (DataRow row in partsTable.Rows)
                            {
                                response.Parts.Add(new IndividualPartDetailModel
                                {
                                    個体部品ID = row["個体部品ID"]?.ToString() ?? "",
                                    個体ID = row["個体ID"]?.ToString(),
                                    機械管理ID = row["機械管理ID"]?.ToString() ?? "",
                                    部品ID = row["部品ID"]?.ToString() ?? "",
                                    個体IDごと連番 = Convert.ToInt16(row["個体IDごと連番"]),
                                    個数 = Convert.ToInt16(row["個数"]),
                                    廃止日 = row["廃止日"] == DBNull.Value ? null : Convert.ToDateTime(row["廃止日"]),
                                    廃止時改訂ID = row["廃止時改訂ID"]?.ToString(),
                                    登録日 = Convert.ToDateTime(row["登録日"]),
                                    登録時改訂ID = row["登録時改訂ID"]?.ToString(),
                                    品番 = row["品番"]?.ToString() ?? "",
                                    品名 = row["品名"]?.ToString() ?? "",
                                    単位 = "", // 単位カラムは存在しない
                                    メーカー = row["メーカー"]?.ToString() ?? "",
                                    材質 = row["材質"]?.ToString() ?? "",
                                    型式 = row["型式"]?.ToString() ?? "",
                                    仕様 = "", // 仕様カラムは存在しない
                                    備考 = row["備考"]?.ToString() ?? "",
                                    部品種別 = row["部品種別"] == DBNull.Value ? (short)0 : Convert.ToInt16(row["部品種別"]),
                                    ユニット種別 = row["ユニット種別"] == DBNull.Value ? (short)0 : Convert.ToInt16(row["ユニット種別"]),
                                    オプションユニットFL = row["オプションユニットFL"] == DBNull.Value ? (short)0 : Convert.ToInt16(row["オプションユニットFL"])
                                });
                            }
                        }
                    }

                    // 2. T_個体部品子部品サブとT_部品マスタを結合して親子関係を取得
                    string childQuery = @"
                        SELECT 
                            c.親子ID, c.個体ID, c.機械管理ID, c.親部品コード, c.子部品コード, c.連番, c.個数,
                            c.廃止日, c.廃止時改訂ID, c.登録日, c.登録時改訂ID,
                            pp.品番 AS 親品番, pp.品名 AS 親品名,
                            cp.品番 AS 子品番, cp.品名 AS 子品名, 
                            cp.メーカー AS 子メーカー, cp.材質 AS 子材質, cp.型式 AS 子型式, 
                            cp.備考 AS 子備考,
                            cp.部品種別 AS 子部品種別, cp.ユニット種別 AS 子ユニット種別, cp.オプションユニットFL AS 子オプションユニットFL
                        FROM T_個体部品子部品サブ c
                        LEFT JOIN T_部品マスタ pp ON c.親部品コード = pp.部品ID
                        LEFT JOIN T_部品マスタ cp ON c.子部品コード = cp.部品ID
                        WHERE c.機械管理ID = @machineId AND c.廃止日 IS NULL
                        ORDER BY pp.品番, cp.品番, c.連番";

                    using (SqlCommand childCmd = new SqlCommand(childQuery, connection))
                    {
                        childCmd.Parameters.AddWithValue("@machineId", machineId);

                        using (SqlDataAdapter childAdapter = new SqlDataAdapter(childCmd))
                        {
                            DataTable childTable = new DataTable();
                            childAdapter.Fill(childTable);

                            foreach (DataRow row in childTable.Rows)
                            {
                                response.ChildRelations.Add(new IndividualPartChildDetailModel
                                {
                                    親子ID = row["親子ID"]?.ToString() ?? "",
                                    個体ID = row["個体ID"]?.ToString(),
                                    機械管理ID = row["機械管理ID"]?.ToString() ?? "",
                                    親部品コード = row["親部品コード"]?.ToString() ?? "",
                                    子部品コード = row["子部品コード"]?.ToString() ?? "",
                                    連番 = Convert.ToInt16(row["連番"]),
                                    個数 = Convert.ToInt16(row["個数"]),
                                    廃止日 = row["廃止日"] == DBNull.Value ? null : Convert.ToDateTime(row["廃止日"]),
                                    廃止時改訂ID = row["廃止時改訂ID"]?.ToString(),
                                    登録日 = Convert.ToDateTime(row["登録日"]),
                                    登録時改訂ID = row["登録時改訂ID"]?.ToString(),
                                    親品番 = row["親品番"]?.ToString() ?? "",
                                    親品名 = row["親品名"]?.ToString() ?? "",
                                    子品番 = row["子品番"]?.ToString() ?? "",
                                    子品名 = row["子品名"]?.ToString() ?? "",
                                    子単位 = "", // 単位カラムは存在しない
                                    子メーカー = row["子メーカー"]?.ToString() ?? "",
                                    子材質 = row["子材質"]?.ToString() ?? "",
                                    子型式 = row["子型式"]?.ToString() ?? "",
                                    子仕様 = "", // 仕様カラムは存在しない
                                    子備考 = row["子備考"]?.ToString() ?? "",
                                    子部品種別 = row["子部品種別"] == DBNull.Value ? (short)0 : Convert.ToInt16(row["子部品種別"]),
                                    子ユニット種別 = row["子ユニット種別"] == DBNull.Value ? (short)0 : Convert.ToInt16(row["子ユニット種別"]),
                                    子オプションユニットFL = row["子オプションユニットFL"] == DBNull.Value ? (short)0 : Convert.ToInt16(row["子オプションユニットFL"])
                                });
                            }
                        }
                    }

                    response.TotalPartsCount = response.Parts.Count;
                    response.TotalChildRelationsCount = response.ChildRelations.Count;
                    response.LastUpdated = response.Parts.Count > 0 ? response.Parts.Max(p => p.登録日) : null;
                    response.Message = $"機械ID: {machineId} の個体部品データを取得しました。";

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"個体部品データ取得でエラーが発生しました: {ex.Message}" });
            }
        }
    }
}