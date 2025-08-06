// -----------------------------------------------------------------------------
// <copyright file="CBT3_App.cs" company="">
//     Author: Scott Green
//     Date: 2025-07-24
//     Summary: Application state container for the CBT3 Blazor UI.
// </copyright>
// -----------------------------------------------------------------------------

using CBT3_Domain.Entities;

namespace CBT3_UI;

/// <summary>
/// Application state container for the CBT3 Blazor UI.
/// </summary>
public class CBT3_App
{
    /// <summary>
    /// Gets or sets the current training station.
    /// </summary>
    public TrainingStation? TrainingStation { get; set; } = null;
    /// <summary>
    /// Gets or sets the current trainee.
    /// </summary>
    public Trainee? Trainee { get; set; } = null;
    /// <summary>
    /// Gets or sets the current course.
    /// </summary>
    public Course? Course { get; set; } = null;
    /// <summary>
    /// Gets or sets the current lesson page.
    /// </summary>
    public LessonPage? LessonPage { get; set; } = null;
    /// <summary>
    /// Gets or sets a value indicating whether the course was passed.
    /// </summary>
    public bool CoursePass { get; set; } = false;
    /// <summary>
    /// Gets or sets the current training session.
    /// </summary>
    public TrainingSession? TrainingSession { get; set; } = null;
}
