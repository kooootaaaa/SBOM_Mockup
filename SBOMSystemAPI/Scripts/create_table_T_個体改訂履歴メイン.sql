-- =============================================================================
-- T_個体改訂履歴メイン テーブル作成スクリプト
-- =============================================================================
CREATE TABLE T_個体改訂履歴メイン (
    個体改訂ID NVARCHAR(7) NOT NULL,
    改訂種別 SMALLINT,
    改訂No NVARCHAR(6),
    発行日 DATETIME,
    改訂完了日 DATETIME,
    承認日 DATETIME,
    改訂担当者CODE NVARCHAR(4),
    承認者CODE NVARCHAR(4),
    改訂状態 NVARCHAR(6),
    編集中ユーザーID NVARCHAR(4),
    改訂内容 NVARCHAR(64),
    背景 NVARCHAR(64),
    備考 NVARCHAR(64)
);

-- 主キー制約の追加
ALTER TABLE T_個体改訂履歴メイン
ADD CONSTRAINT PK_T_個体改訂履歴メイン PRIMARY KEY (個体改訂ID);

-- インデックスの追加
CREATE INDEX IX_T_個体改訂履歴メイン_改訂No ON T_個体改訂履歴メイン (改訂No);
CREATE INDEX IX_T_個体改訂履歴メイン_改訂状態 ON T_個体改訂履歴メイン (改訂状態);
CREATE INDEX IX_T_個体改訂履歴メイン_改訂担当者CODE ON T_個体改訂履歴メイン (改訂担当者CODE);
CREATE INDEX IX_T_個体改訂履歴メイン_発行日 ON T_個体改訂履歴メイン (発行日);

-- テーブルコメントの追加
EXEC sp_addextendedproperty 
    @name = N'MS_Description',
    @value = N'個体改訂履歴のメイン情報を管理するテーブル',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'T_個体改訂履歴メイン';

-- フィールドコメントの追加
EXEC sp_addextendedproperty 
    @name = N'MS_Description', @value = N'個体改訂の一意識別子（主キー）',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'T_個体改訂履歴メイン',
    @level2type = N'COLUMN', @level2name = N'個体改訂ID';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', @value = N'改訂の種別コード',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'T_個体改訂履歴メイン',
    @level2type = N'COLUMN', @level2name = N'改訂種別';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', @value = N'改訂番号',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'T_個体改訂履歴メイン',
    @level2type = N'COLUMN', @level2name = N'改訂No';

-- =============================================================================
-- T_個体改訂部品サブ テーブル作成スクリプト
-- =============================================================================
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
    個体改訂処理