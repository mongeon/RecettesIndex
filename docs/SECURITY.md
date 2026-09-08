# Security Model

This app is a Blazor WebAssembly SPA talking directly to Supabase. There is no
server of our own in between. That single fact drives every decision below, and
it is the part most Blazor + Supabase examples skip.

## The anon key is public. On purpose.

The Supabase URL and anon key live in `wwwroot/appsettings.json`, which is
served as a static file and shipped inside the WASM bundle. Anyone can open
devtools, read them, and issue their own PostgREST or GraphQL calls against this
database from curl. There is no way around this in a client-only app — no
environment variable, no build-time secret, no obfuscation. If a value must stay
secret, it cannot live in this project.

So the anon key is not a credential. It identifies the project and maps the
caller to the Postgres `anon` role. **The only thing standing between a stranger
with our anon key and our data is Row Level Security.** RLS is not a
nice-to-have here; it is the entire authorization layer.

The service role key is never used by this application and must never appear in
this repository.

## Access model

Read is public. Write requires an authenticated session.

The recipe collection is meant to be browsable by anyone who visits the site —
no login wall. Editing is restricted to the site owner.

RLS is enabled on all eight tables in `public`. Every content table carries four
policies:

| Command | Granted to | Expression |
|---|---|---|
| `SELECT` | `public` (i.e. every role, including `anon`) | `true` |
| `INSERT` | `authenticated` | `with check (true)` |
| `UPDATE` | `authenticated` | `using (true)` |
| `DELETE` | `authenticated` | `using (true)` |

Applied to: `recettes`, `authors`, `books`, `books_authors`, `stores`,
`etiquettes`, `recettes_etiquettes`. The eighth table, `app_logs`, is
authenticated-only for both `SELECT` and `INSERT`, and has no `DELETE` policy.

```sql
alter table public.recettes enable row level security;

create policy "Enable read access for all users" on public.recettes
  for select to public using (true);

create policy "Enable insert for authenticated users only" on public.recettes
  for insert to authenticated with check (true);

create policy "Update recettes" on public.recettes
  for update to authenticated using (true) with check (true);

create policy "Delete recettes" on public.recettes
  for delete to authenticated using (true);
```

Note what is *absent*: there is no `auth.uid() = user_id` anywhere, because
there is no `user_id` column and no per-user ownership. This is a single-owner
catalogue, not a multi-tenant app. Copying multi-tenant policies into a schema
that has no owner column produces policies that either fail or silently allow
everything.

Two details worth knowing if you read the policies directly:

- `to public` in Postgres means *all roles*, which includes `anon` and
  `authenticated`. It is the Supabase dashboard's default template for a public
  read policy. It is equivalent to `to anon, authenticated` here, but it is a
  broader grant than it looks, so it is worth writing deliberately rather than
  accepting from a template.
- On `books`, `books_authors` and `stores` the `UPDATE` policies omit
  `with check`. Postgres falls back to the `using` expression as the check in
  that case, and since `using` is `true`, the behaviour matches the other
  tables. It is an inconsistency, not a hole — but making it explicit removes
  the need for the reader to know that fallback rule.

## The trap: `authenticated` does not mean "me"

`to authenticated` means *any user with a valid JWT from this project's auth
service*. Supabase enables email authentication by default, and new sign-ups
are allowed unless the toggle is switched off. Since every write policy here is
`using (true)`, a stranger who can register an account gets full write access to
the entire catalogue.

Two ways to close it, pick one:

**Option A — disable sign-ups** (simplest for a single-owner app).
Dashboard → Authentication → disable new user sign-ups, for the email provider
*and* for every OAuth provider that is enabled. Existing users keep signing in.

**Option B — scope writes to an explicit allow-list**, if sign-ups must stay
open for another reason:

```sql
create table public.editors (user_id uuid primary key references auth.users(id));
alter table public.editors enable row level security;

drop policy "Enable insert for authenticated users only" on public.recettes;
create policy "editors insert" on public.recettes
  for insert to authenticated
  with check (exists (select 1 from public.editors e where e.user_id = auth.uid()));
```

## Verifying it

Do not trust the policies as written; test them from the outside, the way an
attacker would.

```bash
SUPABASE_URL="https://<ref>.supabase.co"
ANON="<anon key from the published bundle>"

# Should succeed: public read
curl -s "$SUPABASE_URL/rest/v1/recettes?select=id,name&limit=1" \
  -H "apikey: $ANON"

# Should be rejected: anonymous write
curl -s -X POST "$SUPABASE_URL/rest/v1/recettes" \
  -H "apikey: $ANON" -H "Content-Type: application/json" \
  -d '{"name":"rls check"}'
```

The second call must return a row-level-security violation, not a new row.

Inside the database, confirm no table was missed after any schema change:

```sql
select c.relname, c.relrowsecurity as rls_enabled,
       (select count(*) from pg_policies p
        where p.schemaname = 'public' and p.tablename = c.relname) as policies
from pg_class c
join pg_namespace n on n.oid = c.relnamespace
where n.nspname = 'public' and c.relkind = 'r'
order by 1;
```

Any row with `rls_enabled = false` is an open door. Supabase's own security
advisors catch these along with auth misconfigurations; run them after every
migration.

## Known gaps

- **`app_logs` is discoverable but not readable.** The `anon` role still holds a
  `SELECT` grant on it, so the table appears in the auto-generated GraphQL
  schema even though RLS returns no rows. `revoke select on public.app_logs
  from anon, authenticated;` removes it from the exposed schema.
- **`app_logs` cannot be written by anonymous visitors.** Its `INSERT` policy is
  `authenticated`-only, so client-side logging from the public read path is
  silently dropped. Decide whether that is intended; if not, the policy needs to
  admit `anon`, with a rate-limit or column constraint to keep it from becoming
  an open write endpoint.
- **`app_logs` has no `DELETE` policy and no retention**, so it can only grow.
  Add a scheduled cleanup before enabling logging in earnest.
- **Leaked-password protection is off.** Supabase can check new passwords
  against HaveIBeenPwned; it is a dashboard toggle.
- **The email OTP expiry exceeds one hour**, above Supabase's recommended
  threshold.
- **The Postgres instance has outstanding security patches available.**

## What this does not protect against

- **Rate limiting and scraping.** Public read means the whole catalogue can be
  pulled by anyone. That is acceptable here; it would not be for private data.
- **Cost.** A public anon key is an open endpoint. Free-tier quotas are the only
  backstop.
- **Anything requiring a secret.** Third-party API keys, webhooks, or admin
  operations belong in an Edge Function, not in this client.
