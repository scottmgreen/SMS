using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;

namespace PDXSMS_Presentation.Pages.Documents;

public class ViewerModel : PageModel
{
    private readonly ILogger<ViewerModel> _logger;

    public ViewerModel(ILogger<ViewerModel> logger)
    {
        _logger = logger;
    }

    public string FilePath { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public IActionResult OnGet(string? file = null, string? category = null)
    {
        try
        {
            // Check if user is authenticated (you can add role-based checks later)
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrEmpty(file))
            {
                _logger.LogWarning("Document viewer accessed without file parameter");
                ErrorMessage = "No document specified.";
                return Page();
            }

            // Sanitize the file parameter to prevent directory traversal
            var sanitizedFile = global::System.IO.Path.GetFileName(file);
            if (string.IsNullOrEmpty(sanitizedFile))
            {
                _logger.LogWarning("Invalid file parameter: {File}", file);
                ErrorMessage = "Invalid document specified.";
                return Page();
            }

            // Determine the category (default to policies if not specified)
            var documentCategory = category ?? "policies";
            
            // Validate category
            var allowedCategories = new[] { "policies", "manuals", "templates", "forms" };
            if (!allowedCategories.Contains(documentCategory.ToLower()))
            {
                _logger.LogWarning("Invalid document category: {Category}", documentCategory);
                ErrorMessage = "Invalid document category.";
                return Page();
            }

            // Build the file path
            var relativePath = $"/documents/{documentCategory.ToLower()}/{sanitizedFile}";
            var physicalPath = global::System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", documentCategory.ToLower(), sanitizedFile);

            // Check if file exists
            if (!global::System.IO.File.Exists(physicalPath))
            {
                _logger.LogWarning("Requested document not found: {PhysicalPath}", physicalPath);
                ErrorMessage = $"Document '{sanitizedFile}' not found in {documentCategory}.";
                return Page();
            }

            // Verify it's a PDF file
            var extension = global::System.IO.Path.GetExtension(sanitizedFile).ToLower();
            if (extension != ".pdf")
            {
                _logger.LogWarning("Non-PDF file requested: {File}", sanitizedFile);
                ErrorMessage = "Only PDF documents can be viewed in the document viewer.";
                return Page();
            }

            FilePath = relativePath;
            DocumentName = global::System.IO.Path.GetFileNameWithoutExtension(sanitizedFile);

            _logger.LogInformation("Document viewer loaded: {File} by user {UserId}", sanitizedFile, userId);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading document viewer for file: {File}", file);
            ErrorMessage = "An error occurred while loading the document.";
            return Page();
        }
    }
}