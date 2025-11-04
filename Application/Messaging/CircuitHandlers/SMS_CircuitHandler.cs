using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SMS_Domain.Entities;

using SMS_Infrastructure.Configuration;
using SMS_Infrastructure.Interfaces;

using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CircuitHandlers;


public class SMS_CircuitHandler : BaseCircuitHandler
{
    //private ILogger<SMS_CircuitHandler> _logger;
    private new readonly ILogger<SMS_CircuitHandler> _logger;
    public SMS_CircuitHandler(ILogger<SMS_CircuitHandler> logger, ILogSupport logsupport, IMediator mediator) : base(logger, logsupport, mediator)
    {
        _logger = logger;
    }

    protected override Task HandleConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(circuit.Id))
            {

                if (circuit.Id != null)
                {
                    //TrainingStation trainingstation = new();
                    //trainingstation.HostName = base.HostName;
                    //trainingstation.CircuitId = circuit.Id;
                    //trainingstation.Status = SMS_Domain.Enums.TrainingStationStatus.MachineStopped;

                    //UpdateTrainingStationCommand request = new UpdateTrainingStationCommand(trainingstation);
                    //_ = Task.Run(() => Mediator.SendAsync(request, default)).Result;
                }

            }
            _logger.LogInformation($"[CBT_UI_CircuitHandler] Client Disconnected: CircuitId: {circuit.Id}, ConnectionId: {ConnectionId}, IP: {IPAddress}, Host: {HostName}");
            return Task.CompletedTask;
        }
        catch (OperationCanceledException)
        {
            _logger.LogError($"[CBT_UI_CircuitHandler] Client Disconnected Error: CircuitId: {circuit.Id}, ConnectionId: {ConnectionId}, IP: {IPAddress}, Host: {HostName}");
            throw;
        }
    }

    protected override Task HandleConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(circuit.Id))
            {
                if (circuit.Id != null)
                {
                    //TrainingStation trainingstation = new();
                    //trainingstation.HostName = base.HostName;
                    //trainingstation.CircuitId = circuit.Id;
                    //trainingstation.Status = SMS_Domain.Enums.TrainingStationStatus.MachineRunning;

                    //UpdateTrainingStationCommand request = new UpdateTrainingStationCommand(trainingstation);
                    //_ = Task.Run(() => Mediator.SendAsync(request, default)).Result;
                }

            }

            _logger.LogInformation($"[CBT_UI_CircuitHandler] Client Connected: CircuitId: {circuit.Id}, ConnectionId: {ConnectionId}, IP: {IPAddress}, Host: {HostName}");
            return Task.CompletedTask;
        }
        catch (OperationCanceledException)
        {

            _logger.LogError($"[CBT_UI_CircuitHandler] Client Connected Error: CircuitId: {circuit.Id}, ConnectionId: {ConnectionId}, IP: {IPAddress}, Host: {HostName}");
            throw;
        }
    }
}
