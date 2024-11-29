namespace RecettesIndex.Api.Data.Converter
{
    public static class RecetteConverter
    {
        public static Models.Recette Convert(this Shared.Recette recette)
        {
            return new Models.Recette
            {
                Id = recette.Id,
                Name = recette.Name,
                CreatedAt = recette.CreatedAt,
                Book_Id = recette.BookId,
                Book = recette.Book.Convert()
            };
        }
        public static Shared.Recette Convert(this Models.Recette recette)
        {
            return new Shared.Recette
            {
                Id = recette.Id,
                Name = recette.Name,
                CreatedAt = recette.CreatedAt,
                BookId = recette.Book_Id,
                Book = recette.Book.Convert()
            };
        }
    }
}
