using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CBT3_Domain.Interfaces;



namespace CBT3_Infrastructure.Interfaces;
public  interface IDashboard
{
    Task<Result<List<TrainingStation>>> GetTrainingStationsAsync(CancellationToken ct = default);
    Task<Result<TrainingStation>> GetTrainingStationByHostNameAsync(string hostName, CancellationToken ct = default);
    Task<Result<TrainingStation>> UpdateTrainingStationAsync(ITrainingStation trainingmachine, CancellationToken ct = default);
    
    Task<Result<TrainingSession>> UpdateTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default);
    Task<Result<TrainingSession>> StartTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default);
    Task<Result<TrainingSession>> FinishTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default);

    //Task<bool> UpdateTrainingStatusAsync(TrainingStatus trainingStatus, CancellationToken cancellationToken = default);
    //Task<bool> UpdateTrainingStationStatusAsync(TrainingStationStatus machineStatus, CancellationToken cancellationToken = default);
}
