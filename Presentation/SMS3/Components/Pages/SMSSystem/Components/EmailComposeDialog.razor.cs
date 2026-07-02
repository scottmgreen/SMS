using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using SMS3.Components.Pages.SMSSystem.Models;


public class EmailComposeDialogBase : ComponentBase
{
    [Inject] protected NotificationService NotificationService { get; set; } = default!;
    [Inject] protected DialogService DialogService { get; set; } = default!;
    [Inject] protected IEmailSender EmailSender { get; set; } = default!;

    [Parameter] public EmailComposeModel? InitialModel { get; set; }
    [Parameter] public bool PreviewOnly { get; set; }
    [Parameter] public string? DialogTitleOverride { get; set; }

    protected EmailComposeModel Model { get; private set; } = new();
    protected bool ShowCc { get; set; }
    protected bool ShowBcc { get; set; }
    protected bool IsSending { get; set; }
    protected string DialogTitle { get; set; } = "New message";

    




    protected override void OnInitialized()
    {
        // Seed model if provided (e.g., prefilled recipients)
        if (InitialModel != null)
        {
            Model = new EmailComposeModel
            {
                To = InitialModel.To?.Distinct().ToList() ?? new(),
                Cc = InitialModel.Cc?.Distinct().ToList() ?? new(),
                Bcc = InitialModel.Bcc?.Distinct().ToList() ?? new(),
                Subject = InitialModel.Subject ?? string.Empty,
                BodyHtml = InitialModel.BodyHtml ?? string.Empty
            };

            ShowCc = (Model.Cc?.Count ?? 0) > 0;
            ShowBcc = (Model.Bcc?.Count ?? 0) > 0;

            if (!string.IsNullOrWhiteSpace(Model.Subject)
                && Model.Subject.Contains("Mitigation Approval Required", StringComparison.OrdinalIgnoreCase))
            {
                DialogTitle = "Mitigation Approval Request";
            }
        }

        if (!string.IsNullOrWhiteSpace(DialogTitleOverride))
        {
            DialogTitle = DialogTitleOverride;
        }
    }

    protected void ToggleCc() => ShowCc = !ShowCc;
    protected void ToggleBcc() => ShowBcc = !ShowBcc;

    protected void OnRecipientsChanged(object? _ = null)
    {
        // Normalize separators (comma, semicolon, spaces) that Chips may accept as text additions
        Model.To = NormalizeList(Model.To);
        Model.Cc = NormalizeList(Model.Cc);
        Model.Bcc = NormalizeList(Model.Bcc);
    }

    private static System.Collections.Generic.List<string> NormalizeList(System.Collections.Generic.IEnumerable<string>? source)
    {
        var result = new System.Collections.Generic.List<string>();
        if (source == null) return result;

        foreach (var token in source)
        {
            var parts = token
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p));

            foreach (var p in parts)
            {
                if (!result.Contains(p, StringComparer.OrdinalIgnoreCase))
                {
                    result.Add(p);
                }
            }
        }
        return result;
    }

    protected async Task SendAsync()
    {
        if (IsSending) return;

        // Basic validation to keep things Outlook-simple
        var errors = ValidateModel(Model);
        if (errors.Any())
        {
            NotificationService.Notify(new Radzen.NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Cannot send",
                Detail = string.Join(Environment.NewLine, errors)
            });
            return;
        }

        try
        {
            IsSending = true;

            // Build the lightweight mail request (backend-independent)
            var request = new MailRequest
            {
                To = Model.To,
                Cc = Model.Cc,
                Bcc = Model.Bcc,
                Subject = Model.Subject?.Trim() ?? string.Empty,
                BodyHtml = Model.BodyHtml ?? string.Empty
            };

            await EmailSender.SendAsync(request);

            NotificationService.Notify(new Radzen.NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Message sent",
                Detail = "Your email has been sent."
            });

            DialogService.Close(true);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new Radzen.NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Send failed",
                Detail = ex.Message
            });
        }
        finally
        {
            IsSending = false;
        }
    }

    protected void Close()
    {
        DialogService.Close(false);
    }

    private static System.Collections.Generic.List<string> ValidateModel(EmailComposeModel m)
    {
        var errors = new System.Collections.Generic.List<string>();

        if (m.To == null || m.To.Count == 0)
            errors.Add("At least one recipient (To) is required.");

        // Validate email syntax using .NET's MailAddress (for most cases)
        void ValidateList(System.Collections.Generic.IEnumerable<string>? list, string label)
        {
            if (list == null) return;
            foreach (var addr in list)
            {
                var trimmed = addr.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                try { _ = new MailAddress(trimmed); }
                catch { errors.Add($"{label} contains an invalid email: {trimmed}"); }
            }
        }

        ValidateList(m.To, "To");
        ValidateList(m.Cc, "Cc");
        ValidateList(m.Bcc, "Bcc");

        // Optional: subject not strictly required (Outlook allows empty subject with a prompt; we’ll allow)
        // Optional: bodyHtml can be empty

        return errors;
    }
}

