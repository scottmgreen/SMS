namespace CBT3_UnitTests.Application.Services;
using System.Threading.Tasks;

using CBT3_Application.Services;

using CBT3_Domain.Common;
using CBT3_Domain.Entities;
using CBT3_Domain.Errors;
using CBT3_Domain.ValueObjects;

using CBT3_Infrastructure.Interfaces;
using CBT3_Infrastructure.Repositories;
using CBT3_Infrastructure.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

public class RegistrationServiceTests
{
    IConfigurationRoot _configuration;
    CancellationTokenSource cts = new();
    CancellationToken ct;
    Mock<IServiceScopeFactory> _MockScopefactory = new Mock<IServiceScopeFactory>();
    Mock<ILogSupport> _MockLoggerSupport = new Mock<ILogSupport>();
    Mock<ILogger<TrainingDataService>> _MockServicelogger = new Mock<ILogger<TrainingDataService>>();
    Mock<ILogger<TrainingRepository>> _MockRepologger = new Mock<ILogger<TrainingRepository>>();

    // Constructor to set up common configuration
    public RegistrationServiceTests()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    }


    [Fact]
    public async Task SubmitFirstName_ValidFirstName_ReturnsSuccessResult()
    {
        // Arrange
                       
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        cts.Cancel();
        ct = cts.Token;
        
        
        var firstName = "Scott";

        // Act
        var result = await registrationService.SubmitFirstNameAsync(firstName,ct).ConfigureAwait(false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(firstName, result.Value);
    }
    [Fact]
    public async Task SubmitLastName_ValidLastLast_ReturnsSuccessResult()
    {
        // Arrange
        ct = cts.Token;
                
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);
        
        var registrationService = new RegistrationService(service);
        var lastName = "Green";

        // Act
        var result = await registrationService.SubmitLastNameAsync(lastName,ct).ConfigureAwait(false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(lastName, result.Value);
    }

    [Fact]
    public async Task SubmitFirstName_InvalidFirstName_ReturnsErrorResult()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        var firstName = "123";

        // Act
        var result = await registrationService.SubmitFirstNameAsync(firstName,ct).ConfigureAwait(false);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrors.FirstNameError.ContainsSpecialCharactersOrNumbers, result.Error);
    }
    [Fact]
    public async Task SubmitLastName_InvalidLastName_ReturnsErrorResult()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        var lastName = "!23";

        // Act
        var result = await registrationService.SubmitLastNameAsync(lastName,ct).ConfigureAwait(false);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrors.LastNameError.ContainsSpecialCharactersOrNumbers, result.Error);
    }
    [Fact]
    public async Task SubmitFirstUPID_InvalidUPID_ReturnsErrorResult()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        var firstUpid = "123";

        // Act
        var result = await registrationService.SubmitFirstUPIDAsync(firstUpid,ct).ConfigureAwait(false);

        // Assert
        Assert.False(result.IsSuccess);
        //Assert.Equal("Invalid first UPID", result.Error);
    }


    [Fact]
    public async Task SubmitFirstUPID_ValidUPID_ReturnsSuccessResult()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        var _firstUpid = "1234567";

        // Act
        var result = await registrationService.SubmitFirstUPIDAsync(_firstUpid,ct).ConfigureAwait(false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_firstUpid, result.Value);
    }

    [Fact]
    public async Task SubmitSecondUPID_InvalidUPID_ReturnsErrorResult()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        var _firstUpid = "1234567";
        var secondUpid = "123";

        // Act
        var result = await registrationService.SubmitSecondUPIDAsync(secondUpid, UPID.Create(_firstUpid).Value,ct).ConfigureAwait(false);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrors.UPIDError.RequiredLength, result.Error);
    }
    [Fact]
    public async Task SubmitSecondUPID_ValidUPID_ReturnsSuccessResult()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        var _firstUpid = "1234567";
        var secondUpid = "1234567";

        // Act
        var result1 = await registrationService.SubmitFirstUPIDAsync(_firstUpid, default);
        var result2 = await registrationService.SubmitSecondUPIDAsync(secondUpid, result1.Value, ct).ConfigureAwait(false);

        // Assert
        Assert.True(result2.IsSuccess);
        Assert.Equal(_firstUpid, result2.Value);
    }

    [Fact]
    public async Task SubmitFirstYearOfBirth_ValidYearOfBirth_ReturnsSuccessResult()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        var firstYearOfBirth = "1965";

        // Act
        var result = await registrationService.SubmitFirstYearOfBirthAsync(firstYearOfBirth,ct).ConfigureAwait(false);

        // Assert
        Assert.True(result.IsSuccess);

    }
    [Fact]
    public async Task SubmitFirstYearOfBirth_InvalidYearOfBirth_ReturnsErrorResult()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        var firstYearOfBirth = "1899";


        // Act
        var result = await registrationService.SubmitFirstYearOfBirthAsync(firstYearOfBirth,ct).ConfigureAwait(false);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrors.YearOfBirthError.OutOfRange, result.Error);
    }
    [Fact]
    public async Task SubmitSecondYearOfBirth_ValidYearOfBirth_ReturnsSuccessResult()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        var firstYearOfBirth = "1965";
        var secondYearOfBirth = "1965";

        // Act
        var result1 = await registrationService.SubmitFirstYearOfBirthAsync(firstYearOfBirth, ct).ConfigureAwait(false);
        var result2 = await registrationService.SubmitSecondYearOfBirthAsync(firstYearOfBirth, result1.Value, ct).ConfigureAwait(false);

        // Assert
        Assert.True(result2.IsSuccess);
        Assert.Equal(firstYearOfBirth, result2.Value);
    }
    [Fact]
    public async Task SubmitSecondYearOfBirth_InvalidYearOfBirth_ReturnsErrorResult()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        var firstYearOfBirth = "1965";
        var secondYearOfBirth = "1962";


        // Act
        var result = await registrationService.SubmitSecondYearOfBirthAsync(firstYearOfBirth, YearOfBirth.Create(secondYearOfBirth).Value,ct).ConfigureAwait(false);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrors.YearOfBirthError.MismatchYearOfBirth, result.Error);
    }
    
    [Fact]
    public void GetTrainee_AllStepsCompleted_ReturnsTraineeObject()
    {
        // Arrange
        ct = cts.Token;
        
        var repo = new TrainingRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
        var service = new TrainingDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

        var registrationService = new RegistrationService(service);
        TraineeID traineeId = new(Guid.NewGuid().ToString());

        Result<FirstName> firstname = registrationService.SubmitFirstNameAsync("Scott",ct).Result;
        Result<LastName> lastname = registrationService.SubmitLastNameAsync("Green",ct).Result;
        Result<UPID> upid1 = registrationService.SubmitFirstUPIDAsync("1234567",ct).Result;
        Result<UPID> upid2 = registrationService.SubmitSecondUPIDAsync("1234567", upid1.Value, ct).Result;
        Result<YearOfBirth> yob1 = registrationService.SubmitFirstYearOfBirthAsync("1990",ct).Result;
        Result<YearOfBirth> yob2 = registrationService.SubmitSecondYearOfBirthAsync("1990", yob1.Value,ct).Result;

        


        Trainee trainee = new(traineeId);
        trainee.FirstName = firstname.Value;
        trainee.LastName = lastname.Value;
        trainee.UPID = upid2.Value;
        trainee.YearOfBirth = yob2.Value;
        trainee.CreatedDate = DateTime.Now;
        trainee.SubscribeToEmailNewsletter = SubscribeToEmailNewsletter.Create(false).Value;
        trainee.SubscribeToTextNewsletter = SubscribeToTextNewsletter.Create(false).Value;
        trainee.SubscribeToOperationalTexts = SubscribeToOperationalTexts.Create(false).Value;

        Result<Trainee> addedTrainee = registrationService.AddTraineeAsync(trainee, ct).Result;

        // Act
        var traineeget = registrationService.GetTraineeByIdAsync((TraineeID)addedTrainee.Value.Id).Result;
       
        // Assert
        Assert.NotNull(traineeget);
        Assert.Equal("SCOTT", traineeget.Value.FirstName);
        Assert.Equal("GREEN", traineeget.Value.LastName);
        Assert.Equal("1234567", traineeget.Value.UPID);
        Assert.Equal("1990", traineeget.Value.YearOfBirth);
        Assert.Equal<TraineeID>((TraineeID)addedTrainee.Value.Id,(TraineeID) traineeget.Value.Id);
    }
}

