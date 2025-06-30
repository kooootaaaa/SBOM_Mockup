using System;

namespace SBOMSystemAPI.Models
{
    public class PartsChangeMainModel
    {
        public string? 個体改訂ID { get; set; }
        public short? 改訂種別 { get; set; }
        public string? 改訂No { get; set; }
        public DateTime? 発行日 { get; set; }
        public DateTime? 改訂完了日 { get; set; }
        public DateTime? 承認日 { get; set; }
        public string? 改訂担当者CODE { get; set; }
        public string? 承認者CODE { get; set; }
        public string? 改訂状態 { get; set; }
        public string? 編集中ユーザーID { get; set; }
        public string? 改訂内容 { get; set; }
        public string? 背景 { get; set; }
        public string? 備考 { get; set; }
        
        // Navigation property
        public List<PartsChangeSubModel>? 部品明細 { get; set; }
    }
}