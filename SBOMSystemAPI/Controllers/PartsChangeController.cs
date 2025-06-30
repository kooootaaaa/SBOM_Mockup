using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SBOMSystemAPI.Models;
using System.Data;

namespace SBOMSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartsChangeController : ControllerBase
    {
        private readonly string _connectionString = "Server=192.168.1.19;Database=WeBOMSQL;User Id=SHINTEC;Password=SHINTEC;TrustServerCertificate=true";

        // GET: api/PartsChange/{machineId}
        [HttpGet("{machineId}")]
        public async Task<ActionResult<IEnumerable<PartsChangeMainModel>>> GetChangeHistory(string machineId)
        {
            var changes = new List<PartsChangeMainModel>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT m.*, 
                           (SELECT COUNT(*) FROM T_個体改訂部品サブ s WHERE s.個体改訂ID = m.個体改訂ID) as 明細件数
                    FROM T_個体改訂履歴メイン m
                    INNER JOIN T_個体改訂部品サブ s ON m.個体改訂ID = s.個体改訂ID
                    WHERE s.個体ID = @MachineId
                    GROUP BY m.個体改訂ID, m.改訂種別, m.改訂No, m.発行日, m.改訂完了日, 
                             m.承認日, m.改訂担当者CODE, m.承認者CODE, m.改訂状態, 
                             m.編集中ユーザーID, m.改訂内容, m.背景, m.備考
                    ORDER BY m.個体改訂ID DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MachineId", machineId);
                    
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            changes.Add(new PartsChangeMainModel
                            {
                                個体改訂ID = reader["個体改訂ID"]?.ToString(),
                                改訂種別 = reader["改訂種別"] as short?,
                                改訂No = reader["改訂No"]?.ToString(),
                                発行日 = reader["発行日"] as DateTime?,
                                改訂完了日 = reader["改訂完了日"] as DateTime?,
                                承認日 = reader["承認日"] as DateTime?,
                                改訂担当者CODE = reader["改訂担当者CODE"]?.ToString(),
                                承認者CODE = reader["承認者CODE"]?.ToString(),
                                改訂状態 = reader["改訂状態"]?.ToString(),
                                編集中ユーザーID = reader["編集中ユーザーID"]?.ToString(),
                                改訂内容 = reader["改訂内容"]?.ToString(),
                                背景 = reader["背景"]?.ToString(),
                                備考 = reader["備考"]?.ToString()
                            });
                        }
                    }
                }
            }
            
            return Ok(changes);
        }

        // GET: api/PartsChange/detail/{revisionId}
        [HttpGet("detail/{revisionId}")]
        public async Task<ActionResult<PartsChangeMainModel>> GetChangeDetail(string revisionId)
        {
            PartsChangeMainModel? changeMain = null;
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Get main record
                var mainQuery = "SELECT * FROM T_個体改訂履歴メイン WHERE 個体改訂ID = @RevisionId";
                using (var command = new SqlCommand(mainQuery, connection))
                {
                    command.Parameters.AddWithValue("@RevisionId", revisionId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            changeMain = new PartsChangeMainModel
                            {
                                個体改訂ID = reader["個体改訂ID"]?.ToString(),
                                改訂種別 = reader["改訂種別"] as short?,
                                改訂No = reader["改訂No"]?.ToString(),
                                発行日 = reader["発行日"] as DateTime?,
                                改訂完了日 = reader["改訂完了日"] as DateTime?,
                                承認日 = reader["承認日"] as DateTime?,
                                改訂担当者CODE = reader["改訂担当者CODE"]?.ToString(),
                                承認者CODE = reader["承認者CODE"]?.ToString(),
                                改訂状態 = reader["改訂状態"]?.ToString(),
                                編集中ユーザーID = reader["編集中ユーザーID"]?.ToString(),
                                改訂内容 = reader["改訂内容"]?.ToString(),
                                背景 = reader["背景"]?.ToString(),
                                備考 = reader["備考"]?.ToString(),
                                部品明細 = new List<PartsChangeSubModel>()
                            };
                        }
                    }
                }
                
                if (changeMain == null)
                    return NotFound();
                
                // Get sub records
                var subQuery = "SELECT * FROM T_個体改訂部品サブ WHERE 個体改訂ID = @RevisionId ORDER BY 改訂連番";
                using (var command = new SqlCommand(subQuery, connection))
                {
                    command.Parameters.AddWithValue("@RevisionId", revisionId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            changeMain.部品明細!.Add(new PartsChangeSubModel
                            {
                                個体改訂部品ID = (int)reader["個体改訂部品ID"],
                                個体改訂ID = reader["個体改訂ID"]?.ToString(),
                                改訂連番 = reader["改訂連番"] as int?,
                                廃止部品ID = reader["廃止部品ID"]?.ToString(),
                                廃止品番 = reader["廃止品番"]?.ToString(),
                                廃止品名 = reader["廃止品名"]?.ToString(),
                                廃止個数 = reader["廃止個数"] as short?,
                                廃止部品ユニットFL = reader["廃止部品ユニットFL"] as short?,
                                新部品ID = reader["新部品ID"]?.ToString(),
                                新品番 = reader["新品番"]?.ToString(),
                                新品名 = reader["新品名"]?.ToString(),
                                新個数 = reader["新個数"] as int?,
                                新部品ユニットFL = reader["新部品ユニットFL"] as short?,
                                互換 = reader["互換"]?.ToString(),
                                部品種別 = reader["部品種別"] as short?,
                                変更事項 = reader["変更事項"]?.ToString(),
                                変更時期 = reader["変更時期"]?.ToString(),
                                完了FL = reader["完了FL"] as short?,
                                個体改訂処理日 = reader["個体改訂処理日"] as DateTime?,
                                新部品FL = reader["新部品FL"] as short?,
                                部品備考 = reader["部品備考"]?.ToString(),
                                個体ID = reader["個体ID"]?.ToString()
                            });
                        }
                    }
                }
            }
            
            return Ok(changeMain);
        }

        // POST: api/PartsChange
        [HttpPost]
        public async Task<ActionResult<PartsChangeMainModel>> CreateChange(PartsChangeRequestModel request)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Get next numbers
                        var numbers = await GetNextRevisionNumbers(connection, transaction);
                        
                        // Insert main record
                        var insertMainQuery = @"
                            INSERT INTO T_個体改訂履歴メイン (
                                個体改訂ID, 改訂種別, 改訂No, 発行日, 改訂担当者CODE, 
                                改訂状態, 改訂内容, 背景, 備考
                            ) VALUES (
                                @個体改訂ID, NULL, @改訂No, @発行日, @改訂担当者CODE, 
                                '編集中', @改訂内容, @背景, @備考
                            )";
                        
                        using (var command = new SqlCommand(insertMainQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@個体改訂ID", numbers.NextRevisionId);
                            command.Parameters.AddWithValue("@改訂No", numbers.NextRevisionNo);
                            command.Parameters.AddWithValue("@発行日", DateTime.Now);
                            command.Parameters.AddWithValue("@改訂担当者CODE", request.改訂担当者CODE ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@改訂内容", request.改訂内容 ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@背景", request.背景 ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@備考", request.備考 ?? (object)DBNull.Value);
                            
                            await command.ExecuteNonQueryAsync();
                        }
                        
                        // Insert sub records
                        if (request.変更明細 != null)
                        {
                            int 連番 = 1;
                            foreach (var item in request.変更明細)
                            {
                                var insertSubQuery = @"
                                    INSERT INTO T_個体改訂部品サブ (
                                        個体改訂部品ID, 個体改訂ID, 改訂連番, 
                                        廃止部品ID, 廃止品番, 廃止品名, 廃止個数, 
                                        新部品ID, 新品番, 新品名, 新個数, 
                                        互換, 変更事項, 変更時期, 部品備考, 個体ID
                                    ) VALUES (
                                        @個体改訂部品ID, @個体改訂ID, @改訂連番, 
                                        @廃止部品ID, @廃止品番, @廃止品名, @廃止個数, 
                                        @新部品ID, @新品番, @新品名, @新個数, 
                                        @互換, @変更事項, @変更時期, @部品備考, @個体ID
                                    )";
                                
                                using (var command = new SqlCommand(insertSubQuery, connection, transaction))
                                {
                                    // Generate sub ID (you might want to use a proper ID generation strategy)
                                    var subId = await GetNextSubId(connection, transaction);
                                    
                                    command.Parameters.AddWithValue("@個体改訂部品ID", subId);
                                    command.Parameters.AddWithValue("@個体改訂ID", numbers.NextRevisionId);
                                    command.Parameters.AddWithValue("@改訂連番", 連番++);
                                    command.Parameters.AddWithValue("@廃止部品ID", item.廃止部品ID ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@廃止品番", item.廃止品番 ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@廃止品名", item.廃止品名 ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@廃止個数", item.廃止個数 ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@新部品ID", item.新部品ID ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@新品番", item.新品番 ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@新品名", item.新品名 ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@新個数", item.新個数 ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@互換", item.互換 ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@変更事項", item.変更事項 ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@変更時期", item.変更時期 ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@部品備考", item.部品備考 ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@個体ID", request.個体ID);
                                    
                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                        }
                        
                        await transaction.CommitAsync();
                        
                        // Return created record
                        return await GetChangeDetail(numbers.NextRevisionId!);
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        // PUT: api/PartsChange/{revisionId}/status
        [HttpPut("{revisionId}/status")]
        public async Task<IActionResult> UpdateStatus(string revisionId, UpdateStatusRequestModel request)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    UPDATE T_個体改訂履歴メイン 
                    SET 改訂状態 = @改訂状態";
                
                if (request.改訂状態 == "承認済み")
                {
                    query += ", 承認日 = @承認日, 承認者CODE = @承認者CODE";
                }
                else if (request.改訂状態 == "完了")
                {
                    query += ", 改訂完了日 = @改訂完了日";
                }
                
                query += " WHERE 個体改訂ID = @RevisionId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@改訂状態", request.改訂状態);
                    command.Parameters.AddWithValue("@RevisionId", revisionId);
                    
                    if (request.改訂状態 == "承認済み")
                    {
                        command.Parameters.AddWithValue("@承認日", DateTime.Now);
                        command.Parameters.AddWithValue("@承認者CODE", request.承認者CODE ?? (object)DBNull.Value);
                    }
                    else if (request.改訂状態 == "完了")
                    {
                        command.Parameters.AddWithValue("@改訂完了日", DateTime.Now);
                    }
                    
                    await connection.OpenAsync();
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    
                    if (rowsAffected == 0)
                        return NotFound();
                }
            }
            
            return NoContent();
        }

        // GET: api/PartsChange/next-numbers
        [HttpGet("next-numbers")]
        public async Task<ActionResult<RevisionNumbersModel>> GetNextNumbers()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var numbers = await GetNextRevisionNumbers(connection, null);
                return Ok(numbers);
            }
        }

        private async Task<RevisionNumbersModel> GetNextRevisionNumbers(SqlConnection connection, SqlTransaction? transaction)
        {
            var result = new RevisionNumbersModel();
            
            // Get next revision ID
            var maxIdQuery = "SELECT MAX(CAST(個体改訂ID AS INT)) FROM T_個体改訂履歴メイン";
            using (var command = new SqlCommand(maxIdQuery, connection, transaction))
            {
                var maxId = await command.ExecuteScalarAsync();
                int nextId = (maxId == DBNull.Value) ? 1 : Convert.ToInt32(maxId) + 1;
                result.NextRevisionId = nextId.ToString("D6"); // Format as 6 digits
            }
            
            // Get next revision No (YY + 3 digits)
            var year = DateTime.Now.ToString("yy");
            var maxNoQuery = @"
                SELECT MAX(CAST(SUBSTRING(改訂No, 3, 3) AS INT)) 
                FROM T_個体改訂履歴メイン 
                WHERE 改訂No LIKE @YearPrefix + '%'";
            
            using (var command = new SqlCommand(maxNoQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@YearPrefix", year);
                var maxNo = await command.ExecuteScalarAsync();
                int nextNo = (maxNo == DBNull.Value) ? 1 : Convert.ToInt32(maxNo) + 1;
                result.NextRevisionNo = year + nextNo.ToString("D3");
            }
            
            return result;
        }

        private async Task<int> GetNextSubId(SqlConnection connection, SqlTransaction transaction)
        {
            var query = "SELECT ISNULL(MAX(個体改訂部品ID), 0) + 1 FROM T_個体改訂部品サブ";
            using (var command = new SqlCommand(query, connection, transaction))
            {
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }
    }
}