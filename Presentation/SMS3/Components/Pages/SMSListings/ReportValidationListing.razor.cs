using SMS_Domain.Entities;
using SMS_Domain.Events;
using SMS_Application.Queries;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSListings;

public partial class ReportValidationListing : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ReportValidationListing> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    

    private RadzenDataGrid<ReportValidation>? _validationsGrid;
    private IEnumerable<ReportValidation> _validations = new List<ReportValidation>();
    private readonly Dictionary<string, string> _userCodeToFullName = new(StringComparer.OrdinalIgnoreCase);
    private int _totalCount;

    protected override async Task OnInitializedAsync()
    {
        await LoadUserDisplayMapAsync();
        await LoadInitialData();
    }

    private async Task LoadUserDisplayMapAsync()
    {
        _userCodeToFullName.Clear();

        var usersResult = await _mediator.SendAsync(new GetAllSMSApplicationUsersQuery(), CancellationToken.None);
        if (!usersResult.IsSuccess || usersResult.Value is null)
        {
            return;
        }

        foreach (var user in usersResult.Value)
        {
            if (string.IsNullOrWhiteSpace(user.Code))
            {
                continue;
            }

            var firstName = user.FirstName?.Value?.Trim() ?? string.Empty;
            var lastName = user.LastName?.Value?.Trim() ?? string.Empty;
            var fullName = string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                _userCodeToFullName[user.Code] = fullName;
            }
        }
    }

    private string GetUserDisplayName(string? userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
        {
            return string.Empty;
        }

        return _userCodeToFullName.TryGetValue(userCode, out var fullName)
            ? fullName
            : userCode;
    }

    private async Task LoadInitialData()
    {
        try
        {
            var query = new GetAllReportValidationsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                _validations = result.Value;
                _totalCount = _validations.Count();
                _logger.LogInformation("Loaded {Count} report validations", _totalCount);
            }
            else
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to load report validations"));
                _logger.LogError("Failed to load report validations: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report validations");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading report validations"));
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            await LoadInitialData();

            var query = _validations.AsQueryable();

            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                var (propertyName, isDescending) = ParseOrderBy(args.OrderBy);
                if (!string.IsNullOrWhiteSpace(propertyName))
                {
                    query = isDescending
                        ? query.OrderByDescending(GetPropertyExpression(propertyName))
                        : query.OrderBy(GetPropertyExpression(propertyName));
                }
            }

            _totalCount = query.Count();

            if (args.Skip.HasValue)
            {
                query = query.Skip(args.Skip.Value);
            }

            const int pageSize = 15;
            query = query.Take(pageSize);

            _validations = query.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadData");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading data"));
        }
    }

    private static Expression<Func<ReportValidation, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(ReportValidation), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<ReportValidation, object>>(conversion, parameter);
    }

    private static (string propertyName, bool isDescending) ParseOrderBy(string orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return (string.Empty, false);
        }

        var parts = orderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var propertyName = parts[0];
        var isDescending = parts.Length > 1 && string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase);
        return (propertyName, isDescending);
    }

    private void ShowActions(ReportValidation validation)
    {
        _logger.LogInformation("Actions requested for report validation: {Code}", validation.Code);
    }
}
