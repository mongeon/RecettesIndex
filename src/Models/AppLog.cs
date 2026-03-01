using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace RecettesIndex.Models;

[Table("app_logs")]
public class AppLog : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("level")]
    public string Level { get; set; } = string.Empty;

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("context")]
    public string? Context { get; set; }

    [Column("stack_trace")]
    public string? StackTrace { get; set; }
}
