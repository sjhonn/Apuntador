using System.Text.Json;
using Apuntador.Models;

namespace Apuntador.Services;

public sealed class AppDataService(LocalStorageService storage)
{
    private const string StorageKey = "apuntador.data.v1";
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private bool _initialized;

    public AppData Data { get; private set; } = new();
    public event Action? Changed;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        Data = await storage.GetAsync<AppData>(StorageKey) ?? new AppData();
        Data.Notes ??= [];
        Data.Activities ??= [];
        Data.NoteCategories ??= ["General", "Trabajo", "Estudio", "Personal", "Ideas"];
        _initialized = true;
    }

    public async Task SaveAsync()
    {
        await storage.SetAsync(StorageKey, Data);
        Changed?.Invoke();
    }

    public async Task UpsertNoteAsync(NoteItem note)
    {
        var current = Data.Notes.FirstOrDefault(x => x.Id == note.Id);
        if (current is null) Data.Notes.Add(note);
        else
        {
            current.Title = note.Title.Trim();
            current.Content = note.Content.Trim();
            current.Category = note.Category;
            current.IsImportant = note.IsImportant;
            current.UpdatedAt = DateTime.Now;
        }
        if (!Data.NoteCategories.Contains(note.Category, StringComparer.OrdinalIgnoreCase))
            Data.NoteCategories.Add(note.Category);
        await SaveAsync();
    }

    public async Task DeleteNoteAsync(Guid id)
    {
        Data.Notes.RemoveAll(x => x.Id == id);
        await SaveAsync();
    }

    public async Task ToggleImportantAsync(Guid id)
    {
        var note = Data.Notes.FirstOrDefault(x => x.Id == id);
        if (note is null) return;
        note.IsImportant = !note.IsImportant;
        note.UpdatedAt = DateTime.Now;
        await SaveAsync();
    }

    public async Task UpsertActivityAsync(ActivityItem activity)
    {
        var current = Data.Activities.FirstOrDefault(x => x.Id == activity.Id);
        if (current is null) Data.Activities.Add(activity);
        else
        {
            current.Title = activity.Title.Trim();
            current.Description = activity.Description.Trim();
            current.Date = activity.Date.Date;
            current.Time = activity.Time;
            current.Category = activity.Category;
            current.IsCompleted = activity.IsCompleted;
        }
        await SaveAsync();
    }

    public async Task DeleteActivityAsync(Guid id)
    {
        Data.Activities.RemoveAll(x => x.Id == id);
        await SaveAsync();
    }

    public async Task ToggleActivityAsync(Guid id)
    {
        var activity = Data.Activities.FirstOrDefault(x => x.Id == id);
        if (activity is null) return;
        activity.IsCompleted = !activity.IsCompleted;
        await SaveAsync();
    }

    public string CreateBackupJson() => JsonSerializer.Serialize(new BackupEnvelope { Data = Data }, _jsonOptions);

    public string CreateTextExport()
    {
        var lines = new List<string> { "APUNTADOR - EXPORTACIÓN DE NOTAS", $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}", "" };
        foreach (var note in Data.Notes.OrderByDescending(x => x.IsImportant).ThenByDescending(x => x.UpdatedAt))
        {
            lines.Add($"[{note.Category}] {(note.IsImportant ? "★ " : "")}{note.Title}");
            lines.Add(note.Content);
            lines.Add($"Actualizada: {note.UpdatedAt:dd/MM/yyyy HH:mm}");
            lines.Add(new string('-', 48));
        }
        return string.Join(Environment.NewLine, lines);
    }

    public async Task<(bool Success, string Message)> ImportAsync(string json)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<BackupEnvelope>(json, _jsonOptions);
            var imported = envelope?.Data ?? JsonSerializer.Deserialize<AppData>(json, _jsonOptions);
            if (imported is null) return (false, "El archivo no contiene información válida.");
            imported.Notes ??= [];
            imported.Activities ??= [];
            imported.NoteCategories ??= ["General", "Trabajo", "Estudio", "Personal", "Ideas"];
            Data = imported;
            await SaveAsync();
            return (true, "La copia de seguridad se importó correctamente.");
        }
        catch (JsonException)
        {
            return (false, "El archivo JSON está dañado o no pertenece a Apuntador.");
        }
    }
}
