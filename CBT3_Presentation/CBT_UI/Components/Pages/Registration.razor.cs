using CBT_UI.Components.Pages.Shared;

using CBT3_Application.Interfaces;
using CBT3_Application.Messaging;
using CBT3_Application.Messaging.Commands;
using CBT3_Application.Services;

using CBT3_Domain.Common;
using CBT3_Domain.Entities;
using CBT3_Domain.Errors;
using CBT3_Domain.Interfaces;
using CBT3_Domain.ValueObjects;

using CBT3_UI;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Radzen;
using Radzen.Blazor;

using static CBT_UI.Components.Pages.Registration;
namespace CBT_UI.Components.Pages;

public partial class Registration : ComponentBase
{
    public class TraineeDTO
    {
        // FirstName property
        public string FirstName { get; set; }

        // LastName property
        public string LastName { get; set; }

        // UPID property
        public string UPID { get; set; }

        // YearOfBirth property
        public string YearOfBirth { get; set; }

        public bool SubscribeToEmailNewsletter { get; set; }

        public bool SubscribeToTextNewsletter { get; set; }

        public bool SubscribeToOperationalTexts { get; set; }
    }

    [CascadingParameter]
    public CascadingAppState AppState { get; set; }

    [Inject]
    IJSRuntime _jsRuntime { get; set; }
    
    [Inject]
    IMediator _mediator { get; set; }

    bool popup = true;
    private int _selectedIndex = 0;
    private bool _savedNameSuccess;
    private bool _savedUPIDSuccess;
    private bool _savedUPIDRepeatSuccess;
    private bool _savedYOBSuccess;
    private bool _savedYOBRepeatSuccess;
    private bool _savedSubscriptionsSuccess;
    //private bool? _savedSMSNotificationSuccess;

    private bool isNameKeyboardVisible = true;
    private bool isUPIDKeyboardVisible = false;
    private bool isYOBKeyboardVisible = false;
    private string keyboardStyle => isNameKeyboardVisible ? "" : "display: none;";
    private string upidkeyboardStyle => isUPIDKeyboardVisible ? "" : "display: none;";
    private string yobkeyboardStyle => isYOBKeyboardVisible ? "" : "display: none;";


    private string pressedNameKeys = "";
    private bool firstNameFocus = true;
    private bool lastNameFocus = false;

    private string pressedNumKeys = "";
    private bool upidFocus = false;
    private bool upidRepeatFocus = false;

    private bool yobFocus = false;
    private bool yobRepeatFocus = false;

    private bool notificationsFocus = false;


    private RadzenTextBox firstNameInput;
    private RadzenTextBox lastNameInput;
    private RadzenTextBox upidInput;
    private RadzenTextBox upidRepeatInput;

    private RadzenTextBox yobInput;
    private RadzenTextBox yobRepeatInput;

    
    public static Trainee? Trainee { get; set; } = null;
    public static Course? Course { get; set; } = null;
    public static bool CoursePass { get; set; }

    TraineeDTO _traineeDTO;
    FirstName _firstName = null;
    LastName _lastName = null;
    UPID _firstupid = null;
    UPID _secondupid = null;
    YearOfBirth _firstyearofbirth = null;
    YearOfBirth _secondyearofbirth = null;
    SubscribeToEmailNewsletter _subscribeToEmailNewsletter = null;
    SubscribeToTextNewsletter _subscribeToTextNewsletter = null;
    SubscribeToOperationalTexts _subscribeToOperationalTexts = null;

    private Radzen.ConfirmOptions _confirmOptions = new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No", ShowClose = false };
    private Radzen.DialogOptions _diagOptions = new Radzen.DialogOptions() { Width = "800px", Height = "900px", Resizable = false, Draggable = true, ShowClose = false };

    MarkupString Message;
    String _errorMsgs;

    private string _registationStyle = "";
    

    private void OnChange()
    {

    }
    

    public async Task<Result<Trainee>> AddTraineeAsync(Trainee _trainee)
    {
        try
        {
            AddTraineeCommand newtrainee_cmd = new AddTraineeCommand { Trainee = _trainee };

            var result = await _mediator.SendAsync(newtrainee_cmd, default);
            return result;
        }
        catch (Exception ex)
        {
            // Handle exceptions here
            Console.WriteLine($"An error occurred while adding trainee: {ex.Message}");
            return Result.Failure<Trainee>(DomainErrors.TraineeError.NullOrEmpty);
        }
    }

    private async Task<Result<LastName>> ValidateLastNameAsync(string lastname)
    {
        try
        {
            SubmitLastNameCommand cmd = new SubmitLastNameCommand
            {
                LastName = lastname
            };

            var result = await _mediator.SendAsync(cmd, default);

            if (result.IsSuccess)
            {
                _lastName = result.Value;
            }
            else
            {
                _errorMsgs = result.Error.Message;
                return Result<LastName>.Failure<LastName>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _errorMsgs = ex.Message;
        }

        if (_lastName is not null)
            return Result<LastName>.Success(_lastName);
        else //this should never happen
            return Result<LastName>.Failure<LastName>(DomainErrors.LastNameError.NullOrEmpty);
    }

    private async Task<Result<FirstName>> ValidateFirstNameAsync(string firstname)
    {
        try
        {
            SubmitFirstNameCommand cmd = new SubmitFirstNameCommand
            {
                FirstName = firstname
            };

            var result = await _mediator.SendAsync(cmd, default);

            if (result.IsSuccess)
            {
                _firstName = result.Value;
            }
            else
            {
                _errorMsgs = result.Error.Message;
                return Result<FirstName>.Failure<FirstName>(result.Error);
                
            }
        }
        catch (Exception ex)
        {
            _errorMsgs = ex.Message;
            //await ShowAlert(_errorMsgs);
        }

        if (_firstName is not null)
            return Result<FirstName>.Success(_firstName);
        else //this should never happen
            return Result<FirstName>.Failure<FirstName>(DomainErrors.FirstNameError.NullOrEmpty);

    }

    private async Task<Result<YearOfBirth>> ValidateFirstYearOfBirthAsync(string firstyearofbirth)
    {
        _errorMsgs = string.Empty;
        try
        {
            SubmitFirstYearOfBirthCommand cmd = new SubmitFirstYearOfBirthCommand
            {
                FirstYearOfBirth = firstyearofbirth
            };

            var result = await _mediator.SendAsync(cmd, default);

            if (result.IsSuccess)
            {

                _firstyearofbirth = result.Value;
                return Result<YearOfBirth>.Success(_firstyearofbirth);
            }
            else
            {
                _traineeDTO.YearOfBirth = "";
                pressedNumKeys = "";
                _errorMsgs = result.Error.Message;
                return Result<YearOfBirth>.Failure<YearOfBirth>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _errorMsgs = ex.Message;
        }

        if (_firstyearofbirth is not null)
            return Result<YearOfBirth>.Success(_firstyearofbirth);
        else //this should never happen
            return Result<YearOfBirth>.Failure<YearOfBirth>(DomainErrors.YearOfBirthError.NullOrEmpty);

    }

    private async Task<Result<YearOfBirth>> ValidateSecondYearOfBirthAsync(string secondyearofbirth)
    {
        _errorMsgs = string.Empty;
        try
        {
            SubmitSecondYearOfBirthCommand cmd = new SubmitSecondYearOfBirthCommand
            {
                FirstYearOfBirth = _firstyearofbirth,
                SecondYearOfBirth = secondyearofbirth
            };

            var result = await _mediator.SendAsync(cmd, default);

            if (result.IsSuccess)
            {
                _secondyearofbirth = result.Value;
                _traineeDTO.YearOfBirth = "";
                pressedNumKeys = "";
                return Result<YearOfBirth>.Success(_secondyearofbirth);
            }
            else
            {
                _traineeDTO.YearOfBirth = "";
                pressedNumKeys = "";
                _errorMsgs = result.Error.Message;
                return Result<YearOfBirth>.Failure<YearOfBirth>(result.Error);

            }
        }
        catch (Exception ex)
        {
            _errorMsgs = ex.Message;
        }

        if (_secondyearofbirth is not null)
            return Result<YearOfBirth>.Success(_secondyearofbirth);
        else //this should never happen
            return Result<YearOfBirth>.Failure<YearOfBirth>(DomainErrors.UPIDError.NullOrEmpty);


    }

    private async Task<Result<UPID>> ValidateFirstUPIDAsync(string firstupid)
    {
        try
        {
            SubmitFirstUPIDCommand cmd = new SubmitFirstUPIDCommand
            {
                FirstUPID = firstupid
            };

            var result = await _mediator.SendAsync(cmd, default);

            if (result.IsSuccess)
            {

                _firstupid = result.Value;
                return Result<UPID>.Success(_firstupid);
            }
            else
            {
                _traineeDTO.UPID = "";
                pressedNumKeys = "";
                _errorMsgs = result.Error.Message;
                //await InvokeAsync(StateHasChanged);
                return Result<UPID>.Failure<UPID>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _errorMsgs = ex.Message;
            return Result<UPID>.Failure<UPID>(DomainErrors.UPIDError.NullOrEmpty);
        }

        //if (_firstupid is not null)
        //    return Result<UPID>.Success(_firstupid);
        //else //this should never happen
        //    return Result<UPID>.Failure<UPID>(DomainErrors.UPIDError.NullOrEmpty);
    }

    private async Task<Result<UPID>> ValidateSecondUPIDAsync(string secondupid)
    {
        
        try
        {
            SubmitSecondUPIDCommand cmd = new SubmitSecondUPIDCommand
            {
                FirstUPID = _firstupid,
                SecondUPID = secondupid
            };

            var result = await _mediator.SendAsync(cmd, default);

            if (result.IsSuccess)
            {
                _secondupid = result.Value;
                pressedNumKeys = "";
                return Result<UPID>.Success(_secondupid);
            }
            else
            {
                _traineeDTO.UPID = "";
                pressedNumKeys = "";
                _errorMsgs = result.Error.Message;
               return Result<UPID>.Failure<UPID>(result.Error);

            }
        }
        catch (Exception ex)
        {
            _errorMsgs = ex.Message;
            return Result<UPID>.Failure<UPID>(DomainErrors.UPIDError.NullOrEmpty);
        }

        //if (_secondupid is not null)
        //    return Result<UPID>.Success(_secondupid);
        //else //this should never happen
        //    return Result<UPID>.Failure<UPID>(DomainErrors.UPIDError.NullOrEmpty);


    }

    private async Task<Result<SubscribeToEmailNewsletter>> ValidateSubscribeToEmailNewsletterAsync(SubscribeToEmailNewsletter emailpref)
    {

        try
        {
            SubmitSubscribeToEmailNewsletterCommand cmd = new SubmitSubscribeToEmailNewsletterCommand
            {
                SubscribeToEmailNewsletterOption = _subscribeToEmailNewsletter,
            };

            var result = await _mediator.SendAsync(cmd, default);

            if (result.IsSuccess)
            {
                _subscribeToEmailNewsletter = result.Value;
                pressedNumKeys = "";
                return Result<SubscribeToEmailNewsletter>.Success(_subscribeToEmailNewsletter);
            }
            else
            {
                _traineeDTO.UPID = "";
                pressedNumKeys = "";
                _errorMsgs = result.Error.Message;
                return Result<SubscribeToEmailNewsletter>.Failure<SubscribeToEmailNewsletter>(result.Error);

            }
        }
        catch (Exception ex)
        {
            _errorMsgs = ex.Message;
            return Result<SubscribeToEmailNewsletter>.Failure<SubscribeToEmailNewsletter>(DomainErrors.SubscribeToEmailNewsletterError.NullOrEmpty);
        }

        //if (_secondupid is not null)
        //    return Result<UPID>.Success(_secondupid);
        //else //this should never happen
        //    return Result<UPID>.Failure<UPID>(DomainErrors.UPIDError.NullOrEmpty);


    }

    private async Task<Result<SubscribeToTextNewsletter>> ValidateSubscribeToTextNewsletterAsync(SubscribeToTextNewsletter smspref)
    {

        try
        {
            SubmitSubscribeToTextNewsletterCommand cmd = new SubmitSubscribeToTextNewsletterCommand
            {
                SubscribeToTextNewsletterOption = _subscribeToTextNewsletter,
            };

            var result = await _mediator.SendAsync(cmd, default);

            if (result.IsSuccess)
            {
                _subscribeToTextNewsletter = result.Value;
                pressedNumKeys = "";
                return Result<SubscribeToTextNewsletter>.Success(_subscribeToTextNewsletter);
            }
            else
            {
                _traineeDTO.UPID = "";
                pressedNumKeys = "";
                _errorMsgs = result.Error.Message;
                return Result<SubscribeToTextNewsletter>.Failure<SubscribeToTextNewsletter>(result.Error);

            }
        }
        catch (Exception ex)
        {
            _errorMsgs = ex.Message;
            return Result<SubscribeToTextNewsletter>.Failure<SubscribeToTextNewsletter>(DomainErrors.SubscribeToTextNewsletterError.NullOrEmpty);
        }

        //if (_secondupid is not null)
        //    return Result<UPID>.Success(_secondupid);
        //else //this should never happen
        //    return Result<UPID>.Failure<UPID>(DomainErrors.UPIDError.NullOrEmpty);


    }

    private async Task<Result<SubscribeToOperationalTexts>> ValidateSubscribeToOperationalTextsAsync(SubscribeToOperationalTexts smspref)
    {

        try
        {
            SubmitSubscribeToOperationalTextsCommand cmd = new SubmitSubscribeToOperationalTextsCommand
            {
                SubscribeToOperationalTextsOption = _subscribeToOperationalTexts,
            };

            var result = await _mediator.SendAsync(cmd, default);

            if (result.IsSuccess)
            {
                _subscribeToOperationalTexts = result.Value;
                pressedNumKeys = "";
                return Result<SubscribeToOperationalTexts>.Success(_subscribeToOperationalTexts);
            }
            else
            {
                _traineeDTO.UPID = "";
                pressedNumKeys = "";
                _errorMsgs = result.Error.Message;
                return Result<SubscribeToOperationalTexts>.Failure<SubscribeToOperationalTexts>(result.Error);

            }
        }
        catch (Exception ex)
        {
            _errorMsgs = ex.Message;
            return Result<SubscribeToOperationalTexts>.Failure<SubscribeToOperationalTexts>(DomainErrors.SubscribeToOperationalTextsError.NullOrEmpty);
        }

        //if (_secondupid is not null)
        //    return Result<UPID>.Success(_secondupid);
        //else //this should never happen
        //    return Result<UPID>.Failure<UPID>(DomainErrors.UPIDError.NullOrEmpty);


    }

    void OnInvalidSubmit(FormInvalidSubmitEventArgs args)
    {

    }

        
    private async Task ShowAlert(string message)
    {
        await _dialogService.Alert(message);
    }    
    

    private void BackspacePressed()
    {
        if (firstNameFocus)
        {
            if (_traineeDTO.FirstName.Length > 0)
            {
                pressedNameKeys = pressedNameKeys.Substring(0, pressedNameKeys.Length - 1);
                _traineeDTO.FirstName = _traineeDTO.FirstName.Substring(0, _traineeDTO.FirstName.Length - 1);
            }
        }
        if (lastNameFocus)
        {
            if (_traineeDTO.LastName.Length > 0)
            {
                pressedNameKeys = pressedNameKeys.Substring(0, pressedNameKeys.Length - 1);
                _traineeDTO.LastName = _traineeDTO.LastName.Substring(0, _traineeDTO.LastName.Length - 1);
            }
        }
        if (upidFocus)
        {
            if (_traineeDTO.UPID.Length > 0)
            {
                pressedNumKeys = pressedNumKeys.Substring(0, pressedNumKeys.Length - 1);
                _traineeDTO.UPID = _traineeDTO.UPID.Substring(0, _traineeDTO.UPID.Length - 1);
            }
        }
        if (upidRepeatFocus)
        {
            if (_traineeDTO.UPID.Length > 0)
            {
                pressedNumKeys = pressedNumKeys.Substring(0, pressedNumKeys.Length - 1);
                _traineeDTO.UPID = _traineeDTO.UPID.Substring(0, _traineeDTO.UPID.Length - 1);
            }
        }
        if (yobFocus)
        {
            if (_traineeDTO.YearOfBirth.Length > 0)
            {
                pressedNumKeys = pressedNumKeys.Substring(0, pressedNumKeys.Length - 1);
                _traineeDTO.YearOfBirth = _traineeDTO.YearOfBirth.Substring(0, _traineeDTO.YearOfBirth.Length - 1);
            }
        }
        if (yobRepeatFocus)
        {
            if (_traineeDTO.YearOfBirth.Length > 0)
            {
                pressedNumKeys = pressedNumKeys.Substring(0, pressedNumKeys.Length - 1);
                _traineeDTO.YearOfBirth = _traineeDTO.YearOfBirth.Substring(0, _traineeDTO.YearOfBirth.Length - 1);
            }
        }
    }
    private void NameKeyPressed(string key)
    {

        if (key == "TAB")
        {
            // Toggle focus between FirstName and LastName
            if (firstNameFocus)
            {
                pressedNameKeys = "";
                _traineeDTO.LastName = string.Empty;
                lastNameFocus = true;
                lastNameInput.FocusAsync();
                firstNameFocus = false;
            }
            else
            {
                pressedNameKeys = "";
                _traineeDTO.FirstName = string.Empty;
                lastNameFocus = false;
                firstNameFocus = true;
                firstNameInput.FocusAsync();
            }
        }
        else
        {
            pressedNameKeys += key;
            if (firstNameFocus)
            {
                _traineeDTO.FirstName = pressedNameKeys;
            }
            else
            {
                _traineeDTO.LastName = pressedNameKeys;
            }


        }
    }
    private void TabPressed(string e)
    {
        if (e == "Tab")
        {
            if (firstNameFocus)
            {
                lastNameFocus = true;
                lastNameInput.FocusAsync();
                firstNameFocus = false;
            }
            else
            {
                firstNameFocus = true;
                firstNameInput.FocusAsync();
                lastNameFocus = false;
            }
        }
    }
    private void NumKeyPressed(string key)
    {

        if (upidFocus || upidRepeatFocus)
        {
            if (pressedNumKeys.Length < 7)
            {
                pressedNumKeys += key;
            }
        }

        if (yobFocus || yobRepeatFocus)
        {
            if (pressedNumKeys.Length < 4)
            {
                pressedNumKeys += key;
            }
        }

        if (upidFocus)
        {
            _traineeDTO.UPID = pressedNumKeys;
        }
        if (upidRepeatFocus)
        {
            _traineeDTO.UPID = pressedNumKeys;
        }
        if (yobFocus)
        {
            _traineeDTO.YearOfBirth = pressedNumKeys;
        }
        if (yobRepeatFocus)
        {
            _traineeDTO.YearOfBirth = pressedNumKeys;
        }


    }
    
    private async Task CanChange(StepsCanChangeEventArgs args)
    {
        if (args.SelectedIndex == 0 && _savedNameSuccess)
        {
            return;
        }

        if (args.SelectedIndex == 1 && _savedUPIDSuccess)
        {
            return;
        }
        if (args.SelectedIndex == 2 && _savedUPIDRepeatSuccess)
        {
            return;
        }
        if (args.SelectedIndex == 3 && _savedYOBSuccess)
        {
            return;
        }
        if (args.SelectedIndex == 4 && _savedYOBRepeatSuccess)
        {
            return;
        }
        if (args.SelectedIndex == 5 && _savedSubscriptionsSuccess)
        {
            return;
        }
        var response = await _dialogService.Alert(
            "Are you sure you want to contine without saving?",
            "Confirm",
            new ConfirmOptions()
            {
                CloseDialogOnEsc = false,
                ShowClose = false,
                CancelButtonText = "No",
                OkButtonText = "Yes",
            });

        if (response == false)
        {
            args.PreventDefault();
        }
    }

    private async void SubmitUPID()
    {
        if (upidFocus)
        {
            _savedUPIDSuccess = _firstupid is not null ? true : _savedUPIDSuccess;

            upidFocus = false;
            upidRepeatFocus = true;
            pressedNumKeys = "";
            _traineeDTO.UPID = "";
            _selectedIndex += 1;
            Message = (MarkupString)"<h2><strong>Please re-enter your 7-digit unique ID number (UPID).</h2></strong>";
            await PlayAudio("AUD_LOGIN_REENTER_UPID_7.mp3");


        }
        else
        {
            _savedUPIDRepeatSuccess = _secondupid is not null ? true : _savedUPIDRepeatSuccess;
            var secondResult = UPID.Create(_secondupid.Value, _firstupid);
            if (secondResult.IsFailure)
            {
                await ShowAlert(secondResult.Error);  // Show validation error
                return;
            }
            _secondupid = secondResult.Value;
            _savedUPIDRepeatSuccess = true;

            _cbtApp.Trainee.UPID = _secondupid;
            upidFocus = false;
            upidRepeatFocus = false;
            yobFocus = true;
            pressedNumKeys = "";
            isUPIDKeyboardVisible = !isUPIDKeyboardVisible;
            isYOBKeyboardVisible = true;
            _selectedIndex += 1;
            Message = (MarkupString)"<h2><strong>Please enter the year in which you were born.</h2></strong>";
            await PlayAudio("AUD_LOGIN_ENTER_BIRTH_YEAR.mp3");
            
            //if (_savedUPIDRepeatSuccess)
            //{
            //    _cbtApp.Trainee.UPID = _secondupid;
            //    upidFocus = false;
            //    upidRepeatFocus = false;
            //    yobFocus = true;
            //    pressedNumKeys = "";
            //    isUPIDKeyboardVisible = !isUPIDKeyboardVisible;
            //    isYOBKeyboardVisible = true;
            //    _selectedIndex += 1;
            //    Message = (MarkupString)"<h2><strong>Please enter the year in which you were born.</h2></strong>";
            //    await PlayAudio("AUD_LOGIN_ENTER_BIRTH_YEAR.mp3");

            //}
            //else
            //{
            //    await ShowAlert("UPIDS MUST MATCH!");


            //}

        }
    }





    private async void SubmitYOB()
    {
        if (yobFocus)
        {
            _savedYOBSuccess = _firstyearofbirth is not null ? true : _savedYOBSuccess;
            yobFocus = false;
            yobRepeatFocus = true;
            pressedNumKeys = "";
            _traineeDTO.YearOfBirth = "";
            _selectedIndex += 1;
            Message = (MarkupString)"<h2><strong>Please re-enter the year in which you were born.</h2></strong>";
            await PlayAudio("AUD_LOGIN_REENTER_BIRTH_YEAR.mp3");


        }
        else if (yobRepeatFocus)
        {
            _savedYOBRepeatSuccess = _secondyearofbirth is not null ? true : _savedYOBRepeatSuccess;

            var secondResult = YearOfBirth.Create(_secondyearofbirth.Value, _firstyearofbirth);

            if (secondResult.IsFailure)
            {
                await ShowAlert(secondResult.Error);  // Show validation error
                return;
            }

            _secondyearofbirth = secondResult.Value;
            _savedYOBRepeatSuccess = true;

            _cbtApp.Trainee.YearOfBirth = _secondyearofbirth;
            notificationsFocus = true;
            _selectedIndex += 1;
            Message = (MarkupString)"<h2><strong>Employee Communications Preferences.</h2></strong>";
        }


    }


    private async void SubmitName()
    {
        var firstnameOk = ValidateFirstNameAsync(_traineeDTO.FirstName).Result;

        if (firstnameOk.IsSuccess)
        {
            _cbtApp.Trainee.FirstName = _firstName;
        }
        var lastnameOk = ValidateLastNameAsync(_traineeDTO.LastName).Result;
        if (lastnameOk.IsSuccess)
        {
            _cbtApp.Trainee.LastName = _lastName;
        }

        if (firstnameOk.IsSuccess && lastnameOk.IsSuccess)
        {
            _savedNameSuccess = true;
            isNameKeyboardVisible = !isNameKeyboardVisible;
            isUPIDKeyboardVisible = !isUPIDKeyboardVisible;
            _selectedIndex += 1;
            upidFocus = true;
            Message = (MarkupString)"<h2><strong>Please enter your 7-digit unique ID number (UPID).</h2></strong>";
            await PlayAudio("AUD_LOGIN_ENTER_UPID_7.mp3");
        }


    }
    
    private async void SubmitNotificationChoices()
    {
       
            if (notificationsFocus)
            {
                _cbtApp.Trainee.SubscribeToEmailNewsletter = SubscribeToEmailNewsletter.Create(_traineeDTO.SubscribeToEmailNewsletter).Value;
                _cbtApp.Trainee.SubscribeToTextNewsletter = SubscribeToTextNewsletter.Create(_traineeDTO.SubscribeToTextNewsletter).Value;
                _cbtApp.Trainee.SubscribeToOperationalTexts = SubscribeToOperationalTexts.Create(_traineeDTO.SubscribeToOperationalTexts).Value;
                
                _registationStyle = "display:none:";

                 await ShowTraineeRegistrationConfirmation(_cbtApp.Trainee);
            }

    }
    
   
    private async Task InitializeTraineeRegistration()
    {
        _cbtApp.Trainee = null;
        _selectedIndex = 0;
        TraineeID traineeId = new(Guid.NewGuid().ToString());
        _cbtApp.Trainee = new Trainee(traineeId);
        _traineeDTO = new();
        isNameKeyboardVisible = true;
        isUPIDKeyboardVisible = false;
        isYOBKeyboardVisible = false;
        firstNameFocus = true;
        pressedNumKeys = "";
        pressedNameKeys = "";
        Message = (MarkupString)"<h2><strong>Type in your First Name and touch TAB to move to the next field.<br /> You must enter a First and Last name.<br /> Touch ENTER when finished.</h2></strong>";
        await InvokeAsync(StateHasChanged);
        await Task.Run(() => PlayAudio("AUD_LOGIN_ENTER_FULL_NAME.mp3"));
    }

    private async Task ShowNameConfirmation()
    {
        string dialogBodyText = $@"
        <div class='info-grid' style='font-size: 25px;font-weight:400;'>
            <div class='label'>First Name:</div>    
            <div class='data'><b>{_traineeDTO.FirstName}</b></div>
            <div class='label'>Last Name:</div>    
            <div class='data'><b>{_traineeDTO.LastName}</b></div>
        </div>";
        MarkupString dialogBody = new MarkupString(dialogBodyText);

        string dialogQuestion = $@"
        <div style='font-size:25px;font-weight:400;text-align: center;display: flex; justify-content: center; align-items: center; width: 100%;'>
          <b>  Is this Correct? </b>
        </div>";
        
       
        var dialog = await _dialogService.Confirm(dialogQuestion, dialogBodyText.ToString(), new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No", ShowClose = false });
        
        if (dialog.Value)
        {
            SubmitName();
        }
        else
        {
            AppState.SetProperty(this, "Trainee", null);
            AppState.SetProperty(this, "Course", null);
            AppState.SetProperty(this, "LessonPage", null);
            AppState.SetProperty(this, "Session", string.Empty);
            AppState.SetProperty(this, "CoursePass", false);
            _navManager.NavigateTo("/instructionpage");

        }
    }

    private async Task ShowTraineeRegistrationConfirmation(Trainee trainee)
    {
        string Checkmark(bool? value) => value == true ? "✅" : "⬜";
        var subEmailNewsletter = Checkmark(trainee.SubscribeToEmailNewsletter);
        var subTextNewsletter = Checkmark(trainee.SubscribeToTextNewsletter);
        var subOperationalNewsletter = Checkmark(trainee.SubscribeToOperationalTexts);

        string dialogBodyText = $@"
        <div class='info-grid' style='font-size: 25px;font-weight:400;'>
            <div class='label'>First Name:</div>    
            <div class='data'><b>{trainee.FirstName}</b></div>
            <div class='label'>Last Name:</div>    
            <div class='data'><b>{trainee.LastName}</b></div>
            <div class='label'>UPID:</div>    
            <div class='data'><b>{trainee.UPID}</b></div>
            <div class='label'>Year of Birth:</div>    
            <div class='data'><b>{trainee.YearOfBirth}</b></div>

            <div class='label'>Subscribe to Email Newsletter:</div>    
            <div class='data'><b>{subEmailNewsletter}</b></div>

            <div class='label'>Subscribe to Text Newsletter:</div>    
            <div class='data'><b>{subTextNewsletter}</b></div>          

            <div class='label'>Subscribe to Operational Texts:</div>    
            <div class='data'><b>{subOperationalNewsletter}</b></div>
        </div>";
        MarkupString dialogBody = new MarkupString(dialogBodyText);
        string dialogQuestion = $@"
        <div style='font-size:25px;font-weight:400;text-align: center;display: flex; justify-content: center; align-items: center; width: 100%;'>
          <b>  Is this Correct? </b>
        </div>";

                
        var dialog = await _dialogService.Confirm(dialogQuestion, dialogBody.ToString(), new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" , ShowClose = false });
        
        if (dialog.Value)
        {
            var result = await AddTraineeAsync(trainee);
            trainee = result.Value;
            UpdateToolBarMessage(trainee);
            _navManager.NavigateTo($"/courseselection/{trainee.Id.Value}");
        }
        else
        {
             _navManager.NavigateTo("/instructionpage");
        }
    }

    
      

    private async Task PlayAudio(string audiofile)
    {
        await _jsRuntime.InvokeVoidAsync("playAudio", "./audio/" + audiofile);
        await Task.Delay(7000);
        
    }


    private void UpdateToolBarMessage(Trainee trainee)
    {

        AppState.SetProperty(this, "Trainee", trainee);
    }
}
