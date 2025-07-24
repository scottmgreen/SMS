namespace CBT3_Infrastructure.Interfaces;

public interface ILogSupport
{
    string GenerateLogHeader();
    string GenerateLogHeaderWithTimestamp();
}
