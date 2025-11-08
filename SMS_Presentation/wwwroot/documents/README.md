# PDXSMS Document Storage Structure

## Directory Organization

### `/wwwroot/documents/`
Central location for all publicly accessible documents in the PDXSMS application.

#### `/policies/` 
**Purpose:** Safety policies, procedures, and regulatory documents
- Safety Management System Policy documents
- Standard Operating Procedures (SOPs)  
- Regulatory compliance documents
- Policy updates and revisions

**Example Files:**
- `sms-policy-v2024.pdf`
- `emergency-procedures.pdf`
- `regulatory-compliance-14cfr139.pdf`

#### `/templates/`
**Purpose:** Downloadable document templates for users
- Risk assessment templates
- Hazard reporting forms
- Audit checklists
- Investigation report templates

**Example Files:**
- `hazard-report-template.pdf`
- `risk-assessment-template.pdf`
- `audit-checklist-template.pdf`

#### `/manuals/`
**Purpose:** User guides, training materials, and system documentation
- System user manuals
- Training guides
- Quick reference cards
- Process flow diagrams

**Example Files:**
- `user-manual-v2024.pdf`
- `hazard-reporting-guide.pdf`
- `risk-assessment-training.pdf`

#### `/forms/`
**Purpose:** Official forms and blank documents
- Printable forms
- Signature pages
- Official letterhead templates

**Example Files:**
- `official-report-form.pdf`
- `signature-authorization.pdf`

## Access Patterns

### Public Access (No Authentication Required)
- General information documents
- Public safety policies
- Contact information

### Authenticated Access (Login Required)
- Internal procedures
- Training materials
- System manuals

### Role-Based Access (Specific Roles Only)
- Confidential procedures
- Administrative documents
- Audit materials

## File Naming Conventions

### Format: `{category}-{document-name}-{version}.pdf`

**Examples:**
- `policy-sms-overview-v2024.pdf`
- `template-hazard-report-v1.2.pdf`
- `manual-user-guide-v2024.1.pdf`
- `form-incident-report-blank.pdf`

## Security Considerations

### File Access Control
- Use middleware or action filters for role-based document access
- Implement download logging for sensitive documents
- Consider encrypted storage for confidential materials

### File Validation
- Validate file types (PDF only for policies)
- Scan for malware before serving
- Verify file integrity with checksums

## Implementation Notes

### HTML Links
```html
<!-- Direct link to policy document -->
<a href="~/documents/policies/sms-policy-v2024.pdf" target="_blank" class="btn btn-primary">
    <i class="fas fa-file-pdf"></i> Download SMS Policy
</a>

<!-- Role-based document access -->
@if (userRole == "Safety Manager" || userRole == "Administrator")
{
    <a href="~/documents/policies/confidential-procedures.pdf" class="btn btn-info">
        <i class="fas fa-lock"></i> Internal Procedures
    </a>
}
```

### Controller Action for Secured Documents
```csharp
public IActionResult DownloadDocument(string category, string filename)
{
    // Implement role-based access control
    // Log download activity
    // Serve file with appropriate headers
}
```

## Storage Best Practices

1. **Version Control:** Keep old versions with date stamps
2. **Backup:** Regular backups of critical policy documents  
3. **Access Logging:** Track who downloads sensitive documents
4. **File Size:** Optimize PDFs for web delivery
5. **Metadata:** Include document properties (author, creation date, etc.)

## Deployment Notes

- Ensure documents are included in deployment packages
- Set appropriate MIME types for PDF serving
- Configure compression for large documents
- Implement CDN caching for frequently accessed documents