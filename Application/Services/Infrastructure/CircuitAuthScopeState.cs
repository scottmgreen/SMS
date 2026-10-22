namespace SMS_Application.Services;

public interface ICircuitAuthScopeState
{
    string? AuthStorageKey { get; set; }
}

public sealed class CircuitAuthScopeState : ICircuitAuthScopeState
{
    public string? AuthStorageKey { get; set; }
}
