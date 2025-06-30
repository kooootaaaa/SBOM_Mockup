using System;

namespace SBOMSystemAPI.Models
{
    public class PartsChangeSubModel
    {
        public int 個体改訂部品ID { get; set; }
        public string? 個体改訂ID { get; set; }
        public int? 改訂連番 { get; set; }
        public string? 廃止部品ID { get; set; }
        public string? 廃止品番 { get; set; }
        public string? 廃止品名 { get; set; }
        public short? 廃止個数 { get; set; }
        public short? 廃止部品ユニットFL { get; set; }
        public string? 新部品ID { get; set; }
        public string? 新品番 { get; set; }
        public string? 新品名 { get; set; }
        public int? 新個数 { get; set; }
        public short? 新部品ユニットFL { get; set; }
        public string? 互換 { get; set; }
        public short? 部品種別 { get; set; }
        public string? 変更事項 { get; set; }
        public string? 変更時期 { get; set; }
        public short? 完了FL { get; set; }
        public DateTime? 個体改訂処理日 { get; set; }
        public short? 新部品FL { get; set; }
        public string? 部品備考 { get; set; }
        public string? 個体ID { get; set; }
    }
}