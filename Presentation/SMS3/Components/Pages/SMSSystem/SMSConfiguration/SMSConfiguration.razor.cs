using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using SMS3.Components.Shared;
namespace SMS3.Components.Pages.SMSSystem.SMSConfiguration;

public partial class SMSConfiguration
{

private readonly List<SettingItem> _settings = new();
    private JsonObject? _root;
    private string _appSettingsPath = string.Empty;

    private bool _isLoading = true;
    private bool _isSaving;
    private string? _statusMessage;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        _appSettingsPath = Path.Combine(Environment.ContentRootPath, "appsettings.json");
        await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            var json = await File.ReadAllTextAsync(_appSettingsPath);
            _root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            })?.AsObject();

            _settings.Clear();

            if (_root is null)
            {
                _errorMessage = "Could not parse appsettings.json.";
                return;
            }

            FlattenSettings(_root, string.Empty, string.Empty);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load appsettings.json");
            _errorMessage = $"Failed to load appsettings.json: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void FlattenSettings(JsonObject obj, string currentPath, string currentTopLevel)
    {
        foreach (var property in obj)
        {
            var path = string.IsNullOrWhiteSpace(currentPath) ? property.Key : $"{currentPath}:{property.Key}";
            var topLevel = string.IsNullOrWhiteSpace(currentTopLevel) ? property.Key : currentTopLevel;

            if (property.Value is JsonObject childObj)
            {
                FlattenSettings(childObj, path, topLevel);
                continue;
            }

            if (property.Value is JsonArray array)
            {
                _settings.Add(new SettingItem
                {
                    Path = path,
                    TopLevelCategory = topLevel,
                    SettingType = ConfigSettingType.Array,
                    TextValue = string.Join(global::System.Environment.NewLine, array.Select(GetNodeValueAsString))
                });
                continue;
            }

            if (property.Value is JsonValue value)
            {
                if (value.TryGetValue<bool>(out var boolValue))
                {
                    _settings.Add(new SettingItem
                    {
                        Path = path,
                        TopLevelCategory = topLevel,
                        SettingType = ConfigSettingType.Boolean,
                        BooleanValue = boolValue,
                        TextValue = boolValue.ToString()
                    });
                }
                else if (value.TryGetValue<int>(out var intValue))
                {
                    _settings.Add(new SettingItem
                    {
                        Path = path,
                        TopLevelCategory = topLevel,
                        SettingType = ConfigSettingType.Number,
                        TextValue = intValue.ToString(CultureInfo.InvariantCulture)
                    });
                }
                else if (value.TryGetValue<long>(out var longValue))
                {
                    _settings.Add(new SettingItem
                    {
                        Path = path,
                        TopLevelCategory = topLevel,
                        SettingType = ConfigSettingType.Number,
                        TextValue = longValue.ToString(CultureInfo.InvariantCulture)
                    });
                }
                else if (value.TryGetValue<double>(out var doubleValue))
                {
                    _settings.Add(new SettingItem
                    {
                        Path = path,
                        TopLevelCategory = topLevel,
                        SettingType = ConfigSettingType.Number,
                        TextValue = doubleValue.ToString(CultureInfo.InvariantCulture)
                    });
                }
                else if (value.TryGetValue<string>(out var stringValue))
                {
                    _settings.Add(new SettingItem
                    {
                        Path = path,
                        TopLevelCategory = topLevel,
                        SettingType = ConfigSettingType.Text,
                        TextValue = stringValue ?? string.Empty
                    });
                }
                else
                {
                    _settings.Add(new SettingItem
                    {
                        Path = path,
                        TopLevelCategory = topLevel,
                        SettingType = ConfigSettingType.Text,
                        TextValue = value.ToJsonString()
                    });
                }
            }
        }
    }

    private async Task OnBooleanChangedAsync(SettingItem setting, ChangeEventArgs args)
    {
        var newValue = args.Value is bool boolValue && boolValue;
        setting.BooleanValue = newValue;
        setting.TextValue = newValue.ToString();

        await SaveSettingAsync(setting);
    }

    private async Task OnTextChangedAsync(SettingItem setting, ChangeEventArgs args)
    {
        setting.TextValue = args.Value?.ToString() ?? string.Empty;
        await SaveSettingAsync(setting);
    }

    private async Task SaveSettingAsync(SettingItem setting)
    {
        if (_root is null)
        {
            return;
        }

        try
        {
            _isSaving = true;
            _errorMessage = null;
            _statusMessage = null;

            if (!TryUpdateJsonNode(_root, setting))
            {
                _errorMessage = $"Could not update setting: {setting.Path}";
                return;
            }

            var updatedJson = _root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_appSettingsPath, updatedJson);

            _statusMessage = $"Saved {setting.Path} at {DateTime.Now:hh:mm:ss tt}";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save appsettings value for path {Path}", setting.Path);
            _errorMessage = $"Failed to save setting '{setting.Path}': {ex.Message}";
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private static bool TryUpdateJsonNode(JsonObject root, SettingItem setting)
    {
        var segments = setting.Path.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return false;
        }

        JsonObject current = root;
        for (var i = 0; i < segments.Length - 1; i++)
        {
            if (current[segments[i]] is not JsonObject child)
            {
                return false;
            }

            current = child;
        }

        var leafKey = segments[^1];

        current[leafKey] = setting.SettingType switch
        {
            ConfigSettingType.Boolean => JsonValue.Create(setting.BooleanValue),
            ConfigSettingType.Number => CreateNumberNode(setting.TextValue),
            ConfigSettingType.Array => CreateArrayNode(setting.TextValue),
            _ => JsonValue.Create(setting.TextValue)
        };

        return true;
    }

    private static JsonNode? CreateNumberNode(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            return JsonValue.Create(intValue);
        }

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
        {
            return JsonValue.Create(longValue);
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return JsonValue.Create(doubleValue);
        }

        return JsonValue.Create(0);
    }

    private static JsonArray CreateArrayNode(string value)
    {
        var array = new JsonArray();
        var lines = value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            array.Add(JsonValue.Create(line));
        }

        return array;
    }

    private static string GetNodeValueAsString(JsonNode? node)
    {
        if (node is null)
        {
            return string.Empty;
        }

        if (node is JsonValue value)
        {
            if (value.TryGetValue<string>(out var stringValue))
            {
                return stringValue ?? string.Empty;
            }

            if (value.TryGetValue<bool>(out var boolValue))
            {
                return boolValue.ToString();
            }

            if (value.TryGetValue<int>(out var intValue))
            {
                return intValue.ToString(CultureInfo.InvariantCulture);
            }

            if (value.TryGetValue<long>(out var longValue))
            {
                return longValue.ToString(CultureInfo.InvariantCulture);
            }

            if (value.TryGetValue<double>(out var doubleValue))
            {
                return doubleValue.ToString(CultureInfo.InvariantCulture);
            }
        }

        return node.ToJsonString();
    }

    private sealed class SettingItem
    {
        public string Path { get; set; } = string.Empty;
        public string TopLevelCategory { get; set; } = string.Empty;
        public ConfigSettingType SettingType { get; set; }
        public string TextValue { get; set; } = string.Empty;
        public bool BooleanValue { get; set; }
    }

    private enum ConfigSettingType
    {
        Text,
        Number,
        Boolean,
        Array
    }
}


