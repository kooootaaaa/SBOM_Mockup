using System.ComponentModel.DataAnnotations;

namespace SBOMSystemMockup.Models
{
    /// <summary>
    /// 個体部品登録リクエストモデル
    /// </summary>
    public class IndividualPartsRegisterRequest
    {
        public string MachineId { get; set; } = "";
        public List<IndividualPartItem> DirectParts { get; set; } = new List<IndividualPartItem>();
        public List<IndividualPartChildItem> ChildRelations { get; set; } = new List<IndividualPartChildItem>();
    }

    /// <summary>
    /// 個体部品情報（T_個体部品サブ用）
    /// </summary>
    public class IndividualPartItem
    {
        public string PartId { get; set; } = "";
        public int Quantity { get; set; }
        public int SequenceNo { get; set; }
    }

    /// <summary>
    /// 個体部品親子関係情報（T_個体部品子部品サブ用）
    /// </summary>
    public class IndividualPartChildItem
    {
        public string ParentPartCode { get; set; } = "";
        public string ChildPartCode { get; set; } = "";
        public int Quantity { get; set; }
        public int SequenceNo { get; set; }
    }

    /// <summary>
    /// 個体部品登録レスポンスモデル
    /// </summary>
    public class IndividualPartsRegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int RegisteredPartsCount { get; set; }
        public int RegisteredChildRelationsCount { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}