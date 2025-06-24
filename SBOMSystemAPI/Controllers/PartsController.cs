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
    }
}