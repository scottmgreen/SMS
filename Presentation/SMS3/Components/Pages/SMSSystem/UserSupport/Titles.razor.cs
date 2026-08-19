using SMS_Application.Interfaces;
using SMS_Application.Commands;
using SMS_Application.Queries;

using SMS_Domain.Enums;

namespace SMS3.Components.Pages.SMSSystem.UserSupport;

public partial class Titles : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private ILogger<Titles> _logger { get; set; } = default!;

    private RadzenDataGrid<SMSJobTitle>? _titlesGrid;
    private List<SMSJobTitle> _titles = new();
    private bool _showCreateModal;
    private bool _showEditModal;
    private bool _isSaving;
    private string _errorMessage = string.Empty;

    private string _newCode = string.Empty;
    private string _newName = string.Empty;

    private string _editOriginalCode = string.Empty;
    private string _editCode = string.Empty;
    private string _editName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadTitlesAsync();
    }

    private async Task LoadTitlesAsync()
    {
        _errorMessage = string.Empty;
        var result = await _mediator.SendAsync(new GetAllSMSJobTitlesQuery(), CancellationToken.None);
        if (result.IsSuccess)
        {
            _titles = result.Value?.OrderBy(t => t.Name).ToList() ?? new List<SMSJobTitle>();
            return;
        }

        _titles = new List<SMSJobTitle>();
        _errorMessage = result.Error?.Message ?? "Failed to load titles.";
    }

    private void OpenCreateModal()
    {
        _showCreateModal = true;
        _newCode = string.Empty;
        _newName = string.Empty;
        _errorMessage = string.Empty;
    }

    private void CloseCreateModal()
    {
        _showCreateModal = false;
    }

    private async Task CreateTitle()
    {
        if (string.IsNullOrWhiteSpace(_newName))
        {
            _errorMessage = "Title is required.";
            return;
        }

        try
        {
            _isSaving = true;
            _errorMessage = string.Empty;
            var generatedCode = "JT-0000";
            var title = SMSJobTitle.Create(generatedCode, _newName.Trim());
            var result = await _mediator.SendAsync(new CreateSMSJobTitleCommand(title), CancellationToken.None);
            if (result.IsFailure)
            {
                _errorMessage = result.Error?.Message ?? "Failed to create title.";
                return;
            }

            _showCreateModal = false;
            await LoadTitlesAsync();
            if (_titlesGrid is not null)
            {
                await _titlesGrid.Reload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating title");
            _errorMessage = "Error creating title.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void OpenEditModal(SMSJobTitle title)
    {
        _showEditModal = true;
        _editOriginalCode = title.Value;
        _editCode = title.Value;
        _editName = title.Name;
        _errorMessage = string.Empty;
    }

    private void CloseEditModal()
    {
        _showEditModal = false;
    }

    private async Task UpdateTitle()
    {
        if (string.IsNullOrWhiteSpace(_editOriginalCode) || string.IsNullOrWhiteSpace(_editName))
        {
            _errorMessage = "Title is required.";
            return;
        }

        try
        {
            _isSaving = true;
            _errorMessage = string.Empty;
            var title = SMSJobTitle.Create(_editOriginalCode.Trim(), _editName.Trim());
            var result = await _mediator.SendAsync(new UpdateSMSJobTitleCommand(_editOriginalCode, title), CancellationToken.None);
            if (result.IsFailure)
            {
                _errorMessage = result.Error?.Message ?? "Failed to update title.";
                return;
            }

            _showEditModal = false;
            await LoadTitlesAsync();
            if (_titlesGrid is not null)
            {
                await _titlesGrid.Reload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating title {Code}", _editOriginalCode);
            _errorMessage = "Error updating title.";
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task DeleteTitle(SMSJobTitle title)
    {
        if (string.IsNullOrWhiteSpace(title.Value))
        {
            _errorMessage = "Title code is required.";
            return;
        }

        try
        {
            var result = await _mediator.SendAsync(new DeleteSMSJobTitleCommand(title.Value), CancellationToken.None);
            if (result.IsFailure)
            {
                _errorMessage = result.Error?.Message ?? "Failed to delete title.";
                return;
            }

            await LoadTitlesAsync();
            if (_titlesGrid is not null)
            {
                await _titlesGrid.Reload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting title {Code}", title.Value);
            _errorMessage = "Error deleting title.";
        }
    }
}
