using CBT3_Domain.Entities;
using CBT3_Domain.Errors;
using CBT3_Domain.Interfaces;

using CBT3_Infrastructure.Services;

namespace CBT3_Application.Messaging.CommandHandlers;


    public class AddTraineeCommandHandler : BaseCommandBundle,IRequestHandler<AddTraineeCommand, Result<Trainee>>
    {
        private readonly RegistrationService _registrationService;
        private Trainee _trainee;

        public AddTraineeCommandHandler(RegistrationService registrationService)
        {
            _registrationService = registrationService;
        }
        
        public async Task<Result<Trainee>> HandleAsync(AddTraineeCommand request, CancellationToken ct = default)
        {
            _trainee = request.Trainee;
            request.Trainee.CreatedDate = DateTime.UtcNow;
            request.Trainee.CreatedBy = "SYSTEM";
            Result<Trainee> traineeResult = await _registrationService.AddTraineeAsync(request.Trainee,ct).ConfigureAwait(false);

            if (traineeResult.IsSuccess)
            {
                return traineeResult;
            }
            else
            {
                return Result<Trainee>.Failure<Trainee>(traineeResult.Error);

            }
        
        }

    public override string ToString()
    {
        return _trainee.ToString();
    }
}

