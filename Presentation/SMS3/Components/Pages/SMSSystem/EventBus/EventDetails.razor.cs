using Microsoft.AspNetCore.Components;
using System.Text.Json;
using SMS_Domain.ValueObjects;
namespace SMS3.Components.Pages.SMSSystem.EventBus;

public partial class EventDetails
{
    private readonly List<(string Key, string Value)> _summaryFields = new();
    private readonly List<string> _toRecipients = new();
    private readonly List<string> _ccRecipients = new();
    private readonly List<string> _bccRecipients = new();
    private readonly List<(string Key, string Value)> _emailMetadata = new();

    private bool _isHtmlContent;
    private string _emailBody = string.Empty;
    private string _formattedEventData = string.Empty;

    [Parameter] public QueuedEvent Event { get; set; } = null!;

    protected override void OnParametersSet()
    {
        BuildEventDataView();
    }

    private void BuildEventDataView()
    {
        _summaryFields.Clear();
        _toRecipients.Clear();
        _ccRecipients.Clear();
        _bccRecipients.Clear();
        _emailMetadata.Clear();
        _isHtmlContent = false;
        _emailBody = string.Empty;
        _formattedEventData = Event.EventData ?? string.Empty;

        if (string.IsNullOrWhiteSpace(Event.EventData))
        {
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(Event.EventData);
            _formattedEventData = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });

            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            AddSummaryIfPresent(doc.RootElement, "EventId");
            AddSummaryIfPresent(doc.RootElement, "OccurredOn");
            AddSummaryIfPresent(doc.RootElement, "EventType");
            AddSummaryIfPresent(doc.RootElement, "ReportId");
            AddSummaryIfPresent(doc.RootElement, "TargetSystem");
            AddSummaryIfPresent(doc.RootElement, "RelatedEntityId");
            AddSummaryIfPresent(doc.RootElement, "RelatedEntityType");
            AddSummaryIfPresent(doc.RootElement, "WorkflowType");
            AddSummaryIfPresent(doc.RootElement, "Subject");

            _toRecipients.AddRange(ReadStringArray(doc.RootElement, "ToRecipients"));
            _ccRecipients.AddRange(ReadStringArray(doc.RootElement, "CcRecipients"));
            _bccRecipients.AddRange(ReadStringArray(doc.RootElement, "BccRecipients"));

            _isHtmlContent = ReadBool(doc.RootElement, "IsHtmlContent");
            _emailBody = ReadString(doc.RootElement, "Body");

            if (doc.RootElement.TryGetProperty("EmailMetadata", out var metadata)
                && metadata.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in metadata.EnumerateObject())
                {
                    var value = property.Value.ValueKind == JsonValueKind.String
                        ? property.Value.GetString() ?? string.Empty
                        : property.Value.ToString();

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        _emailMetadata.Add((property.Name, value));
                    }
                }
            }
        }
        catch
        {
            // keep raw event data when payload cannot be parsed
        }
    }

    private void AddSummaryIfPresent(JsonElement root, string key)
    {
        var value = ReadString(root, key);
        if (!string.IsNullOrWhiteSpace(value))
        {
            _summaryFields.Add((key, value));
        }
    }

    private static string ReadString(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var value))
        {
            return value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : value.ToString();
        }

        return string.Empty;
    }

    private static bool ReadBool(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var value))
        {
            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String when bool.TryParse(value.GetString(), out var parsed) => parsed,
                _ => false
            };
        }

        return false;
    }

    private static List<string> ReadStringArray(JsonElement root, string propertyName)
    {
        var values = new List<string>();
        if (!root.TryGetProperty(propertyName, out var value)
            || value.ValueKind != JsonValueKind.Array)
        {
            return values;
        }

        foreach (var item in value.EnumerateArray())
        {
            var text = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                values.Add(text.Trim());
            }
        }

        return values;
    }
}


