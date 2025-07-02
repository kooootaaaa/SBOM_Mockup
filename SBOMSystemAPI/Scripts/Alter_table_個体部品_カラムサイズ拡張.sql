-- T_個体部品サブおよびT_個体部品子部品サブテーブルのカラムサイズ拡張スクリプト
-- 作成日: 2025-07-02
-- 説明: 主キーカラムのサイズを拡張して、長い機械管理IDに対応

-- ===================================================
-- T_個体部品サブテーブルの変更
-- ===================================================

-- 1. まず既存のインデックスと制約を削除
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'PK_T_個体部品サブ' AND object_id = OBJECT_ID('T_個体部品サブ'))
    ALTER TABLE T_個体部品サブ DROP CONSTRAINT PK_T_個体部品サブ;

-- 2. カラムサイズを変更
ALTER TABLE T_個体部品サブ
    ALTER COLUMN 個体部品ID nvarchar(50) NOT NULL;

-- 3. 主キー制約を再作成
ALTER TABLE T_個体部品サブ
    ADD CONSTRAINT PK_T_個体部品サブ PRIMARY KEY (個体部品ID);

PRINT 'T_個体部品サブテーブルの個体部品IDカラムを50文字に拡張しました。';

-- ===================================================
-- T_個体部品子部品サブテーブルの変更
-- ===================================================

-- 1. まず既存のインデックスと制約を削除
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'PK_T_個体部品子部品サブ' AND object_id = OBJECT_ID('T_個体部品子部品サブ'))
    ALTER TABLE T_個体部品子部品サブ DROP CONSTRAINT PK_T_個体部品子部品サブ;

-- 2. カラムサイズを変更
ALTER TABLE T_個体部品子部品サブ
    ALTER COLUMN 親子ID nvarchar(50) NOT NULL;

-- 3. 主キー制約を再作成
ALTER TABLE T_個体部品子部品サブ
    ADD CONSTRAINT PK_T_個体部品子部品サブ PRIMARY KEY (親子ID);

PRINT 'T_個体部品子部品サブテーブルの親子IDカラムを50文字に拡張しました。';

-- ===================================================
-- 変更確認
-- ===================================================

-- T_個体部品サブテーブルの確認
SELECT 
    c.TABLE_NAME,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = 'T_個体部品サブ'
    AND c.COLUMN_NAME = '個体部品ID';

-- T_個体部品子部品サブテーブルの確認
SELECT 
    c.TABLE_NAME,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = 'T_個体部品子部品サブ'
    AND c.COLUMN_NAME = '親子ID';

PRINT '===================================================';
PRINT 'カラムサイズ拡張が完了しました。';
PRINT '===================================================';