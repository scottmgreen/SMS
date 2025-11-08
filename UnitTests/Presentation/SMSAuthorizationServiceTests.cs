using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using SMS_Application.Services;
using SMS_Domain.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Common;
using SMS_Domain.Errors;
using SMS_Domain.Enums;
using SMS_Shared.Common;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Presentation;

/// <summary>
/// Unit tests for SMSAuthorizationService (Application Services version)
/// Tests core authorization functionality with repository integration
/// </summary>
public class SMSAuthorizationServiceTests : ApplicationTestBase
{
    private readonly Mock<ISMSApplicationUserRepository> _mockAppUserRepo;
    private readonly Mock<ISMSOrganizationalUserRepository> _mockOrgUserRepo;
    private readonly Mock<ISMSStakeholderUserRepository> _mockStakeholderRepo;
    private readonly Mock<ISMSRoleService> _mockRoleService;
    private readonly SMSAuthorizationService _authService;

    public SMSAuthorizationServiceTests()
    {
        _mockAppUserRepo = new Mock<ISMSApplicationUserRepository>();
        _mockOrgUserRepo = new Mock<ISMSOrganizationalUserRepository>();
        _mockStakeholderRepo = new Mock<ISMSStakeholderUserRepository>();
        _mockRoleService = new Mock<ISMSRoleService>();
        
        _authService = new SMSAuthorizationService(
            _mockAppUserRepo.Object,
            _mockOrgUserRepo.Object,
            _mockStakeholderRepo.Object,
            _mockRoleService.Object
        );
    }

    protected override void RegisterServices(IServiceCollection services)
    {
        // Register mocks for DI
        services.AddSingleton(_mockAppUserRepo.Object);
        services.AddSingleton(_mockOrgUserRepo.Object);
        services.AddSingleton(_mockStakeholderRepo.Object);
        services.AddSingleton(_mockRoleService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullApplicationUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SMSAuthorizationService(null!, _mockOrgUserRepo.Object, _mockStakeholderRepo.Object, _mockRoleService.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("applicationUserRepository");
    }

    [Fact]
    public void Constructor_WithNullOrganizationalUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SMSAuthorizationService(_mockAppUserRepo.Object, null!, _mockStakeholderRepo.Object, _mockRoleService.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("organizationalUserRepository");
    }

    [Fact]
    public void Constructor_WithNullStakeholderUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SMSAuthorizationService(_mockAppUserRepo.Object, _mockOrgUserRepo.Object, null!, _mockRoleService.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("stakeholderUserRepository");
    }

    [Fact]
    public void Constructor_WithNullRoleService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new SMSAuthorizationService(_mockAppUserRepo.Object, _mockOrgUserRepo.Object, _mockStakeholderRepo.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("roleService");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _authService.Should().NotBeNull();
        _authService.Should().BeOfType<SMSAuthorizationService>();
    }

    #endregion

    #region CanUserPerformActionAsync Tests

    [Fact]
    public async Task CanUserPerformActionAsync_WithApplicationUser_ShouldReturnTrue()
    {
        // Arrange
        var userId = "APP-001";
        var action = "MANAGE_USERS";
        var appUser = CreateTestApplicationUser(userId);
        
        _mockAppUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result<SMSApplicationUser>.Success(appUser));

        // Act
        var result = await _authService.CanUserPerformActionAsync(userId, action);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockAppUserRepo.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CanUserPerformActionAsync_WithOrganizationalUser_ShouldCallRepository()
    {
        // Arrange
        var userId = "ORG-001";
        var action = "SUBMIT_HAZARD_REPORTS";
        var orgUser = CreateTestOrganizationalUser(userId);
        
        _mockAppUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound));
        
        _mockOrgUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result<SMSOrganizationalUser>.Success(orgUser));

        // Act
        var result = await _authService.CanUserPerformActionAsync(userId, action);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockOrgUserRepo.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CanUserPerformActionAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = "INVALID-001";
        var action = "ANY_ACTION";
        
        _mockAppUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound));
        
        _mockOrgUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound));
        
        _mockStakeholderRepo.Setup(x => x.GetByIdAsync(userId))
                           .ReturnsAsync(Result.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound));

        // Act
        var result = await _authService.CanUserPerformActionAsync(userId, action);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region CanUserAccessAreaAsync Tests

    [Fact]
    public async Task CanUserAccessAreaAsync_WithApplicationUser_ShouldReturnTrue()
    {
        // Arrange
        var userId = "APP-001";
        var area = "USER_MANAGEMENT";
        var appUser = CreateTestApplicationUser(userId);
        
        _mockAppUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result<SMSApplicationUser>.Success(appUser));

        // Act
        var result = await _authService.CanUserAccessAreaAsync(userId, area);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserAccessAreaAsync_WithOrganizationalUser_ShouldReturnResult()
    {
        // Arrange
        var userId = "ORG-001";
        var area = "COMMITTEES";
        var orgUser = CreateTestOrganizationalUser(userId);
        
        _mockAppUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound));
        
        _mockOrgUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result<SMSOrganizationalUser>.Success(orgUser));

        // Act
        var result = await _authService.CanUserAccessAreaAsync(userId, area);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockOrgUserRepo.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    #endregion

    #region Role Authority Tests

    [Fact]
    public async Task HasUserRoleAuthorityAsync_WithSufficientAuthority_ShouldReturnTrue()
    {
        // Arrange
        var userId = "ORG-001";
        var requiredLevel = 5;
        
        _mockRoleService.Setup(x => x.GetUserMaxAuthorityLevelAsync(userId))
                       .ReturnsAsync(7);

        // Act
        var result = await _authService.HasUserRoleAuthorityAsync(userId, requiredLevel);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasUserRoleAuthorityAsync_WithInsufficientAuthority_ShouldReturnFalse()
    {
        // Arrange
        var userId = "ORG-001";
        var requiredLevel = 10;
        
        _mockRoleService.Setup(x => x.GetUserMaxAuthorityLevelAsync(userId))
                       .ReturnsAsync(5);

        // Act
        var result = await _authService.HasUserRoleAuthorityAsync(userId, requiredLevel);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion

    #region User Management Tests

    [Fact]
    public async Task CanUserManageUsersAsync_WithApplicationUser_ShouldReturnTrue()
    {
        // Arrange
        var userId = "APP-001";
        var targetUserType = "Organizational";
        var appUser = CreateTestApplicationUser(userId);
        
        _mockAppUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result<SMSApplicationUser>.Success(appUser));

        // Act
        var result = await _authService.CanUserManageUsersAsync(userId, targetUserType);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserManageUsersAsync_WithOrganizationalUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = "ORG-001";
        var targetUserType = "Stakeholder";
        var orgUser = CreateTestOrganizationalUser(userId);
        
        _mockAppUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound));
        
        _mockOrgUserRepo.Setup(x => x.GetByIdAsync(userId))
                       .ReturnsAsync(Result<SMSOrganizationalUser>.Success(orgUser));

        // Act
        var result = await _authService.CanUserManageUsersAsync(userId, targetUserType);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse(); // Only app users can manage users
    }

    #endregion

    #region Stakeholder Access Tests

    [Fact]
    public async Task CanStakeholderAccessOperationalDataAsync_WithValidStakeholder_ShouldReturnResult()
    {
        // Arrange
        var userId = "STK-001";
        var dataScope = "AIRSIDE_DATA";
        var stakeholderUser = CreateTestStakeholderUser(userId);
        
        _mockStakeholderRepo.Setup(x => x.GetByIdAsync(userId))
                           .ReturnsAsync(Result<SMSStakeholderUser>.Success(stakeholderUser));

        // Act
        var result = await _authService.CanStakeholderAccessOperationalDataAsync(userId, dataScope);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CanStakeholderSubmitReportsAsync_WithValidStakeholder_ShouldReturnResult()
    {
        // Arrange
        var userId = "STK-001";
        var reportType = "HAZARD_REPORT";
        var stakeholderUser = CreateTestStakeholderUser(userId);
        
        _mockStakeholderRepo.Setup(x => x.GetByIdAsync(userId))
                           .ReturnsAsync(Result<SMSStakeholderUser>.Success(stakeholderUser));

        // Act
        var result = await _authService.CanStakeholderSubmitReportsAsync(userId, reportType);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static SMSApplicationUser CreateTestApplicationUser(string userId)
    {
        var mockAppUser = new Mock<SMSApplicationUser>();
        var baseUserId = new BaseUserID(userId);
        var firstName = FirstName.Create("Test").Value;
        var lastName = LastName.Create("AppUser").Value;
        var userName = UserName.Create("test.appuser@test.com").Value;
        
        mockAppUser.SetupGet(x => x.UserId).Returns(baseUserId);
        mockAppUser.SetupGet(x => x.FirstName).Returns(firstName);
        mockAppUser.SetupGet(x => x.LastName).Returns(lastName);
        mockAppUser.SetupGet(x => x.UserName).Returns(userName);
        mockAppUser.SetupGet(x => x.IsActive).Returns(true);
        mockAppUser.SetupGet(x => x.DisplayName).Returns($"{firstName.Value} {lastName.Value}");
        mockAppUser.Setup(x => x.HasPermission(It.IsAny<string>())).Returns(true);
        mockAppUser.Setup(x => x.CanPerform(It.IsAny<string>())).Returns(true);
        mockAppUser.Setup(x => x.CanAccess(It.IsAny<string>())).Returns(true);
        
        return mockAppUser.Object;
    }

    private static SMSOrganizationalUser CreateTestOrganizationalUser(string userId)
    {
        var mockOrgUser = new Mock<SMSOrganizationalUser>();
        var baseUserId = new BaseUserID(userId);
        var firstName = FirstName.Create("Test").Value;
        var lastName = LastName.Create("OrgUser").Value;
        var userName = UserName.Create("test.orguser@test.com").Value;
        
        mockOrgUser.SetupGet(x => x.UserId).Returns(baseUserId);
        mockOrgUser.SetupGet(x => x.FirstName).Returns(firstName);
        mockOrgUser.SetupGet(x => x.LastName).Returns(lastName);
        mockOrgUser.SetupGet(x => x.UserName).Returns(userName);
        mockOrgUser.SetupGet(x => x.IsActive).Returns(true);
        mockOrgUser.SetupGet(x => x.DisplayName).Returns($"{firstName.Value} {lastName.Value}");
        mockOrgUser.Setup(x => x.HasPermission(It.IsAny<string>())).Returns(true);
        
        return mockOrgUser.Object;
    }

    private static SMSStakeholderUser CreateTestStakeholderUser(string userId)
    {
        var mockStakeholderUser = new Mock<SMSStakeholderUser>();
        var baseUserId = new BaseUserID(userId);
        var firstName = FirstName.Create("Test").Value;
        var lastName = LastName.Create("Stakeholder").Value;
        var userName = UserName.Create("test.stakeholder@test.com").Value;
        
        mockStakeholderUser.SetupGet(x => x.UserId).Returns(baseUserId);
        mockStakeholderUser.SetupGet(x => x.FirstName).Returns(firstName);
        mockStakeholderUser.SetupGet(x => x.LastName).Returns(lastName);
        mockStakeholderUser.SetupGet(x => x.UserName).Returns(userName);
        mockStakeholderUser.SetupGet(x => x.IsActive).Returns(true);
        mockStakeholderUser.SetupGet(x => x.DisplayName).Returns($"{firstName.Value} {lastName.Value}");
        mockStakeholderUser.SetupGet(x => x.StakeholderType).Returns("Airline");
        mockStakeholderUser.SetupGet(x => x.Organization).Returns("Test Airlines");
        mockStakeholderUser.SetupGet(x => x.AccessLevel).Returns("Standard");
        mockStakeholderUser.Setup(x => x.HasPermission(It.IsAny<string>())).Returns(true);
        mockStakeholderUser.Setup(x => x.CanAccessData(It.IsAny<string>())).Returns(true);
        mockStakeholderUser.Setup(x => x.CanParticipate(It.IsAny<string>())).Returns(true);
        
        return mockStakeholderUser.Object;
    }

    #endregion
}