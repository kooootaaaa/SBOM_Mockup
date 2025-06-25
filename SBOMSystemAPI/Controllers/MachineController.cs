using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

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
                            
                            return Ok(new { exists = true, columns = columns });
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
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMachineDetail(string id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        SELECT 
                            製造番号,
                            ItemSubCode,
                            型式,
                            機種名,
                            ロット番号,
                            ロット連番,
                            号機番号,
                            案件番号,
                            中古番号,
                            取付タンク番号,
                            OrderSerialNo,
                            [預けNo.],
                            河北製造番号,
                            過去製造番号,
                            備考1,
                            マスタ外登録CH,
                            傷有FLG,
                            非在庫品FLG,
                            製造種別,
                            現在地,
                            機械状態,
                            管理区分,
                            完成予定日,
                            仕掛品完成日,
                            完成日,
                            CR登録日,
                            メーカーオプション,
                            客先仕様,
                            仕様変更履歴,
                            改造日,
                            製造時備考,
                            改造内容
                        FROM T_機械管理台帳 
                        WHERE 製造番号 = @MachineId";
                    
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
                                return NotFound(new { error = $"製造番号 {id} の機械が見つかりません。" });
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
    }
}