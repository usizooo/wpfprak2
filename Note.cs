namespace Notes
{
    public class Note
    {
        public string Title { get; set; } 
        public string Description { get; set; }
        public Note(string title, string definition)
        {
            Title = title;
            Description = definition;
        }
    }
}
