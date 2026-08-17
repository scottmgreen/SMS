using SMS_Application.Interfaces;

using SMS_Domain.Enums;
using SMS_Infrastructure.Interfaces;

namespace SMS3.Components.Pages.SMSSystem.UserSupport;

public partial class Companies : ComponentBase
{
    [Inject] private ISMSCompanyRepository _companyRepository { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private ILogger<Companies> _logger { get; set; } = default!;

    private RadzenDataGrid<SMSCompany>? _companiesGrid;
    private List<SMSCompany> _companies = new();

    private bool _showCreateModal;
    private bool _showEditModal;
    private bool _isSaving;
    private string _successMessage = string.Empty;
    private string _errorMessage = string.Empty;

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
        var result = await _companyRepository.GetAllAsync();
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

            var createdBy = string.IsNullOrWhiteSpace(_currentUserService.UserCode) ? "SYSTEM" : _currentUserService.UserCode;
            var result = await _companyRepository.CreateAsync(company, createdBy);

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

            var updatedBy = string.IsNullOrWhiteSpace(_currentUserService.UserCode) ? "SYSTEM" : _currentUserService.UserCode;
            var result = await _companyRepository.UpdateAsync(company, updatedBy);

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
            var deletedBy = string.IsNullOrWhiteSpace(_currentUserService.UserCode) ? "SYSTEM" : _currentUserService.UserCode;
            var result = await _companyRepository.DeleteAsync(company.Value, deletedBy);

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
}
