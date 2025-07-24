//using CBT3_Domain.Events.DomainEvents;
using CBT3_Infrastructure.Services;

using CBT3_Domain.Common;
using CBT3_Domain.Entities;
using CBT3_Domain.ValueObjects;

namespace CBT3_Application.Services;


public  class RegistrationService
{
    private FirstName? _firstName;
    private LastName? _lastName;
    private UPID? _upid;
    private YearOfBirth? _yearOfBirth;
    private SubscribeToEmailNewsletter? _subscribeToEmailNewsletter;
    private SubscribeToTextNewsletter? _subscribeToTextNewsletter;
    private SubscribeToOperationalTexts? _subscribeToOperationalTexts;

    private bool _FirstNameCompleted;
    private bool _LastNameCompleted;
    private bool _UPIDCompleted;
    private bool _YOBCompleted;

    private readonly TrainingDataService _trainingDataService;
    public RegistrationService(TrainingDataService trainingDataService)
    {
        _trainingDataService = trainingDataService;
    }
    public async Task<Result<FirstName>> SubmitFirstNameAsync(string firstName, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Validate first and last name
        Result<FirstName> result = FirstName.Create(firstName);
        // Simulate async operation
        await Task.Delay(0).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            this._firstName = result.Value;
            _FirstNameCompleted = true;
            return Result<FirstName>.Success(result.Value);
        }
        else
        {
            return Result<FirstName>.Failure< FirstName>(result.Error);
        }


        //_NameCompleted = true;
        //return Result.Success(true);
    }

    public async Task<Result<LastName>> SubmitLastNameAsync(string lastName, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Validate last name
        Result<LastName> result = LastName.Create(lastName);
        await Task.Delay(0).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            this._lastName = result.Value;
            _LastNameCompleted = true;
            return Result<LastName>.Success(result.Value);
        }
        else
        {
            return Result<LastName>.Failure<LastName>(result.Error);
        }
    }

    public async Task<Result<UPID>> SubmitFirstUPIDAsync(string upid, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Validate last name
        Result<UPID> result = UPID.Create(upid);
        await Task.Delay(0).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            this._upid = result.Value;
            _UPIDCompleted = true;
            return Result<UPID>.Success(result.Value);
        }
        else
        {
            return Result<UPID>.Failure<UPID>(result.Error);
        }

    }
    public async Task<Result<UPID>> SubmitSecondUPIDAsync(string upid, UPID originalupid, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Validate last name
        Result<UPID> result = UPID.Create(upid, originalupid);
        await Task.Delay(0).ConfigureAwait(false);
        if (result.IsSuccess && _UPIDCompleted)
        {
            this._upid = result.Value;
            return Result<UPID>.Success(result.Value);
        }
        else
        {
            return Result<UPID>.Failure<UPID>(result.Error);
        }

    }

    public async Task<Result<YearOfBirth>> SubmitFirstYearOfBirthAsync(string yearofbirth, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Validate last name
        Result<YearOfBirth> result = YearOfBirth.Create(yearofbirth);
        await Task.Delay(0).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            this._yearOfBirth = result.Value;
            _YOBCompleted = true;
            return Result<YearOfBirth>.Success(result.Value);
        }
        else
        {
            return Result<YearOfBirth>.Failure<YearOfBirth>(result.Error);
        }

    }
    public async Task<Result<YearOfBirth>> SubmitSecondYearOfBirthAsync(string secondyearofbirth, YearOfBirth originalyearofbirth, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Validate last name
        Result<YearOfBirth> result = YearOfBirth.Create(secondyearofbirth, originalyearofbirth);
        await Task.Delay(0).ConfigureAwait(false);
        if (result.IsSuccess && _YOBCompleted)
        {
            this._yearOfBirth = result.Value;
            return Result<YearOfBirth>.Success(result.Value);
        }
        else
        {
            return Result<YearOfBirth>.Failure<YearOfBirth>(result.Error);
        }

    }

    public async Task<Result<SubscribeToEmailNewsletter>> SubmitSubscribeToEmailNewsletterAsync(bool emailnotificationpref, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Validate last name
        Result<SubscribeToEmailNewsletter> result = SubscribeToEmailNewsletter.Create(emailnotificationpref);
        await Task.Delay(0).ConfigureAwait(false);
        if (result.IsSuccess && _subscribeToEmailNewsletter)
        {
            this._subscribeToEmailNewsletter = result.Value;
            return Result<SubscribeToEmailNewsletter>.Success(result.Value);
        }
        else
        {
            return Result<SubscribeToEmailNewsletter>.Failure<SubscribeToEmailNewsletter>(result.Error);
        }

    }
    public async Task<Result<SubscribeToTextNewsletter>> SubmitSubscribeToTextNewsletterAsync(bool smsnotificationpref, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Validate last name
        Result<SubscribeToTextNewsletter> result = SubscribeToTextNewsletter.Create(smsnotificationpref);
        await Task.Delay(0).ConfigureAwait(false);
        if (result.IsSuccess && _subscribeToTextNewsletter)
        {
            this._subscribeToTextNewsletter = result.Value;
            return Result<SubscribeToTextNewsletter>.Success(result.Value);
        }
        else
        {
            return Result<SubscribeToTextNewsletter>.Failure<SubscribeToTextNewsletter>(result.Error);
        }

    }
    public async Task<Result<SubscribeToOperationalTexts>> SubmitSubscribeToOperationalTextsAsync(bool smsnotificationpref, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Validate last name
        Result<SubscribeToOperationalTexts> result = SubscribeToOperationalTexts.Create(smsnotificationpref);
        await Task.Delay(0).ConfigureAwait(false);
        if (result.IsSuccess && _subscribeToOperationalTexts)
        {
            this._subscribeToOperationalTexts = result.Value;
            return Result<SubscribeToOperationalTexts>.Success(result.Value);
        }
        else
        {
            return Result<SubscribeToOperationalTexts>.Failure<SubscribeToOperationalTexts>(result.Error);
        }

    }


    public Task<Result<Trainee>> AddTraineeAsync(Trainee trainee, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return _trainingDataService.AddTraineeAsync(trainee, token);
    }

    public Task<Result<Trainee>> GetTraineeByIdAsync(TraineeID traineeID,CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return _trainingDataService.GetTraineeByIdAsync(traineeID, token);
    }
    
}









