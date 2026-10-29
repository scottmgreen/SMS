using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GenericComposeDialogBase : EmailComposeDialogBase
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

        ToAddressesText = string.Join("; ", Model.To ?? []);
        CcAddressesText = string.Join("; ", Model.Cc ?? []);
        BccAddressesText = string.Join("; ", Model.Bcc ?? []);
        FromAddress = CurrentUserService?.Email?.Trim() ?? string.Empty;
        _fieldsInitialized = true;
    }

    protected async Task HandleSendAsync()
    {
        Model.To = ParseRecipients(ToAddressesText);
        Model.Cc = ParseRecipients(CcAddressesText);
        Model.Bcc = ParseRecipients(BccAddressesText);

        if (!string.IsNullOrWhiteSpace(FromAddress))
        {
            var normalizedFrom = FromAddress.Trim();
            if (!Model.Cc.Contains(normalizedFrom, StringComparer.OrdinalIgnoreCase))
            {
                Model.Cc.Add(normalizedFrom);
            }
        }

        await SendAsync();
    }

    private static List<string> ParseRecipients(string? recipients)
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
