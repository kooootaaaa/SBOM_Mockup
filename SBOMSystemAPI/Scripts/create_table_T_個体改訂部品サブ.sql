-- T_個体改訂部品サブ テーブル作成スクリプト
CREATE TABLE T_個体改訂部品サブ (
    個体改訂部品ID INT NOT NULL,
    個体改訂ID NVARCHAR(7),
    改訂連番 INT,
    廃止部品ID NVARCHAR(8),
    廃止品番 NVARCHAR(32),
    廃止品名 NVARCHAR(64),
    廃止個数 SMALLINT,
    廃止部品ユニットFL SMALLINT,
    新部品ID NVARCHAR(8),
    新品番 NVARCHAR(32),
    新品名 NVARCHAR(64),
    新個数 INT,
    新部品ユニットFL SMALLINT,
    互換 NVARCHAR(5),
    部品種別 SMALLINT,
    変更事項 NVARCHAR(128),
    変更時期 NVARCHAR(128),
    完了FL SMALLINT,
    個体改訂処理日 DATETIME,
    新部品FL SMALLINT,
    部品備考 NVARCHAR(64),
    個体ID NVARCHAR(50)  -- データ型を推測（括弧付きのため追加フィールドとして扱い）
);

-- 主キー制約の追加（個体改訂部品IDを主キーと仮定）
ALTER TABLE T_個体改訂部品サブ
ADD CONSTRAINT PK_T_個体改訂部品サブ PRIMARY KEY (個体改訂部品ID);

-- インデックスの追加（よく検索されそうなフィールド）
CREATE INDEX IX_T_個体改訂部品サブ_個体改訂ID ON T_個体改訂部品サブ (個体改訂ID);
CREATE INDEX IX_T_個体改訂部品サブ_廃止部品ID ON T_個体改訂部品サブ (廃止部品ID);
CREATE INDEX IX_T_個体改訂部品サブ_新部品ID ON T_個体改訂部品サブ (新部品ID);

-- テーブルコメントの追加
EXEC sp_addextendedproperty 
    @name = N'MS_Description',
    @value = N'個体改訂部品のサブ情報を管理するテーブル',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'T_個体改訂部品サブ';

-- フィールドコメントの追加例
EXEC sp_addextendedproperty 
    @name = N'MS_Description', @value = N'個体改訂部品の一意識別子',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'T_個体改訂部品サブ',
    @level2type = N'COLUMN', @level2name = N'個体改訂部品ID';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', @value = N'個体改訂の識別ID',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'T_個体改訂部品サブ',
    @level2type = N'COLUMN', @level2name = N'個体改訂ID';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', @value = N'改訂の連続番号',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'T_個体改訂部品サブ',
    @level2type = N'COLUMN', @level2name = N'改訂連番';