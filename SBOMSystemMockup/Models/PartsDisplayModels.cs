using System.ComponentModel.DataAnnotations;

namespace SBOMSystemMockup.Models
{
    /// <summary>
    /// 部品ツリー表示用のノードモデル
    /// </summary>
    public class PartNode
    {
        public string PartId { get; set; } = "";
        public string PartNumber { get; set; } = "";
        public string PartName { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public string Unit { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Material { get; set; } = "";
        public string Model { get; set; } = "";
        public string Specification { get; set; } = "";
        public string Remarks { get; set; } = "";
        
        // ツリー表示用プロパティ
        public List<PartNode> Children { get; set; } = new List<PartNode>();
        public bool IsExpanded { get; set; } = false;
        public bool IsLoading { get; set; } = false;
        public bool HasChildren { get; set; } = false;
        public bool ChildrenLoaded { get; set; } = false;
        public int Level { get; set; } = 0;
        public PartNode? Parent { get; set; } = null;
        
        // 部品種別
        public bool IsUnit { get; set; } = false;
        public bool IsOptionUnit { get; set; } = false;
    }

    /// <summary>
    /// 部品集計表示用のモデル
    /// </summary>
    public class PartSummary
    {
        public string PartId { get; set; } = "";
        public string PartNumber { get; set; } = "";
        public string PartName { get; set; } = "";
        public int TotalQuantity { get; set; } = 0;
        public string Unit { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Material { get; set; } = "";
        public string Model { get; set; } = "";
        public string Specification { get; set; } = "";
        public string Remarks { get; set; } = "";
        
        // 使用経路情報
        public List<string> UsagePaths { get; set; } = new List<string>();
        public int UniqueUsageCount { get; set; } = 0; // 異なる使用箇所の数
    }

    /// <summary>
    /// 表示モード列挙型
    /// </summary>
    public enum PartsDisplayMode
    {
        Hierarchy,  // 階層表示
        Summary     // 集計表示
    }

    /// <summary>
    /// ソートモード列挙型
    /// </summary>
    public enum PartsSortMode
    {
        PartNumber,     // 部品番号順
        PartName,       // 部品名順
        QuantityAsc,    // 数量昇順
        QuantityDesc,   // 数量降順
        Manufacturer    // メーカー順
    }

    /// <summary>
    /// 部品データ取得リクエストモデル
    /// </summary>
    public class PartsDataRequest
    {
        public string MachineId { get; set; } = "";
        public PartsDisplayMode DisplayMode { get; set; } = PartsDisplayMode.Hierarchy;
        public PartsSortMode SortMode { get; set; } = PartsSortMode.PartNumber;
        public string? SearchText { get; set; }
        public string? ManufacturerFilter { get; set; }
    }

    /// <summary>
    /// 部品データ応答モデル
    /// </summary>
    public class PartsDataResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "";
        public List<PartNode> HierarchyParts { get; set; } = new List<PartNode>();
        public List<PartSummary> SummaryParts { get; set; } = new List<PartSummary>();
        public int TotalPartsCount { get; set; } = 0;
        public int TotalChildRelationsCount { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 部品階層展開用の補助クラス
    /// </summary>
    public static class PartsTreeHelper
    {
        /// <summary>
        /// フラットな部品リストから階層構造を構築
        /// </summary>
        public static List<PartNode> BuildHierarchy(List<PartNode> flatParts)
        {
            var lookup = flatParts.ToLookup(p => p.Parent?.PartId ?? "");
            var rootParts = lookup[""].ToList();
            
            foreach (var part in flatParts)
            {
                part.Children = lookup[part.PartId].ToList();
                part.HasChildren = part.Children.Count > 0;
                
                // 子部品の親参照を設定
                foreach (var child in part.Children)
                {
                    child.Parent = part;
                    child.Level = part.Level + 1;
                }
            }
            
            return rootParts;
        }

        /// <summary>
        /// 階層構造から集計データを生成
        /// </summary>
        public static List<PartSummary> GenerateSummary(List<PartNode> hierarchyParts)
        {
            var summaryDict = new Dictionary<string, PartSummary>();
            
            void ProcessNode(PartNode node, string path = "")
            {
                var currentPath = string.IsNullOrEmpty(path) ? node.PartNumber : $"{path} > {node.PartNumber}";
                
                // 子部品がない場合のみ集計対象
                if (!node.HasChildren)
                {
                    if (summaryDict.TryGetValue(node.PartId, out var existing))
                    {
                        existing.TotalQuantity += node.Quantity;
                        existing.UsagePaths.Add(currentPath);
                        existing.UniqueUsageCount = existing.UsagePaths.Distinct().Count();
                    }
                    else
                    {
                        summaryDict[node.PartId] = new PartSummary
                        {
                            PartId = node.PartId,
                            PartNumber = node.PartNumber,
                            PartName = node.PartName,
                            TotalQuantity = node.Quantity,
                            Unit = node.Unit,
                            Manufacturer = node.Manufacturer,
                            Material = node.Material,
                            Model = node.Model,
                            Specification = node.Specification,
                            Remarks = node.Remarks,
                            UsagePaths = new List<string> { currentPath },
                            UniqueUsageCount = 1
                        };
                    }
                }

                // 子部品を再帰処理
                foreach (var child in node.Children)
                {
                    ProcessNode(child, currentPath);
                }
            }

            foreach (var rootPart in hierarchyParts)
            {
                ProcessNode(rootPart);
            }

            return summaryDict.Values.ToList();
        }

        /// <summary>
        /// 集計データをソート
        /// </summary>
        public static List<PartSummary> SortSummary(List<PartSummary> summaryParts, PartsSortMode sortMode)
        {
            return sortMode switch
            {
                PartsSortMode.PartNumber => summaryParts.OrderBy(p => p.PartNumber).ToList(),
                PartsSortMode.PartName => summaryParts.OrderBy(p => p.PartName).ToList(),
                PartsSortMode.QuantityAsc => summaryParts.OrderBy(p => p.TotalQuantity).ToList(),
                PartsSortMode.QuantityDesc => summaryParts.OrderByDescending(p => p.TotalQuantity).ToList(),
                PartsSortMode.Manufacturer => summaryParts.OrderBy(p => p.Manufacturer).ToList(),
                _ => summaryParts
            };
        }
    }
}