//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author: Scott Green
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using CBT3_Domain.Entities;

namespace CBT3_UI;

public class CBT3_App
{
    public TrainingStation? TrainingStation { get; set; } = null;
    public Trainee? Trainee { get; set; } = null;
    public Course? Course { get; set; } = null;
    public LessonPage? LessonPage { get; set; } = null;
    public bool CoursePass { get; set; } = false;

    public TrainingSession? TrainingSession { get; set; } = null;


    //public static class CustomPageTypeComponentMapping
    //{
    //    public static readonly Dictionary<PageType, Type> PageTypeComponentMap = new Dictionary<PageType, Type>
    //    {
    //        { PageType.PT_CUSTOM_01, typeof(PT_CUSTOM_01) },
    //        { PageType.PT_CUSTOM_02, typeof(PT_CUSTOM_02) },
    //        { PageType.PT_CUSTOM_03, typeof(PT_CUSTOM_03) },
    //        { PageType.PT_CUSTOM_04, typeof(PT_CUSTOM_04) },
    //        { PageType.PT_CUSTOM_05, typeof(PT_CUSTOM_05) },
    //        { PageType.PT_CUSTOM_06, typeof(PT_CUSTOM_06) },
    //        { PageType.PT_CUSTOM_07, typeof(PT_CUSTOM_07) }
    //    };
    //}



}
