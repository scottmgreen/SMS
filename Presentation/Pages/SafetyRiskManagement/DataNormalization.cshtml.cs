using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.Services;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// Admin page to normalize and clean up risk assessment data
/// </summary>
public class DataNormalizationModel : PageModel
{
    private readonly ILogger<DataNormalizationModel> _logger;
    private readonly RiskAssessmentDataNormalizationService _normalizationService;

    public DataNormalizationModel(
        ILogger<DataNormalizationModel> logger,
        RiskAssessmentDataNormalizationService normalizationService)
    {
        _logger = logger;
        _normalizationService = normalizationService;
    }

    [BindProperty]
    public string? ResultMessage { get; set; }

    [BindProperty]
    public bool IsSuccess { get; set; }

    public void OnGet()
    {
        // Display the normalization page
    }

    public async Task<IActionResult> OnPostNormalizeAsync()
    {
        try
        {
            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
            var riskAssessmentsFile = Path.Combine(appDataPath, "risk-assessments.json");

            _logger.LogInformation("🔧 Starting risk assessment data normalization...");

            var success = await _normalizationService.NormalizeRiskAssessmentFileAsync(riskAssessmentsFile);

            if (success)
            {
                ResultMessage = "✅ Risk assessment data normalized successfully! Redundant data has been cleaned up and structure optimized.";
                IsSuccess = true;
                _logger.LogInformation("✅ Data normalization completed successfully");
            }
            else
            {
                ResultMessage = "❌ Failed to normalize risk assessment data. Please check the logs for details.";
                IsSuccess = false;
                _logger.LogError("❌ Data normalization failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during data normalization");
            ResultMessage = $"❌ Error during normalization: {ex.Message}";
            IsSuccess = false;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostBackupCurrentDataAsync()
    {
        try
        {
            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
            var riskAssessmentsFile = Path.Combine(appDataPath, "risk-assessments.json");
            
            if (!global::System.IO.File.Exists(riskAssessmentsFile))
            {
                ResultMessage = "❌ Risk assessments file not found.";
                IsSuccess = false;
                return Page();
            }

            var backupPath = Path.Combine(appDataPath, "Backups");
            global::System.IO.Directory.CreateDirectory(backupPath);
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFile = Path.Combine(backupPath, $"risk-assessments_backup_{timestamp}.json");
            
            var sourceContent = await global::System.IO.File.ReadAllTextAsync(riskAssessmentsFile);
            await global::System.IO.File.WriteAllTextAsync(backupFile, sourceContent);
            
            ResultMessage = $"✅ Backup created successfully: {Path.GetFileName(backupFile)}";
            IsSuccess = true;
            
            _logger.LogInformation("📁 Created backup: {BackupFile}", backupFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating backup");
            ResultMessage = $"❌ Error creating backup: {ex.Message}";
            IsSuccess = false;
        }

        return Page();
    }
}