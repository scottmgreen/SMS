using SMS_Application.Interfaces;
using SMS_Application.Commands;
using SMS_Application.Queries;

using SMS_Domain.Enums;
using System.Net.Mail;

namespace SMS3.Components.Pages.SMSSystem.UserSupport;

public partial class Companies : ComponentBase
{
    private sealed class CompanyAssignedUserItem
    {
        public string UserType { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private ILogger<Companies> _logger { get; set; } = default!;

    private RadzenDataGrid<SMSCompany>? _companiesGrid;
    private List<SMSCompany> _companies = new();

    private bool _showCreateModal;
    private bool _showEditModal;
    private bool _showAssignedUsersModal;
    private bool _isSaving;
    private string _successMessage = string.Empty;
    private string _errorMessage = string.Empty;
    private string _selectedCompanyDisplay = string.Empty;
    private List<CompanyAssignedUserItem> _assignedUsers = new();

    private string _newName = string.Empty;
    private string _newDescription = string.Empty;
    private string _newContactName = string.Empty;
    private string _newContactEmail = string.Empty;
    private string _newContactPhone = string.Empty;
    private string _newInternalRepresentative = string.Empty;

    private string _editCode = string.Empty;
    private string _editName = string.Empty;
    private string _editDescription = string.Empty;
    private string _editContactName = string.Empty;
    private string _editContactEmail = string.Empty;
    private string _editContactPhone = string.Empty;
    private string _editInternalRepresentative = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadCompaniesAsync();
    }

    private async Task LoadCompaniesAsync()
    {
        _errorMessage = string.Empty;
        var result = await _mediator.SendAsync(new GetAllSMSCompaniesQuery(), CancellationToken.None);
        if (result.IsSuccess)
        {
            _companies = result.Value?.OrderBy(c => c.Company).ToList() ?? new List<SMSCompany>();
            return;
        }

        _companies = new List<SMSCompany>();
        _errorMessage = result.Error?.Message ?? "Failed to load companies.";
    }

    private void OpenCreateModal()
    {
        _showCreateModal = true;
        _successMessage = string.Empty;
        _errorMessage = string.Empty;
        _newName = string.Empty;
        _newDescription = string.Empty;
        _newContactName = string.Empty;
        _newContactEmail = string.Empty;
        _newContactPhone = string.Empty;
        _newInternalRepresentative = string.Empty;
    }

    private void CloseCreateModal()
    {
        _showCreateModal = false;
        _errorMessage = string.Empty;
    }

    private async Task CreateCompany()
    {
        if (string.IsNullOrWhiteSpace(_newName))
        {
            _errorMessage = "Company Name is required.";
            return;
        }

        try
        {
            _isSaving = true;
            _errorMessage = string.Empty;
            _successMessage = string.Empty;
            var seedCode = $"COMP-{Guid.NewGuid():N}"[..13];

            var company = SMSCompany.Create(
                seedCode,
                string.Empty,
                _newName,
                _newContactName,
                string.Empty,
                _newDescription,
                _newContactEmail,
                _newContactPhone,
                _newInternalRepresentative);

            var result = await _mediator.SendAsync(new CreateSMSCompanyCommand(company), CancellationToken.None);

            if (result.IsFailure)
            {
                _errorMessage = result.Error?.Message ?? "Failed to create company.";
                return;
            }

            _showCreateModal = false;
            _successMessage = $"Company '{_newName}' created successfully.";
            await LoadCompaniesAsync();
            if (_companiesGrid is not null)
            {
                await _companiesGrid.Reload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            _errorMessage = "Error creating company.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void OpenEditModal(SMSCompany company)
    {
        _showEditModal = true;
        _successMessage = string.Empty;
        _errorMessage = string.Empty;
        _editCode = company.Value;
        _editName = company.Company;
        _editDescription = company.ServiceProvided;
        _editContactName = company.ContactName;
        _editContactEmail = company.Email;
        _editContactPhone = company.Phone;
        _editInternalRepresentative = company.PortRep;
    }

    private void CloseEditModal()
    {
        _showEditModal = false;
        _editCode = string.Empty;
        _errorMessage = string.Empty;
    }

    private async Task UpdateCompany()
    {
        if (string.IsNullOrWhiteSpace(_editCode) || string.IsNullOrWhiteSpace(_editName))
        {
            _errorMessage = "Company Code and Name are required.";
            return;
        }

        try
        {
            _isSaving = true;
            _errorMessage = string.Empty;
            _successMessage = string.Empty;

            var company = SMSCompany.Create(
                _editCode,
                string.Empty,
                _editName,
                _editContactName,
                string.Empty,
                _editDescription,
                _editContactEmail,
                _editContactPhone,
                _editInternalRepresentative);

            var result = await _mediator.SendAsync(new UpdateSMSCompanyCommand(company), CancellationToken.None);

            if (result.IsFailure)
            {
                _errorMessage = result.Error?.Message ?? "Failed to update company.";
                return;
            }

            _showEditModal = false;
            _successMessage = $"Company '{_editName}' updated successfully.";
            await LoadCompaniesAsync();
            if (_companiesGrid is not null)
            {
                await _companiesGrid.Reload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company {Code}", _editCode);
            _errorMessage = "Error updating company.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task ConfirmDelete(SMSCompany company)
    {
        if (string.IsNullOrWhiteSpace(company.Value))
        {
            _errorMessage = "Company code is required.";
            return;
        }

        try
        {
            var result = await _mediator.SendAsync(new DeleteSMSCompanyCommand(company.Value), CancellationToken.None);

            if (result.IsFailure)
            {
                _errorMessage = result.Error?.Message ?? "Failed to delete company.";
                return;
            }

            _errorMessage = string.Empty;
            _successMessage = $"Company '{company.Company}' deleted successfully.";
            await LoadCompaniesAsync();
            if (_companiesGrid is not null)
            {
                await _companiesGrid.Reload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company {Code}", company.Value);
            _errorMessage = "Error deleting company.";
        }
    }

    private async Task ShowAssignedUsers(SMSCompany company)
    {
        try
        {
            _errorMessage = string.Empty;
            _selectedCompanyDisplay = $"{company.Company} ({company.Value})";

            var appUsersResult = await _mediator.SendAsync(new GetAllSMSApplicationUsersQuery(), CancellationToken.None);
            var orgUsersResult = await _mediator.SendAsync(new GetAllSMSOrganizationalUsersQuery(), CancellationToken.None);
            var stakeholderUsersResult = await _mediator.SendAsync(new GetAllSMSStakeholderUsersQuery(), CancellationToken.None);

            var companyCode = company.Value?.Trim() ?? string.Empty;
            var companyName = company.Company?.Trim() ?? string.Empty;

            var applicationUsers = (appUsersResult.IsSuccess ? appUsersResult.Value : Enumerable.Empty<SMSApplicationUser>())
                .Where(u => IsUserAssignedToCompany(u.Company, companyCode, companyName))
                .Select(u => new CompanyAssignedUserItem
                {
                    UserType = "Application",
                    UserCode = u.Code,
                    DisplayName = u.DisplayName,
                    UserName = u.UserName?.Value ?? string.Empty,
                    Organization = SMSOrganization.FromValue(u.Organization ?? string.Empty)?.Name ?? u.Organization ?? string.Empty,
                    Title = SMSJobTitle.FromValue(u.Title ?? string.Empty)?.Name ?? u.Title ?? string.Empty,
                    IsActive = u.IsActive
                });

            var organizationalUsers = (orgUsersResult.IsSuccess ? orgUsersResult.Value : Enumerable.Empty<SMSOrganizationalUser>())
                .Where(u => IsUserAssignedToCompany(u.Company, companyCode, companyName))
                .Select(u => new CompanyAssignedUserItem
                {
                    UserType = "Organizational",
                    UserCode = u.Code,
                    DisplayName = u.DisplayName,
                    UserName = u.UserName?.Value ?? string.Empty,
                    Organization = SMSOrganization.FromValue(u.Organization ?? string.Empty)?.Name ?? u.Organization ?? string.Empty,
                    Title = SMSJobTitle.FromValue(string.IsNullOrWhiteSpace(u.Title) ? u.Position : u.Title)?.Name
                            ?? u.Title
                            ?? u.Position
                            ?? string.Empty,
                    IsActive = u.IsActive
                });

            var stakeholderUsers = (stakeholderUsersResult.IsSuccess ? stakeholderUsersResult.Value : Enumerable.Empty<SMSStakeholderUser>())
                .Where(u => IsUserAssignedToCompany(u.Company, companyCode, companyName))
                .Select(u => new CompanyAssignedUserItem
                {
                    UserType = "Stakeholder",
                    UserCode = u.Code,
                    DisplayName = u.DisplayName,
                    UserName = u.UserName?.Value ?? string.Empty,
                    Organization = SMSOrganization.FromValue(u.Organization ?? string.Empty)?.Name ?? u.Organization ?? string.Empty,
                    Title = SMSJobTitle.FromValue(u.Title ?? string.Empty)?.Name ?? u.Title ?? string.Empty,
                    IsActive = u.IsActive
                });

            _assignedUsers = applicationUsers
                .Concat(organizationalUsers)
                .Concat(stakeholderUsers)
                .OrderBy(u => u.DisplayName)
                .ToList();

            _showAssignedUsersModal = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assigned users for company {CompanyCode}", company.Value);
            _errorMessage = "Error loading assigned users.";
        }
    }

    private void CloseAssignedUsersModal()
    {
        _showAssignedUsersModal = false;
        _selectedCompanyDisplay = string.Empty;
        _assignedUsers = new();
    }

    private static bool IsUserAssignedToCompany(string? userCompany, string companyCode, string companyName)
    {
        if (string.IsNullOrWhiteSpace(userCompany))
        {
            return false;
        }

        var normalizedUserCompany = userCompany.Trim();

        return string.Equals(normalizedUserCompany, companyCode, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedUserCompany, companyName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEmailFormat(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        try
        {
            var addr = new MailAddress(userName.Trim());
            return string.Equals(addr.Address, userName.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
