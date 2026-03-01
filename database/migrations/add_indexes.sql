-- Indexes for frequent filter/sort columns on recettes
CREATE INDEX IF NOT EXISTS idx_recettes_name ON recettes (name);
CREATE INDEX IF NOT EXISTS idx_recettes_rating ON recettes (rating);
CREATE INDEX IF NOT EXISTS idx_recettes_book_id ON recettes (book_id);
CREATE INDEX IF NOT EXISTS idx_recettes_store_id ON recettes (store_id);
CREATE INDEX IF NOT EXISTS idx_recettes_created_at ON recettes (created_at DESC);

-- Indexes for books_authors join table
CREATE INDEX IF NOT EXISTS idx_books_authors_book_id ON books_authors (book_id);
CREATE INDEX IF NOT EXISTS idx_books_authors_author_id ON books_authors (author_id);

-- French full-text search index on name and notes
CREATE INDEX IF NOT EXISTS idx_recettes_fts ON recettes
  USING gin(to_tsvector('french', name || ' ' || COALESCE(notes, '')));
