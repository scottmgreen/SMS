using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GenericEmailComposeBase : IntegratedEmailComposeBase
{
    protected string FromAddress { get; set; } = string.Empty;
    protected string ToAddressesText { get; set; } = string.Empty;
    protected string CcAddressesText { get; set; } = string.Empty;
    protected string BccAddressesText { get; set; } = string.Empty;

    private bool _fieldsInitialized;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (_fieldsInitialized)
        {
            return;
        }

        Model.To = [];
        Model.Cc = [];
        Model.Bcc = [];

        ToAddressesText = string.Empty;
        CcAddressesText = string.Empty;
        BccAddressesText = string.Empty;
        FromAddress = GetConfiguredFromAddress();
        _fieldsInitialized = true;
    }

    protected async Task HandleSendAsync()
    {
        Model.To = ParseRecipients(ToAddressesText);
        Model.Cc = ParseRecipients(CcAddressesText);
        Model.Bcc = ParseRecipients(BccAddressesText);
        Model.AllowDirectToRecipients = true;
        Model.HasToValidationError = false;
        Model.ToValidationMessage = null;

        await SendAsync();
    }

    protected static List<string> ParseRecipients(string? recipients)
    {
        if (string.IsNullOrWhiteSpace(recipients))
        {
            return [];
        }

        return recipients
            .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(entry => entry.Trim())
            .Where(entry => !string.IsNullOrWhiteSpace(entry))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
