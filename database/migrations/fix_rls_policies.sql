-- Fix recettes: UPDATE and DELETE were incorrectly set to 'public' role (allows anon)
DROP POLICY IF EXISTS "Enable update for users based on email" ON recettes;
DROP POLICY IF EXISTS "Enable delete for users based on user_id" ON recettes;

CREATE POLICY "Update recettes" ON recettes
  FOR UPDATE TO authenticated USING (true) WITH CHECK (true);

CREATE POLICY "Delete recettes" ON recettes
  FOR DELETE TO authenticated USING (true);

-- Add missing DELETE policies for authors, books, stores
CREATE POLICY "Delete authors" ON authors
  FOR DELETE TO authenticated USING (true);

CREATE POLICY "Delete books" ON books
  FOR DELETE TO authenticated USING (true);

CREATE POLICY "Delete stores" ON stores
  FOR DELETE TO authenticated USING (true);
