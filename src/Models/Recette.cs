using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace RecettesIndex.Models;

/// <summary>
/// Represents a recipe in the application.
/// </summary>
[Table("recettes")]
public class Recipe : BaseModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the recipe.
    /// </summary>
    [PrimaryKey("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the recipe.
    /// </summary>
    [Column("name")]
    [Required(ErrorMessage = "The Name field is required.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rating of the recipe (1-5 stars).
    /// </summary>
    [Column("rating")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the recipe.
    /// </summary>
    [Column("created_at")]
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Gets or sets optional notes or modifications for the recipe.
    /// </summary>
    [Column("notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the ID of the book this recipe is from, if applicable.
    /// </summary>
    [Column("book_id")]
    public int? BookId { get; set; }

    /// <summary>
    /// Gets or sets the page number where the recipe can be found in the book.
    /// </summary>
    [Column("page")]
    public int? BookPage { get; set; }

    /// <summary>
    /// Gets or sets the book this recipe is associated with.
    /// </summary>
    [Reference(typeof(Book), joinType: ReferenceAttribute.JoinType.Left, true)]
    public Book? Book { get; set; }
}

/// <summary>
/// Represents a cookbook in the application.
/// </summary>
[Table("books")]
public class Book : BaseModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the book.
    /// </summary>
    [PrimaryKey("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the book.
    /// </summary>
    [Column("title")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date of the book record.
    /// </summary>
    [Column("created_at")]
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the collection of authors who wrote this book.
    /// </summary>
    [Reference(typeof(Author), includeInQuery: true, useInnerJoin: false)]
    public List<Author> Authors { get; set; } = new();
}

/// <summary>
/// Represents an author of one or more cookbooks.
/// </summary>
[Table("authors")]
public class Author : BaseModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the author.
    /// </summary>
    [PrimaryKey("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the first name of the author.
    /// </summary>
    [Column("first_name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the author.
    /// </summary>
    [Column("last_name")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the author record.
    /// </summary>
    [Column("created_at")]
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the collection of books written by this author.
    /// </summary>
    [Reference(typeof(Book), useInnerJoin: false, includeInQuery: true)]
    public List<Book> Books { get; set; } = default!;

    /// <summary>
    /// Gets the full name of the author (first name and last name combined).
    /// </summary>
    public string FullName { get { return $"{Name} {LastName}"; } }
}

/// <summary>
/// Junction table for managing the many-to-many relationship between books and authors.
/// </summary>
[Table("books_authors")]
public class BookAuthor : BaseModel
{
    /// <summary>
    /// Gets or sets the ID of the book in the relationship.
    /// </summary>
    [Column("book_id")]
    public int BookId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the author in the relationship.
    /// </summary>
    [Column("author_id")]
    public int AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the creation date of this association.
    /// </summary>
    [Column("created_at")]
    public DateTime CreationDate { get; set; }
}
