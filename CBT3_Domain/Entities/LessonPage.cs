namespace CBT3_Domain.Entities;

public sealed class LessonPage : BaseEntity
{
    public LessonPage(LessonPageID id) : base(id) { }
    public LessonID LessonID { get; set; }
    public PageType LessonPageType { get; set; }
    public string LessonPageSubType { get; set; }
    public int PageOrder { get; set; }
    public string PageText { get; set; }
    public string VideoURL { get; set; }
    public string AudioURL { get; set; }
    public string ImageURL { get; set; }
}
