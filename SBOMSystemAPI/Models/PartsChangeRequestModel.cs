using System;
using System.Collections.Generic;

namespace SBOMSystemAPI.Models
{
    public class PartsChangeRequestModel
    {
        public string? 個体ID { get; set; }
        public string? 改訂内容 { get; set; }
        public string? 背景 { get; set; }
        public string? 備考 { get; set; }
        public string? 改訂担当者CODE { get; set; }
        public List<PartsChangeItemModel>? 変更明細 { get; set; }
    }

    public class PartsChangeItemModel
    {
        public string? 変更種別 { get; set; } // "追加", "削除", "変更"
        public string? 廃止部品ID { get; set; }
        public string? 廃止品番 { get; set; }
        public string? 廃止品名 { get; set; }
        public short? 廃止個数 { get; set; }
        public string? 新部品ID { get; set; }
        public string? 新品番 { get; set; }
        public string? 新品名 { get; set; }
        public int? 新個数 { get; set; }
        public string? 互換 { get; set; }
        public string? 変更事項 { get; set; }
        public string? 変更時期 { get; set; }
        public string? 部品備考 { get; set; }
    }

    public class RevisionNumbersModel
    {
        public string? NextRevisionId { get; set; }
        public string? NextRevisionNo { get; set; }
    }

    public class UpdateStatusRequestModel
    {
        public string? 改訂状態 { get; set; }
        public string? 承認者CODE { get; set; }
    }
}