using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;
// Recipe.cs
namespace RecettesIndex.Models
{
    [Table("recettes")]
    public class Recipe : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }
        [Column("name")]
        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; } = string.Empty;
        [Column("rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        [Column("created_at")]
        public DateTime CreationDate { get; set; }
        [Column("notes")]
        public string? Notes { get; set; }
        [Column("book_id")]
        public int? BookId { get; set; }
        [Column("page")]
        public int? BookPage { get; set; }
        [Reference(typeof(Book), joinType: ReferenceAttribute.JoinType.Left, true)]
        public Book? Book { get; set; }
    }

    [Table("books")]
    public class Book : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }
        [Column("title")]
        public string Name { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreationDate { get; set; }

        [Reference(typeof(Author), includeInQuery: true, useInnerJoin: false)]
        public List<Author> Authors { get; set; } = new();
    }

    [Table("authors")]
    public class Author : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("first_name")]
        public string Name { get; set; } = string.Empty;
        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("created_at")]
        public DateTime CreationDate { get; set; }


        [Reference(typeof(Book), useInnerJoin: false, includeInQuery: true)]
        public List<Book> Books { get; set; } = default!;

        public string FullName { get { return $"{Name} {LastName}"; } }
    }

    // Junction table for many-to-many relationship between books and authors
    [Table("books_authors")]
    public class BookAuthor : BaseModel
    {
        [Column("book_id")]
        public int BookId { get; set; }

        [Column("author_id")]
        public int AuthorId { get; set; }

        [Column("created_at")]
        public DateTime CreationDate { get; set; }
    }
}
