
using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSPolicy;

public partial class OrganizationalStructure : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<OrganizationalStructure> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;

    // Data Properties
    private List<SMSOrganizationalUser> OrganizationalUsers { get; set; } = new();
    private List<SMSOrganizationalUser> UnassignedUsers { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;

    // UI State Properties
    private bool showCategorySummary { get; set; } = true; // HazardCategory Summary collapsible state
    private bool showOrganizationalHierarchy { get; set; } = true; // Organizational Hierarchy collapsible state
    private bool showAssignmentMatrix { get; set; } = true; // Assignment Matrix collapsible state

    // Assignment modal properties
    private bool ShowAssignmentModal { get; set; } = false;
    private SMSOrganizationalLevel? SelectedLevelForAssignment { get; set; }
    private string SelectedUserForAssignment { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            // Load organizational users using existing CQRS - only active users
            var organizationalUsersQuery = new GetAllSMSOrganizationalUsersQuery();
            var usersResult = await _mediator.SendAsync(organizationalUsersQuery, CancellationToken.None);

            var allUsers = usersResult.IsSuccess ?
                usersResult.Value?.ToList() ?? new List<SMSOrganizationalUser>() :
                new List<SMSOrganizationalUser>();

            // Filter for active users only
            OrganizationalUsers = allUsers.Where(u => u.IsActive).ToList();

            // Get unassigned active users (no organization level or empty)
            UnassignedUsers = OrganizationalUsers
                .Where(u => u.OrganizationLevel == SMSOrganizationalLevel.UnassignedLevel)
                .OrderBy(u => u.DisplayName)
                .ToList();

            _logger.LogInformation("Loaded {UserCount} active organizational users for structure display", OrganizationalUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading organizational structure data");
            await _notificationHelper.ShowErrorAsync("Error loading organizational structure data. Please refresh the page.");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region SMS Organizational Level Integration

    private List<SMSOrganizationalLevelInfo> GetSMSRoles()
    {
        return SMSOrganizationalLevel.GetAllValues()
            .Where(l=> l.AuthorityLevel > 0)
            .OrderByDescending(level => level.AuthorityLevel)
            .Select(level => new SMSOrganizationalLevelInfo
            {
                Level = level,
                AssignedUsers = GetUsersForLevel(level),
                HierarchyPosition = GetHierarchyPosition(level.AuthorityLevel),
                EligibleUsers = GetEligibleUsersForLevel(level)
            }).ToList();
    }

    private List<SMSOrganizationalUser> GetUsersForLevel(SMSOrganizationalLevel level)
    {
        return OrganizationalUsers
            .Where(user => !string.IsNullOrEmpty(user.OrganizationLevel) &&
                          user.OrganizationLevel.Value.Equals(level.Value, StringComparison.OrdinalIgnoreCase))
            .OrderBy(user => user.DisplayName)
            .ToList();
    }

    private List<SMSOrganizationalUser> GetEligibleUsersForLevel(SMSOrganizationalLevel level)
    {
        // For now, return all unassigned users. 
        // You could add business logic here to filter based on department, position, etc.
        return UnassignedUsers.ToList();
    }

    private int GetHierarchyPosition(int authorityLevel)
    {
        // Map authority levels to visual hierarchy positions (1 = top level, higher numbers = more indented)
        return authorityLevel switch
        {
            10 => 1, // Accountable Executive
            9 => 2,  // Responsible Executive 
            8 => 3,  // Responsible Manager
            7 => 4,  // SMS Manager
            6 => 5,  // SMS Coordinator
            5 => 6,  // SMS Team Member
            _ => 7   // Other levels
        };
    }

    #endregion

    #region User Assignment Operations

    private void OpenAssignmentModal(SMSOrganizationalLevel level)
    {
        SelectedLevelForAssignment = level;
        SelectedUserForAssignment = string.Empty;
        ShowAssignmentModal = true;
    }

    private void CloseAssignmentModal()
    {
        ShowAssignmentModal = false;
        SelectedLevelForAssignment = null;
        SelectedUserForAssignment = string.Empty;
    }

    private async Task AssignUserToLevel()
    {
        if (SelectedLevelForAssignment is null || string.IsNullOrEmpty(SelectedUserForAssignment))
        {
            await _notificationHelper.ShowErrorAsync("Please select a user to assign.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Find the user
            var user = UnassignedUsers.FirstOrDefault(u => u.Code == SelectedUserForAssignment);
            if (user is null)
            {
                await _notificationHelper.ShowErrorAsync("Selected user not found.");
                return;
            }

            // Update the user's organization level
            user.OrganizationLevel = SelectedLevelForAssignment;

            // Send update command
            var updateCommand = new UpdateSMSOrganizationalUserCommand(user);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await _notificationHelper.ShowSuccessAsync($"Successfully assigned {user.DisplayName} to {SelectedLevelForAssignment.Name}.");
                CloseAssignmentModal();
                await LoadDataAsync(); // Refresh the data
            }
            else
            {
                await _notificationHelper.ShowErrorAsync(result.Error?.Message ?? "Failed to assign user to organizational level.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user to organizational level");
            await _notificationHelper.ShowErrorAsync("Error assigning user. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task UnassignUserFromLevel(SMSOrganizationalUser user)
    {
        try
        {
            var result = await _dialogService.Confirm(
                $"Remove {user.DisplayName} from {user.OrganizationLevel}?",
                "Confirm Removal",
                new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" }
            );

            if (result == true)
            {
                IsSaving = true;
                StateHasChanged();

                // Clear the user's organization level
                user.OrganizationLevel = SMSOrganizationalLevel.UnassignedLevel;

                // Send update command
                var updateCommand = new UpdateSMSOrganizationalUserCommand(user);
                var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

                if (updateResult.IsSuccess)
                {
                    await _notificationHelper.ShowSuccessAsync($"Successfully removed {user.DisplayName} from organizational level.");
                    await LoadDataAsync(); // Refresh the data
                }
                else
                {
                    await _notificationHelper.ShowErrorAsync(updateResult.Error?.Message ?? "Failed to remove user from organizational level.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user from organizational level");
            await _notificationHelper.ShowErrorAsync("Error removing user. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Statistics and Status

    private int GetTotalRoles() => SMSOrganizationalLevel.GetAllValues().Count();

    private int GetFilledRoles()
    {
        return SMSOrganizationalLevel.GetAllValues()
            .Count(level => GetUsersForLevel(level).Any());
    }

    private int GetVacantRoles()
    {
        return SMSOrganizationalLevel.GetAllValues()
            .Count(level => !GetUsersForLevel(level).Where(x => x.OrganizationLevel != SMSOrganizationalLevel.UnassignedLevel).Any());
    }

    private int GetCompliancePercentage()
    {
        var total = GetTotalRoles();
        var filled = GetFilledRoles();
        return total > 0 ? (int)Math.Round((double)filled / total * 100) : 0;
    }

    private int GetTotalAssignedUsers()
    {
        return OrganizationalUsers.Count(user => !string.IsNullOrEmpty(user.OrganizationLevel));
    }

    #endregion

    #region UI Rendering Helpers

    private RenderFragment RenderRoleCard(SMSOrganizationalLevelInfo roleInfo)
    {
        return builder =>
        {
            var hasAssignments = roleInfo.AssignedUsers.Any();
            var borderColor = hasAssignments ? "var(--rz-success)" : "var(--rz-warning)";
            var indentLevel = (roleInfo.HierarchyPosition - 1) * 30;

            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "rz-card rz-variant-outlined");
            builder.AddAttribute(2, "style", $"border-left: 4px solid {borderColor}; margin-bottom: 1rem; margin-left: {indentLevel}px;");

            builder.OpenElement(3, "div");
            builder.AddAttribute(4, "class", "rz-card-content");
            builder.AddAttribute(5, "style", "padding: 1rem;");

            // Header with role info and assign button
            builder.OpenElement(6, "div");
            builder.AddAttribute(7, "style", "display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1rem;");

            // Left side - Role info
            builder.OpenElement(8, "div");
            builder.AddAttribute(9, "style", "flex: 1;");

            // Title row
            builder.OpenElement(10, "div");
            builder.AddAttribute(11, "style", "display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.5rem;");

            builder.OpenElement(12, "h6");
            builder.AddAttribute(13, "style", "margin: 0; font-weight: 600;");
            builder.AddContent(14, roleInfo.Level.Name);
            builder.CloseElement();

            builder.OpenComponent<RadzenBadge>(20);
            builder.AddAttribute(21, "Text", $"{roleInfo.Level.Category} Authority {roleInfo.Level.AuthorityLevel}");
            builder.AddAttribute(22, "BadgeStyle", GetCategoryBadgeStyle(roleInfo.Level.Category));
            builder.AddAttribute(23, "Variant", Variant.Text);
            builder.AddAttribute(24, "Style", "font-size: 0.75em;");
            builder.CloseComponent();

            //builder.OpenComponent<RadzenBadge>(20);
            //builder.AddAttribute(21, "Text", roleInfo.Level.Name);
            //builder.AddAttribute(22, "BadgeStyle", GetCategoryBadgeStyle(roleInfo.Level.Category));
            //builder.AddAttribute(23, "Variant", Variant.Text);
            //builder.AddAttribute(24, "Style", $"font-size: 0.75em; margin-right: 0.5rem;");
            //builder.CloseComponent();

            //builder.OpenComponent<RadzenBadge>(25);
            //builder.AddAttribute(26, "Text", $"Authority {roleInfo.Level.AuthorityLevel}");
            //builder.AddAttribute(27, "BadgeStyle", BadgeStyle.Info);
            //builder.AddAttribute(28, "Variant", Variant.Outlined);
            //builder.AddAttribute(29, "Style", $"font-size: 0.75em; {GetAuthorityLevelCustomStyle(roleInfo.Level.AuthorityLevel)}");
            //builder.CloseComponent();


            //builder.OpenComponent<RadzenBadge>(15);
            //builder.AddAttribute(16, "Text", $"Authority {roleInfo.Level.AuthorityLevel}");
            //builder.AddAttribute(17, "BadgeStyle", @GetAuthorityLevelBadgeStyle(roleInfo.Level.AuthorityLevel));
            //builder.AddAttribute(18, "Variant", Variant.Outlined);
            //builder.AddAttribute(19, "Style", "font-size: 0.75em;");
            //builder.CloseComponent();

           

            builder.CloseElement(); // Title row

            // Description
            builder.OpenElement(25, "p");
            builder.AddAttribute(26, "style", "margin: 0; color: var(--rz-text-secondary-color); font-size: 0.875rem; margin-bottom: 0.75rem;");
            builder.AddContent(27, roleInfo.Level.Description);
            builder.CloseElement();

            builder.CloseElement(); // Left side

            // Right side - Assign button
            builder.OpenElement(28, "div");
            builder.AddAttribute(29, "style", "display: flex; flex-direction: column; gap: 0.5rem;");

            if (roleInfo.EligibleUsers.Any())
            {
                builder.OpenComponent<RadzenButton>(30);
                builder.AddAttribute(31, "Text", "Assign User");
                builder.AddAttribute(32, "Icon", "person_add");
                builder.AddAttribute(33, "ButtonStyle", ButtonStyle.Success);
                builder.AddAttribute(34, "Size", ButtonSize.Small);
                builder.AddAttribute(35, "Click", EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, (args) => OpenAssignmentModal(roleInfo.Level)));
                builder.AddAttribute(36, "Disabled", IsSaving);
                builder.AddAttribute(37, "title", "Assign a user to this role");
                builder.CloseComponent();
            }

            builder.CloseElement(); // Right side
            builder.CloseElement(); // Header

            // Assigned users section
            if (hasAssignments)
            {
                builder.OpenElement(40, "div");
                builder.AddAttribute(41, "style", "margin-top: 0.5rem;");

                foreach (var user in roleInfo.AssignedUsers.Take(3))
                {
                    builder.OpenElement(42, "div");
                    builder.AddAttribute(43, "style", "display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem; background: var(--rz-success-lighter); border-radius: 4px; margin-bottom: 0.25rem;");

                    //builder.OpenComponent<RadzenGravatar>(44);
                    //builder.AddAttribute(45, "Email", $"{user.FirstName?.Value?.ToLower()}.{user.LastName?.Value?.ToLower()}@organization.com");
                    //builder.AddAttribute(46, "Size", 32);
                    //builder.CloseComponent();

                    builder.OpenElement(47, "div");
                    builder.AddAttribute(48, "style", "flex: 1;");

                    builder.OpenElement(49, "div");
                    builder.AddAttribute(50, "style", "font-weight: 600; margin: 0; font-size: 0.875rem;");
                    builder.AddContent(51, user.DisplayName);
                    builder.CloseElement();

                    builder.OpenElement(52, "div");
                    builder.AddAttribute(53, "style", "color: var(--rz-text-secondary-color); font-size: 0.75rem; margin: 0;");
                    builder.AddContent(54, $"{user.Department} - {user.Position}");
                    builder.CloseElement();

                    builder.CloseElement(); // User info

                    //builder.OpenComponent<RadzenBadge>(55);
                    //builder.AddAttribute(56, "Text", "Assigned");
                    //builder.AddAttribute(57, "BadgeStyle", BadgeStyle.Success);
                    //builder.AddAttribute(58, "Variant", Variant.Filled);
                    //builder.AddAttribute(59, "Style", "font-size: 0.75em;");
                    //builder.CloseComponent();

                    builder.OpenComponent<RadzenButton>(60);
                    builder.AddAttribute(61, "Icon", "close");
                    builder.AddAttribute(62, "ButtonStyle", ButtonStyle.Danger);
                    builder.AddAttribute(63, "Size", ButtonSize.ExtraSmall);
                    builder.AddAttribute(64, "Click", EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, (args) => UnassignUserFromLevel(user)));
                    builder.AddAttribute(65, "title", "Remove user from this role");
                    builder.AddAttribute(66, "Disabled", IsSaving);
                    builder.CloseComponent();

                    builder.CloseElement(); // User row
                }

                if (roleInfo.AssignedUsers.Count > 3)
                {
                    builder.OpenElement(70, "div");
                    builder.AddAttribute(71, "style", "padding: 0.25rem 0.5rem; text-align: center; color: var(--rz-text-secondary-color); font-size: 0.75rem; font-style: italic;");
                    builder.AddContent(72, $"+{roleInfo.AssignedUsers.Count - 3} more assigned to this role");
                    builder.CloseElement();
                }

                builder.CloseElement(); // Assigned users section
            }
            else
            {
                builder.OpenElement(80, "div");
                builder.AddAttribute(81, "style", "display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem; background: var(--rz-warning-lighter); border-radius: 4px; margin-top: 0.5rem;");

                builder.OpenComponent<RadzenIcon>(82);
                builder.AddAttribute(83, "Icon", "person_off");
                builder.AddAttribute(84, "Style", "color: var(--rz-warning);");
                builder.CloseComponent();

                builder.OpenElement(85, "span");
                builder.AddAttribute(86, "style", "color: var(--rz-warning); font-weight: 500; font-size: 0.875rem;");
                builder.AddContent(87, "No personnel assigned to this role");
                builder.CloseElement();

                builder.CloseElement(); // No assignment section
            }

            builder.CloseElement(); // Card content
            builder.CloseElement(); // Card
        };
    }

    /// <summary>
    /// Gets authority level badge style using SMS color system instead of limited BadgeStyle
    /// Maps to 5 distinct colors from SMS color palette for better visual distinction
    /// </summary>
    private string GetAuthorityLevelCustomStyle(int authorityLevel)
    {
        return authorityLevel switch
        {
            >= 9 => "background-color: var(--sms-red-primary) !important; color: white !important;",    // Executive levels
            >= 7 => "background-color: var(--sms-blue-primary) !important; color: white !important;",  // Management levels  
            >= 5 => "background-color: var(--sms-green-primary) !important; color: white !important;",    // Operational levels
            _ => "background-color: var(--sms-blue-light) !important; color: var(--sms-blue-dark-bold) !important;"     // Other levels
        };
    }

    private BadgeStyle GetAuthorityLevelBadgeStyle(int authorityLevel)
    {
        // Keeping for backward compatibility, but preferring custom styles above
        return authorityLevel switch
        {
            >= 9 => BadgeStyle.Danger,    // Executive levels
            >= 7 => BadgeStyle.Primary,   // Management levels  
            >= 5 => BadgeStyle.Success,   // Operational levels
            _ => BadgeStyle.Secondary     // Other levels
        };
    }

    private string GetCoverageColor(double fillPercentage)
    {
        return fillPercentage >= 100 ? "var(--rz-success)" :
               fillPercentage >= 50 ? "var(--rz-warning)" :
               "var(--rz-danger)";
    }

    #endregion

    #region Domain-Aligned Helper Methods

    /// <summary>
    /// Gets category description directly from Domain enum - SINGLE SOURCE OF TRUTH
    /// Dynamically builds description from actual SMSOrganizationalLevel instances in each category
    /// </summary>
    private string GetCategoryDescription(string category)
    {
        // Get all levels in this category from the Domain enum
        var levelsInCategory = SMSOrganizationalLevel.GetLevelsByCategory(category).ToList();
        
        if (!levelsInCategory.Any())
        {
            return $"SMS organizational category: {category}";
        }

        // Get authority level range from actual domain data
        var minAuthority = levelsInCategory.Min(l => l.AuthorityLevel);
        var maxAuthority = levelsInCategory.Max(l => l.AuthorityLevel);
        var authorityRange = minAuthority == maxAuthority ? $"{minAuthority}" : $"{minAuthority}-{maxAuthority}";

        // Build description using domain helper properties
        var firstLevel = levelsInCategory.First();
        
        if (firstLevel.IsExecutiveRole)
        {
            return $"Strategic leadership roles with ultimate accountability for SMS performance and compliance. Authority levels {authorityRange}.";
        }
        else if (firstLevel.IsManagementRole)
        {
            return $"Operational oversight roles managing day-to-day SMS activities and coordination. Authority levels {authorityRange}.";
        }
        else if (firstLevel.IsOperationalRole)
        {
            return $"Subject matter expertise and operational SMS implementation roles. Authority levels {authorityRange}.";
        }
        else if (firstLevel.IsCommitteeRole)
        {
            return $"Collaborative decision-making and governance roles for SMS committees. Authority levels {authorityRange}.";
        }
        else if (firstLevel.IsExternalRole)
        {
            return $"External stakeholder representation and consultation roles. Authority levels {authorityRange}.";
        }
        
        // Fallback using the actual category from domain
        return $"SMS organizational roles in {category} category. Authority levels {authorityRange}.";
    }

    private List<CategorySummary> GetCategorySummary()
    {
        // Get ONLY categories that exist in the Domain enum - NO HARDCODING
        var categories = SMSOrganizationalLevel.GetAllValues()
            .GroupBy(level => level.Category)
            .Select(group => new CategorySummary
            {
                CategoryName = group.Key,
                Description = GetCategoryDescription(group.Key), // Uses Domain-driven method above
                TotalRoles = group.Count(),
                AssignedPersonnel = group.Sum(level => GetUsersForLevel(level).Count),
                FilledRoles = group.Count(level => GetUsersForLevel(level).Any()),
                AuthorityRange = $"{group.Min(l => l.AuthorityLevel)}-{group.Max(l => l.AuthorityLevel)}"
            })
            .OrderByDescending(cat => cat.TotalRoles > 0 ? cat.AssignedPersonnel / (double)cat.TotalRoles : 0)
            .ToList();

        return categories;
    }

    /// <summary>
    /// Gets category badge style using ONLY categories from Domain enum
    /// Maps to BadgeStyle based on category helper properties from SMSOrganizationalLevel
    /// MATCHES the colors shown in OrganizationalStructureGuidancePanel.razor
    /// </summary>
    private BadgeStyle GetCategoryBadgeStyle(string category)
    {
        // Get a representative level from this category from the Domain enum
        var levelInCategory = SMSOrganizationalLevel.GetLevelsByCategory(category).FirstOrDefault();
        
        if (levelInCategory is null)
        {
            return BadgeStyle.Light; // Fallback for unknown categories
        }

        // Use Domain enum helper properties - MATCHES GUIDANCE PANEL COLORS
        if (levelInCategory.IsExecutiveRole)
        {
            return BadgeStyle.Danger;    // Red for executive (matches guidance panel)
        }
        else if (levelInCategory.IsManagementRole)
        {
            return BadgeStyle.Primary;   // Blue for management (matches guidance panel)
        }
        else if (levelInCategory.IsOperationalRole)
        {
            return BadgeStyle.Success;   // Green for operational (matches guidance panel)
        }
        else if (levelInCategory.IsCommitteeRole)
        {
            return BadgeStyle.Info;      // Teal for collaborative
        }
        else if (levelInCategory.IsExternalRole)
        {
            return BadgeStyle.Secondary; // Gray for external
        }
        
        return BadgeStyle.Light; // Default fallback
    }

    #endregion

    #region Enhanced Models

    public class SMSOrganizationalLevelInfo
    {
        public SMSOrganizationalLevel Level { get; set; } = default!;
        public List<SMSOrganizationalUser> AssignedUsers { get; set; } = new();
        public List<SMSOrganizationalUser> EligibleUsers { get; set; } = new();
        public int HierarchyPosition { get; set; }
    }

    public class CategorySummary
    {
        public string CategoryName { get; set; } = "";
        public string Description { get; set; } = "";
        public int TotalRoles { get; set; }
        public int AssignedPersonnel { get; set; }
        public int FilledRoles { get; set; }
        public string AuthorityRange { get; set; } = "";
        public double FillPercentage => TotalRoles > 0 ? (double)FilledRoles / TotalRoles * 100 : 0;
    }

    public class UserAssignmentOption
    {
        public string Code { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Department { get; set; } = "";
        public string Position { get; set; } = "";
    }

    #endregion
}