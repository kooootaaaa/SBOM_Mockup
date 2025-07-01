-- T_個体部品子部品サブテーブル作成スクリプト
-- 作成日: 2025-07-01
-- 説明: 機械個体固有のセット品構成（親子関係）を管理するテーブル

-- 既存テーブルがある場合は削除（開発環境のみで使用）
-- DROP TABLE IF EXISTS T_個体部品子部品サブ;

-- T_個体部品子部品サブテーブル作成
CREATE TABLE T_個体部品子部品サブ (
    親子ID nvarchar(26) NOT NULL,      -- 機械管理ID-親部品コード-子部品コード-連番 例: 1510006-P001-P010-01
    個体ID nvarchar(20) NULL,           -- 将来用、現状NULL
    機械管理ID nvarchar(20) NOT NULL,
    親部品コード nvarchar(8) NOT NULL,
    子部品コード nvarchar(8) NOT NULL,
    連番 smallint NOT NULL,
    個数 smallint NOT NULL,
    廃止日 datetime NULL,
    廃止時改訂ID nvarchar(7) NULL,
    登録日 datetime NOT NULL,
    登録時改訂ID nvarchar(7) NULL,     -- NULL許可
    
    -- 主キー設定
    CONSTRAINT PK_T_個体部品子部品サブ PRIMARY KEY (親子ID),
    
    -- 外部キー設定（必要に応じて）
    -- CONSTRAINT FK_T_個体部品子部品サブ_機械管理ID FOREIGN KEY (機械管理ID) REFERENCES T_機械管理台帳(製造番号),
    -- CONSTRAINT FK_T_個体部品子部品サブ_親部品 FOREIGN KEY (親部品コード) REFERENCES T_部品マスタ(部品ID),
    -- CONSTRAINT FK_T_個体部品子部品サブ_子部品 FOREIGN KEY (子部品コード) REFERENCES T_部品マスタ(部品ID)
);

-- インデックス作成
CREATE INDEX IX_T_個体部品子部品サブ_個体ID ON T_個体部品子部品サブ(個体ID);
CREATE INDEX IX_T_個体部品子部品サブ_機械管理ID ON T_個体部品子部品サブ(機械管理ID);
CREATE INDEX IX_T_個体部品子部品サブ_親部品コード ON T_個体部品子部品サブ(親部品コード);
CREATE INDEX IX_T_個体部品子部品サブ_子部品コード ON T_個体部品子部品サブ(子部品コード);
CREATE INDEX IX_T_個体部品子部品サブ_登録日 ON T_個体部品子部品サブ(登録日);
CREATE INDEX IX_T_個体部品子部品サブ_廃止日 ON T_個体部品子部品サブ(廃止日);

-- 複合インデックス（機械管理ID、親部品コード、子部品コード、連番でユニーク）
CREATE UNIQUE INDEX UX_T_個体部品子部品サブ_複合キー 
    ON T_個体部品子部品サブ(機械管理ID, 親部品コード, 子部品コード, 連番);

-- テーブル作成確認
SELECT 
    '個体部品子部品サブテーブルが正常に作成されました。' as Message,
    COUNT(*) as TableCount
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'T_個体部品子部品サブ';