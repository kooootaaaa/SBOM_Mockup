namespace SBOMSystemAPI.Models
{
    public class PartsModel
    {
        public string? PartsNumber { get; set; }
        public string? PartsName { get; set; }
        public string? PartsType { get; set; }
        public string? Unit { get; set; }
        public decimal? StandardPrice { get; set; }
        public string? Supplier { get; set; }
        public string? Remarks { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}