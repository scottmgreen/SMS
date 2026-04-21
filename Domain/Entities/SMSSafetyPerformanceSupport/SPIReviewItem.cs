//-----------------------------------------------------------------------
// <copyright file="SPIReviewItem.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain entity representing SPI review schedule item for management and oversight.
//                  Core domain model for SPI review tracking and scheduling.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Domain entity representing SPI review schedule item for management and oversight
/// Used for tracking review schedules and ensuring timely SPI assessments
/// </summary>
public class SPIReviewItem
{
    #region Core Properties
    public string SPIId { get; set; } = string.Empty;
    public string SPIName { get; set; } = string.Empty;
    public string ResponsibleDepartment { get; set; } = string.Empty;
    public string ReviewAuthority { get; set; } = string.Empty;
    public DateTime? NextReviewDate { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }
    public string Priority { get; set; } = string.Empty; // High, Medium, Low
    #endregion

    #region Business Rules
    /// <summary>
    /// Calculates if the review is overdue based on the next review date
    /// </summary>
    public bool CalculateIsOverdue()
    {
        if (!NextReviewDate.HasValue)
            return false;

        return DateTime.UtcNow.Date > NextReviewDate.Value.Date;
    }

    /// <summary>
    /// Calculates the number of days overdue
    /// </summary>
    public int CalculateDaysOverdue()
    {
        if (!NextReviewDate.HasValue || !CalculateIsOverdue())
            return 0;

        return (DateTime.UtcNow.Date - NextReviewDate.Value.Date).Days;
    }

    /// <summary>
    /// Determines priority based on days overdue and review frequency
    /// </summary>
    public string DeterminePriority()
    {
        if (!CalculateIsOverdue())
            return "Low";

        var daysOverdue = CalculateDaysOverdue();
        return daysOverdue switch
        {
            > 30 => "High",
            > 14 => "Medium",
            _ => "Low"
        };
    }

    /// <summary>
    /// Updates the review status based on current date
    /// </summary>
    public void UpdateReviewStatus()
    {
        IsOverdue = CalculateIsOverdue();
        DaysOverdue = CalculateDaysOverdue();
        Priority = DeterminePriority();
    }

    /// <summary>
    /// Marks the review as completed and sets next review date
    /// </summary>
    public void CompleteReview(DateTime completedDate, DateTime nextReviewDate)
    {
        LastReviewDate = completedDate;
        NextReviewDate = nextReviewDate;
        UpdateReviewStatus();
    }

    /// <summary>
    /// Gets the days until next review (negative if overdue)
    /// </summary>
    public int GetDaysUntilReview()
    {
        if (!NextReviewDate.HasValue)
            return int.MaxValue;

        return (NextReviewDate.Value.Date - DateTime.UtcNow.Date).Days;
    }
    #endregion

    #region Constructors
    public SPIReviewItem() { }

    public SPIReviewItem(string spiId, string spiName, string responsibleDepartment)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        SPIName = spiName ?? throw new ArgumentNullException(nameof(spiName));
        ResponsibleDepartment = responsibleDepartment ?? throw new ArgumentNullException(nameof(responsibleDepartment));
        Priority = "Low";
    }
    #endregion
}