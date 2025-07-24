namespace BenchmarkProject;
using System.Net.NetworkInformation;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using CBT3_Application.Configuration;
using CBT3_Application.Interfaces;
using CBT3_Application.Messaging;
using CBT3_Application.Services;

using CBT3_Domain.Common;

using CBT3_Domain.Entities;
using CBT3_Domain.Interfaces;

using CBT3_Infrastructure.Configuration;

using CBT3_Shared;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class ServiceProviderContainer
{
    public static void Initialize()
    {
        ServiceCollection services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
              .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
              .Build();

        services.AddSingleton<IConfiguration>(configuration);

        DependencyInjection.Initialize(services, configuration);


        ServiceProvider = services.BuildServiceProvider();

    }

    public static IServiceProvider ServiceProvider { get; private set; }


}
public static class DependencyInjection
{

    public static IServiceCollection Initialize(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedServices(configuration);
        services.AddInfrastructureServices(configuration);
        services.AddApplicationServices();


        return services;
    }

}
public class Program
{

    public static IMediator _mediator = ServiceProviderContainer.ServiceProvider.GetRequiredService<IMediator>();
    

    static void Main(string[] args)
    {
        ServiceProviderContainer.Initialize();
        _mediator = ServiceProviderContainer.ServiceProvider.GetRequiredService<IMediator>();
        BenchmarkRunner.Run<BenchMarking>();

        Console.WriteLine("Hello, World!");
    }
    //[MemoryDiagnoser]
    //[ShortRunJob]
    //public class BenchMarks
    //{
    //    public List<int>? _list = new();
    //    [Params(1_000, 10_000, 100_000, 1_000_000)]
    //    public int _ListSize; 

    //    [GlobalSetup]
    //    public void Setup()
    //    {
    //        for (int i = 0; i < _ListSize; i++)
    //        {
    //            _list.Add(i);
    //        }
    //    }
    //    [Benchmark]
    //    public void SortBenchmarkTest()
    //    {


    //        _list.Sort();

    //    }
    //    [Benchmark]
    //    public void SortTwoBenchmarkTest()
    //    {


    //        _list.Sort((x,y) => x.CompareTo(y));

    //    }
    //}




    [MemoryDiagnoser]
    [ShortRunJob]
    public class BenchMarking
    {
        //[Params(1_000, 10_000, 100_000, 1_000_000)]
        public int logcount;
        public TraineeID traineeId = new("13598968-5678-44f0-b7e4-76ea5ee4b552");

        public string LogMessageWithParameters = "Benchmark Log Message =>  {first} {second} ";

        private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole().AddFakeLogger().SetMinimumLevel(LogLevel.Warning);
            
        });

        private readonly ILogger<BenchMarking> _logger;
        

        public BenchMarking()
        {
            _logger = new Logger<BenchMarking>(_loggerFactory);
            

        }

        //[GlobalSetup]
        //public void Setup()
        //{
        //    logcount = 1;
        //}

        //[Benchmark]
        //public void Log_WithIf_WithParams()
        //{
        //    if (_logger.IsEnabled(LogLevel.Information))
        //    {
        //        for (int i = 0; i < logcount; i++)
        //        {
        //            _logger.LogInformation(LogMessageWithParameters, i, 420);
        //        }



        //    }
        //}
        //[Benchmark]
        //public void Log_WithoutIf_WithParams()
        //{

        //    for (int i = 0; i < logcount; i++)
        //    {
        //        _logger.LogInformation(LogMessageWithParameters, i, 420);
        //    }


        //}
        //[Benchmark]
        //public void LogAdapter_WithoutIf_WithParams()
        //{

        //    for (int i = 0; i < logcount; i++)
        //    {
        //        _loggerAdapter.LogInformation(LogMessageWithParameters, i, 420);
        //    }



        //}
        //[Benchmark]
        //public void LogMessageDef_WithoutIf_WithParams()
        //{
        //    for (int i = 0; i < logcount; i++)
        //    {
        //        _logger.BenchmarkLoggerMessage(69, 420);
        //    }




        //}
        //[Benchmark]
        //public void LogMessageDef_WithIf_WithParams()
        //{
        //    if (_logger.IsEnabled(LogLevel.Information))
        //    {
        //        for (int i = 0; i < logcount; i++)
        //        {
        //            _logger.BenchmarkLoggerMessage(i, 420);
        //        }
        //    }

        //}



        [Benchmark]
        public void GetCourses()
        {
            
            GetCoursesQuery coursesQuery = new GetCoursesQuery(traineeId);
            Result<List<Course>> courses_result = _mediator.SendAsync(coursesQuery, default).Result;
            //if (courses_result != null)
            //{
            //    foreach (Course c in courses_result.Value.ToList())
            //    {
            //        _logger.LogInformation(LogMessageWithParameters, c.CourseName, c.Id);
            //    }
            //}

        }
    }



}

//public static class LoggerMessageDefinitions
//{
//    private static readonly Action<ILogger, int, int, Exception?> BenchmarkLoggerMessageInformation =
//        LoggerMessage.Define<int, int>(LogLevel.Information, 0, "This is a  log message with parameters {first} {second} ");

//    public static void BenchmarkLoggerMessage(this ILogger logger, int arg0, int arg1)
//    {
//        BenchmarkLoggerMessageInformation(logger, arg0, arg1, null);
//    }
//}

public static class FakeLoggerExtensions
{
    public static ILoggingBuilder AddFakeLogger(this ILoggingBuilder loggingBuilder)
    {

        loggingBuilder.Services.AddSingleton<ILoggerProvider, FakeLoggerProvider>();
        return loggingBuilder;
    }
}

//public interface ILoggerAdapter<T>
//{
//    void LogInformation(string message);
//    void LogInformation<T0>(string message, T0 arg0);
//    void LogInformation<T0, T1>(string message, T0 arg0, T1 arg1);
//    void LogInformation<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2);
//}

//public class LoggerAdapter<T> : ILoggerAdapter<T>
//{
//    private readonly ILogger<T> _logger;
//    public LoggerAdapter(ILogger<T> logger)
//    {
//        _logger = logger;
//    }


//    public void LogInformation(string message)
//    {
//        if (_logger.IsEnabled(LogLevel.Information))
//        {
//            _logger.LogInformation(message);
//        }
//    }

//    public void LogInformation<T0>(string message, T0 arg0)
//    {
//        if (_logger.IsEnabled(LogLevel.Information))
//        {
//            _logger.LogInformation(message, arg0);
//        }
//    }

//    public void LogInformation<T0, T1>(string message, T0 arg0, T1 arg1)
//    {
//        if (_logger.IsEnabled(LogLevel.Information))
//        {
//            _logger.LogInformation(message, arg0, arg1);
//        }
//    }

//    public void LogInformation<T0, T1, T2>(string message, T0 arg0, T1 arg1, T2 arg2)
//    {
//        if (_logger.IsEnabled(LogLevel.Information))
//        {
//            _logger.LogInformation(message, arg0, arg1, arg2);
//        }
//    }
//}

public class FakeLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return new FakeDisposible();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {

    }
}

public class FakeDisposible : IDisposable
{
    public void Dispose()
    {

    }
}

public class FakeLoggerProvider : ILoggerProvider
{
    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FakeLogger();
    }
}