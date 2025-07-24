namespace CBT3_Domain.Interfaces;

public interface IBaseLessonPage
{
    string AudioURL { get; set; }
    string ImageURL { get; set; }
    string LessonID { get; set; }
    PageType LessonPageType { get; set; }
    int PageOrder { get; set; }
    string PageText { get; set; }
    string VideoURL { get; set; }
}