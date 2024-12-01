namespace RecettesIndex.Shared;

public record Author
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    public required string LastName { get; set; }

    public string FullName { get{ return  $"{FirstName} {LastName}";}}

    public DateTime CreatedAt { get; set; }
}