-- =====================================================
-- Migration: Remove recipe_type column
-- Date: 2024-01-26
-- Description: Remove recipe_type column as it's redundant
--              with the source (book_id/store_id) information
-- =====================================================

-- Remove the recipe_type column from recettes table
ALTER TABLE recettes 
DROP COLUMN IF EXISTS recipe_type;

-- Verification query (optional - run after migration)
-- This should NOT show recipe_type column
-- SELECT column_name, data_type 
-- FROM information_schema.columns 
-- WHERE table_name = 'recettes' 
-- ORDER BY ordinal_position;

-- =====================================================
-- Rollback script (if needed)
-- =====================================================
-- To rollback this migration, uncomment and run:
-- 
-- ALTER TABLE recettes 
-- ADD COLUMN recipe_type VARCHAR(50);
-- 
-- -- Optionally set default values based on source
-- UPDATE recettes 
-- SET recipe_type = CASE 
--     WHEN book_id IS NOT NULL THEN 'book'
--     WHEN store_id IS NOT NULL THEN 'store'
--     ELSE 'homemade'
-- END;
