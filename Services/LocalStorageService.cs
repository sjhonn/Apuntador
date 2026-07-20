using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Apuntador.Services;

public sealed class LocalStorageService(IJSRuntime js)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await js.InvokeAsync<string?>("apuntadorStorage.get", key);
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
        catch (JsonException) { return default; }
    }

    public ValueTask SetAsync<T>(string key, T value) =>
        js.InvokeVoidAsync("apuntadorStorage.set", key, JsonSerializer.Serialize(value, JsonOptions));

    public ValueTask RemoveAsync(string key) => js.InvokeVoidAsync("apuntadorStorage.remove", key);

    public ValueTask DownloadAsync(string fileName, string content, string mimeType) =>
        js.InvokeVoidAsync("apuntadorFiles.download", fileName, content, mimeType);

    public ValueTask<string> ReadFileAsync(ElementReference input) =>
        js.InvokeAsync<string>("apuntadorFiles.readSelected", input);
}
