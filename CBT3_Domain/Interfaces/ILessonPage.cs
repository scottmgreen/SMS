namespace CBT3_Domain.Interfaces;

public interface ILessonPage
{
    string AudioURL { get; set; }
    string ImageURL { get; set; }
    string LessonID { get; set; }
    string LessonPageSubType { get; set; }
    PageType LessonPageType { get; set; }
    int PageOrder { get; set; }
    string PageText { get; set; }
    string VideoURL { get; set; }
}