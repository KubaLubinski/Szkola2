namespace Szkola2.Models
{
    public class TextsView
    {
        public TextsView(int pageSize = 5)
        {
            PageSize = pageSize;
        }

        public int TextCount { get; set; }
        public int PageSize { get; set; }  
        public int PageNumber { get; set; }
        public int PageCount => (int) Math.Ceiling((decimal)TextCount/PageSize);
    }
}
