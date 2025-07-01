using System.ComponentModel.DataAnnotations;

namespace SBOMSystemAPI.Models
{
    /// <summary>
    /// 個体部品登録リクエストモデル
    /// </summary>
    public class IndividualPartsRegisterRequest
    {
        [Required]
        public string MachineId { get; set; } = "";

        /// <summary>
        /// 機械に直接取り付けられている最上位部品のみ
        /// </summary>
        public List<IndividualPartItem> DirectParts { get; set; } = new List<IndividualPartItem>();

        /// <summary>
        /// この個体固有の全ての直接的な親子関係
        /// </summary>
        public List<IndividualPartChildItem> ChildRelations { get; set; } = new List<IndividualPartChildItem>();
    }

    /// <summary>
    /// 個体部品情報（T_個体部品サブ用）
    /// </summary>
    public class IndividualPartItem
    {
        [Required]
        public string PartId { get; set; } = "";

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "数量は1以上である必要があります")]
        public int Quantity { get; set; }

        [Required]
        public int SequenceNo { get; set; }
    }

    /// <summary>
    /// 個体部品親子関係情報（T_個体部品子部品サブ用）
    /// </summary>
    public class IndividualPartChildItem
    {
        [Required]
        public string ParentPartCode { get; set; } = "";

        [Required]
        public string ChildPartCode { get; set; } = "";

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "数量は1以上である必要があります")]
        public int Quantity { get; set; }

        [Required]
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

    /// <summary>
    /// T_個体部品サブテーブルモデル
    /// </summary>
    public class IndividualPartModel
    {
        public string 個体部品ID { get; set; } = "";
        public string? 個体ID { get; set; }
        public string 機械管理ID { get; set; } = "";
        public string 部品ID { get; set; } = "";
        public short 個体IDごと連番 { get; set; }
        public short 個数 { get; set; }
        public DateTime? 廃止日 { get; set; }
        public string? 廃止時改訂ID { get; set; }
        public DateTime 登録日 { get; set; }
        public string? 登録時改訂ID { get; set; }
    }

    /// <summary>
    /// T_個体部品子部品サブテーブルモデル
    /// </summary>
    public class IndividualPartChildModel
    {
        public string 親子ID { get; set; } = "";
        public string? 個体ID { get; set; }
        public string 機械管理ID { get; set; } = "";
        public string 親部品コード { get; set; } = "";
        public string 子部品コード { get; set; } = "";
        public short 連番 { get; set; }
        public short 個数 { get; set; }
        public DateTime? 廃止日 { get; set; }
        public string? 廃止時改訂ID { get; set; }
        public DateTime 登録日 { get; set; }
        public string? 登録時改訂ID { get; set; }
    }
}