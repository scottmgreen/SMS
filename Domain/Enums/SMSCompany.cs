//-----------------------------------------------------------------------
// <copyright file="SMSCompany.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SQL-driven smart-enum model for SMS company and stakeholder contact lookup values.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

public sealed class SMSCompany : IEquatable<SMSCompany>
{
    private static readonly object _syncLock = new();

    private static IReadOnlyDictionary<string, SMSCompany> _companiesByCode =
        new Dictionary<string, SMSCompany>(StringComparer.OrdinalIgnoreCase);

    private SMSCompany(
        string value,
        string stakeholderGroup,
        string company,
        string contactName,
        string title,
        string serviceProvided,
        string email,
        string phone,
        string portRep)
    {
        Value = value?.Trim() ?? string.Empty;
        StakeholderGroup = stakeholderGroup?.Trim() ?? string.Empty;
        Company = company?.Trim() ?? string.Empty;
        ContactName = contactName?.Trim() ?? string.Empty;
        Title = title?.Trim() ?? string.Empty;
        ServiceProvided = serviceProvided?.Trim() ?? string.Empty;
        Email = email?.Trim() ?? string.Empty;
        Phone = phone?.Trim() ?? string.Empty;
        PortRep = portRep?.Trim() ?? string.Empty;
    }

    public string Value { get; }
    public string StakeholderGroup { get; }
    public string Company { get; }
    public string ContactName { get; }
    public string Title { get; }
    public string ServiceProvided { get; }
    public string Email { get; }
    public string Phone { get; }
    public string PortRep { get; }

    public string Name => Company;

    public static SMSCompany Create(
        string value,
        string stakeholderGroup,
        string company,
        string contactName,
        string title,
        string serviceProvided,
        string email,
        string phone,
        string portRep)
    {
        return new SMSCompany(value, stakeholderGroup, company, contactName, title, serviceProvided, email, phone, portRep);
    }

    public static void SetCompanies(IEnumerable<SMSCompany> companies)
    {
        if (companies is null)
        {
            return;
        }

        var map = companies
            .Where(c => c is not null && !string.IsNullOrWhiteSpace(c.Value))
            .GroupBy(c => c.Value, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToDictionary(c => c.Value, StringComparer.OrdinalIgnoreCase);

        lock (_syncLock)
        {
            _companiesByCode = map;
        }
    }

    public static SMSCompany? FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _companiesByCode.TryGetValue(value.Trim(), out var company) ? company : null;
    }

    public static SMSCompany? FromCompany(string company)
    {
        if (string.IsNullOrWhiteSpace(company))
        {
            return null;
        }

        return _companiesByCode.Values.FirstOrDefault(c => c.Company.Equals(company.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<SMSCompany> GetAllValues() => _companiesByCode.Values;

    public static List<SMSCompany> GetAllValuesList() => _companiesByCode.Values.ToList();

    public static IEnumerable<SMSCompany> GetByStakeholderGroup(string stakeholderGroup)
    {
        if (string.IsNullOrWhiteSpace(stakeholderGroup))
        {
            return Enumerable.Empty<SMSCompany>();
        }

        return _companiesByCode.Values.Where(c => c.StakeholderGroup.Equals(stakeholderGroup.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool Equals(SMSCompany? other)
    {
        return other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is SMSCompany other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString() => Company;
}
