//-----------------------------------------------------------------------
// <copyright file="SMSEmailTemplateBuilder.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Net;
using System.Text;

namespace SMS_Application.Common;

public sealed class SMSEmailField
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool IsFullWidth { get; init; }
    public bool ValueIsHtml { get; init; }
}

public sealed class SMSEmailSection
{
    public string Title { get; init; } = string.Empty;
    public IReadOnlyList<SMSEmailField> Fields { get; init; } = [];
}

public static class SMSEmailTemplateBuilder
{
    public const string DefaultLogoUrl = "";

    public static string BuildStandardEmail(
        string title,
        string introHtml,
        IReadOnlyList<SMSEmailField>? summaryFields = null,
        IReadOnlyList<SMSEmailSection>? sections = null,
        string? footerHtml = null,
        string? logoUrl = null)
    {
        var safeTitle = WebUtility.HtmlEncode(title ?? string.Empty);
        var resolvedLogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? DefaultLogoUrl : logoUrl;
        var includeLogo = ShouldRenderLogo(resolvedLogoUrl);
        var safeLogoUrl = WebUtility.HtmlEncode(resolvedLogoUrl);

        var sb = new StringBuilder();
        sb.Append("<html><body style='margin:0;padding:0;background:#f7f7f7;font-family:Arial,sans-serif;color:#111;'>");
        sb.Append("<table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background:#f7f7f7;padding:24px 0;'><tr><td align='center'>");
        sb.Append("<table role='presentation' width='720' cellspacing='0' cellpadding='0' style='max-width:720px;background:#ffffff;border:1px solid #e6e6e6;'>");
        sb.Append("<tr><td style='background:#111111;padding:16px 24px;text-align:left;'>");
        if (includeLogo)
        {
            sb.Append($"<img src='{safeLogoUrl}' alt='PDX SMS Safety Management System' style='display:block;max-width:320px;width:100%;height:auto;' />");
        }
        else
        {
            sb.Append("<div style='color:#ffffff;font-size:28px;font-weight:700;line-height:1.1;'>PDX SMS</div>");
            sb.Append("<div style='color:#ffffff;font-size:14px;font-weight:400;'>Safety Management System</div>");
        }
        sb.Append("</td></tr>");
        sb.Append($"<tr><td style='background:#003F40;color:#ffffff;padding:12px 24px;font-size:20px;font-weight:bold;'>{safeTitle}</td></tr>");
        sb.Append($"<tr><td style='padding:20px 24px;font-size:14px;line-height:1.6;'>{introHtml}</td></tr>");

        if (summaryFields is { Count: > 0 })
        {
            sb.Append("<tr><td style='padding:0 24px 20px 24px;font-size:14px;line-height:1.6;'>");
            foreach (var field in summaryFields)
            {
                var safeLabel = WebUtility.HtmlEncode(field.Label ?? string.Empty);
                var value = field.ValueIsHtml ? field.Value ?? string.Empty : WebUtility.HtmlEncode(field.Value ?? string.Empty);
                sb.Append($"<strong>{safeLabel}:</strong> {value}<br/>");
            }

            sb.Append("</td></tr>");
        }

        if (sections is { Count: > 0 })
        {
            sb.Append("<tr><td style='padding:0 24px 24px 24px;'><table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='border-collapse:collapse;font-size:13px;'>");

            foreach (var section in sections)
            {
                var safeSectionTitle = WebUtility.HtmlEncode(section.Title ?? string.Empty);
                sb.Append($"<tr><td colspan='2' style='background:#00AF9B;padding:10px;font-weight:bold;'>{safeSectionTitle}</td></tr>");

                foreach (var field in section.Fields)
                {
                    var safeLabel = WebUtility.HtmlEncode(field.Label ?? string.Empty);
                    var value = field.ValueIsHtml ? field.Value ?? string.Empty : WebUtility.HtmlEncode(field.Value ?? string.Empty);

                    if (field.IsFullWidth)
                    {
                        sb.Append($"<tr><td colspan='2' style='padding:10px;border:1px solid #e6e6e6;'><strong>{safeLabel}</strong><br/>{value}</td></tr>");
                    }
                    else
                    {
                        sb.Append($"<tr><td style='width:35%;padding:10px;border:1px solid #e6e6e6;background:#fafafa;'>{safeLabel}</td><td style='padding:10px;border:1px solid #e6e6e6;'>{value}</td></tr>");
                    }
                }
            }

            sb.Append("</table></td></tr>");
        }

        if (!string.IsNullOrWhiteSpace(footerHtml))
        {
            sb.Append($"<tr><td style='padding:0 24px 24px 24px;font-size:12px;color:#444;'>{footerHtml}</td></tr>");
        }

        sb.Append("</table></td></tr></table></body></html>");
        return sb.ToString();
    }

    private static bool ShouldRenderLogo(string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
        {
            return false;
        }

        if (!Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return !uri.IsLoopback &&
               !string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase);
    }
}
