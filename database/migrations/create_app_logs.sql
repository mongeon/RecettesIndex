CREATE TABLE IF NOT EXISTS app_logs (
  id BIGSERIAL PRIMARY KEY,
  created_at TIMESTAMPTZ DEFAULT NOW() NOT NULL,
  level TEXT NOT NULL,
  message TEXT NOT NULL,
  context TEXT,
  stack_trace TEXT
);

ALTER TABLE app_logs ENABLE ROW LEVEL SECURITY;

-- Only authenticated users can insert logs; no read via API (use Supabase dashboard)
CREATE POLICY "Insert app_logs" ON app_logs
  FOR INSERT TO authenticated WITH CHECK (true);

CREATE POLICY "Select app_logs" ON app_logs
  FOR SELECT TO authenticated USING (true);
