using SMS_Application.Interfaces;

using SMS_Domain.Enums;
using SMS_Infrastructure.Interfaces;

namespace SMS3.Components.Pages.SMSSystem.UserSupport;

public partial class Organizations : ComponentBase
{
    [Inject] private ISMSOrganizationRepository _organizationRepository { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private ILogger<Organizations> _logger { get; set; } = default!;

    private RadzenDataGrid<SMSOrganization>? _organizationsGrid;
    private List<SMSOrganization> _organizations = new();
    private bool _showCreateModal;
    private bool _showEditModal;
    private bool _isSaving;
    private string _errorMessage = string.Empty;

    private string _newValue = string.Empty;
    private string _newName = string.Empty;
    private string _newDescription = string.Empty;

    private string _editOriginalValue = string.Empty;
    private string _editValue = string.Empty;
    private string _editName = string.Empty;
    private string _editDescription = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadOrganizationsAsync();
    }

    private async Task LoadOrganizationsAsync()
    {
        var result = await _organizationRepository.GetAllAsync();
        if (result.IsSuccess)
        {
            _organizations = result.Value?.OrderBy(o => o.Name).ToList() ?? new List<SMSOrganization>();
            return;
        }

        _organizations = new List<SMSOrganization>();
        _errorMessage = result.Error?.Message ?? "Failed to load organizations.";
    }

    private void OpenCreateModal()
    {
        _showCreateModal = true;
        _newValue = string.Empty;
        _newName = string.Empty;
        _newDescription = string.Empty;
    }

    private void CloseCreateModal()
    {
        _showCreateModal = false;
    }

    private async Task CreateOrganization()
    {
        if (string.IsNullOrWhiteSpace(_newValue) || string.IsNullOrWhiteSpace(_newName))
        {
            _errorMessage = "Value and Name are required.";
            return;
        }

        try
        {
            _isSaving = true;
            var org = SMSOrganization.Create(_newValue.Trim(), _newName.Trim(), _newDescription?.Trim() ?? string.Empty, Array.Empty<string>());
            var createdBy = string.IsNullOrWhiteSpace(_currentUserService.UserCode) ? "SYSTEM" : _currentUserService.UserCode;

            var result = await _organizationRepository.CreateAsync(org, createdBy);
            if (result.IsFailure)
            {
                _errorMessage = result.Error?.Message ?? "Failed to create organization.";
                return;
            }

            _showCreateModal = false;
            await LoadOrganizationsAsync();
            if (_organizationsGrid is not null)
            {
                await _organizationsGrid.Reload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization");
            _errorMessage = "Error creating organization.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void OpenEditModal(SMSOrganization organization)
    {
        _showEditModal = true;
        _editOriginalValue = organization.Value;
        _editValue = organization.Value;
        _editName = organization.Name;
        _editDescription = organization.Description;
    }

    private void CloseEditModal()
    {
        _showEditModal = false;
    }

    private async Task UpdateOrganization()
    {
        if (string.IsNullOrWhiteSpace(_editOriginalValue) || string.IsNullOrWhiteSpace(_editValue) || string.IsNullOrWhiteSpace(_editName))
        {
            _errorMessage = "Value and Name are required.";
            return;
        }

        try
        {
            _isSaving = true;
            var org = SMSOrganization.Create(_editValue.Trim(), _editName.Trim(), _editDescription?.Trim() ?? string.Empty, Array.Empty<string>());
            var updatedBy = string.IsNullOrWhiteSpace(_currentUserService.UserCode) ? "SYSTEM" : _currentUserService.UserCode;

            var result = await _organizationRepository.UpdateAsync(_editOriginalValue, org, updatedBy);
            if (result.IsFailure)
            {
                _errorMessage = result.Error?.Message ?? "Failed to update organization.";
                return;
            }

            _showEditModal = false;
            await LoadOrganizationsAsync();
            if (_organizationsGrid is not null)
            {
                await _organizationsGrid.Reload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {Code}", _editOriginalValue);
            _errorMessage = "Error updating organization.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task DeleteOrganization(SMSOrganization organization)
    {
        if (string.IsNullOrWhiteSpace(organization.Value))
        {
            _errorMessage = "Organization value is required.";
            return;
        }

        try
        {
            var deletedBy = string.IsNullOrWhiteSpace(_currentUserService.UserCode) ? "SYSTEM" : _currentUserService.UserCode;
            var result = await _organizationRepository.DeleteAsync(organization.Value, deletedBy);
            if (result.IsFailure)
            {
                _errorMessage = result.Error?.Message ?? "Failed to delete organization.";
                return;
            }

            await LoadOrganizationsAsync();
            if (_organizationsGrid is not null)
            {
                await _organizationsGrid.Reload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization {Code}", organization.Value);
            _errorMessage = "Error deleting organization.";
        }
    }
}
