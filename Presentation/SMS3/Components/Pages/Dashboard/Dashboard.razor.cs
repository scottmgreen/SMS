using Microsoft.AspNetCore.Components;

namespace SMS3.Components.Pages.Dashboard;

public partial class Dashboard : ComponentBase
{
    // Dashboard metrics
    public int TotalReports { get; set; } = 127;
    public int ActiveHazards { get; set; } = 23;
    public int PendingAssessments { get; set; } = 8;
    public int CompletedActions { get; set; } = 45;

    // Chart data
    public List<DataPoint> ReportsByMonth { get; set; } = new();
    public List<DataPoint> HazardsByType { get; set; } = new();

    protected override void OnInitialized()
    {
        LoadDashboardData();
    }

    private void LoadDashboardData()
    {
        // Sample data for reports by month
        ReportsByMonth = new List<DataPoint>
        {
            new("Jan", 15),
            new("Feb", 22),
            new("Mar", 18),
            new("Apr", 25),
            new("May", 20),
            new("Jun", 27)
        };

        // Sample data for hazards by type
        HazardsByType = new List<DataPoint>
        {
            new("Safety", 12),
            new("Security", 8),
            new("Environmental", 5),
            new("Operational", 18)
        };
    }

    public class DataPoint
    {
        public string Label { get; set; }
        public int Value { get; set; }

        public DataPoint(string label, int value)
        {
            Label = label;
            Value = value;
        }
    }
}