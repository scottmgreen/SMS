using SMS_Domain.Interfaces;
using System.Reflection;

namespace SMS_Application.EventHandlers;

public static class HandlerHelpers
{
    public static string ResolveReportId<TEvent>(TEvent domainEvent) where TEvent : IBaseDomainEvent
    {
        return ResolveReportId((IBaseDomainEvent)domainEvent);
    }

    public static string ResolveReportId(IBaseDomainEvent domainEvent)
    {
        if (domainEvent is null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(domainEvent.ReportId))
        {
            return domainEvent.ReportId.Trim();
        }

        var reportCodeProperty = domainEvent.GetType().GetProperty("ReportCode", BindingFlags.Public | BindingFlags.Instance);
        if (reportCodeProperty?.PropertyType == typeof(string))
        {
            var reportCode = reportCodeProperty.GetValue(domainEvent) as string;
            if (!string.IsNullOrWhiteSpace(reportCode))
            {
                return reportCode.Trim();
            }
        }

        return string.Empty;
    }
}
