using System.Globalization;
using System.Net;
using System.Text;
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
        Normalize(Data);
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
        note.Title = note.Title.Trim();
        note.Content = note.Content.Trim();
        note.Category = string.IsNullOrWhiteSpace(note.Category) ? "General" : note.Category.Trim();
        note.UpdatedAt = DateTime.Now;

        if (current is null) Data.Notes.Add(note);
        else
        {
            current.Title = note.Title;
            current.Content = note.Content;
            current.Category = note.Category;
            current.IsImportant = note.IsImportant;
            current.UpdatedAt = note.UpdatedAt;
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
        activity.Title = activity.Title.Trim();
        activity.Description = activity.Description.Trim();
        activity.Category = string.IsNullOrWhiteSpace(activity.Category) ? "Personal" : activity.Category.Trim();
        activity.Date = activity.Date.Date;

        if (current is null) Data.Activities.Add(activity);
        else
        {
            current.Title = activity.Title;
            current.Description = activity.Description;
            current.Date = activity.Date;
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
        var lines = new List<string>
        {
            "APUNTADOR - EXPORTACIÓN COMPLETA",
            $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}",
            "",
            "NOTAS",
            new string('=', 64)
        };

        foreach (var note in OrderedNotes())
        {
            lines.Add($"[{note.Category}] {(note.IsImportant ? "IMPORTANTE - " : "")}{note.Title}");
            lines.Add(note.Content);
            lines.Add($"Actualizada: {note.UpdatedAt:dd/MM/yyyy HH:mm}");
            lines.Add(new string('-', 64));
        }

        lines.Add("");
        lines.Add("ACTIVIDADES");
        lines.Add(new string('=', 64));
        foreach (var item in OrderedActivities())
        {
            lines.Add($"[{(item.IsCompleted ? "COMPLETADA" : "PENDIENTE")}] {item.Title}");
            lines.Add($"Fecha: {item.Date:dd/MM/yyyy} {(item.Time?.ToString(@"hh\:mm") ?? "Todo el día")} | Categoría: {item.Category}");
            if (!string.IsNullOrWhiteSpace(item.Description)) lines.Add(item.Description);
            lines.Add(new string('-', 64));
        }
        return string.Join(Environment.NewLine, lines);
    }

    public string CreateNotesCsv()
    {
        var sb = new StringBuilder("Id,Titulo,Contenido,Categoria,Importante,Creada,Actualizada\r\n");
        foreach (var note in OrderedNotes())
            sb.AppendLine(string.Join(',', Csv(note.Id), Csv(note.Title), Csv(note.Content), Csv(note.Category), Csv(note.IsImportant), Csv(note.CreatedAt.ToString("O")), Csv(note.UpdatedAt.ToString("O"))));
        return sb.ToString();
    }

    public string CreateActivitiesCsv()
    {
        var sb = new StringBuilder("Id,Titulo,Descripcion,Fecha,Hora,Categoria,Completada,Creada\r\n");
        foreach (var item in OrderedActivities())
            sb.AppendLine(string.Join(',', Csv(item.Id), Csv(item.Title), Csv(item.Description), Csv(item.Date.ToString("yyyy-MM-dd")), Csv(item.Time?.ToString(@"hh\:mm") ?? ""), Csv(item.Category), Csv(item.IsCompleted), Csv(item.CreatedAt.ToString("O"))));
        return sb.ToString();
    }

    public string CreateMarkdownExport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Apuntador").AppendLine().AppendLine($"Exportado: {DateTime.Now:dd/MM/yyyy HH:mm}").AppendLine();
        sb.AppendLine("## Notas").AppendLine();
        foreach (var note in OrderedNotes())
        {
            sb.AppendLine($"### {(note.IsImportant ? "★ " : "")}{EscapeMarkdown(note.Title)}");
            sb.AppendLine($"**Categoría:** {EscapeMarkdown(note.Category)}  ");
            sb.AppendLine($"**Actualizada:** {note.UpdatedAt:dd/MM/yyyy HH:mm}").AppendLine();
            sb.AppendLine(note.Content).AppendLine();
        }
        sb.AppendLine("## Actividades").AppendLine();
        foreach (var item in OrderedActivities())
        {
            sb.AppendLine($"- [{(item.IsCompleted ? "x" : " ")}] **{EscapeMarkdown(item.Title)}** — {item.Date:dd/MM/yyyy} {(item.Time?.ToString(@"hh\:mm") ?? "Todo el día")} · {EscapeMarkdown(item.Category)}");
            if (!string.IsNullOrWhiteSpace(item.Description)) sb.AppendLine($"  - {item.Description.Replace("\n", " ")}");
        }
        return sb.ToString();
    }

    public string CreateHtmlExport()
    {
        var notes = string.Join("", OrderedNotes().Select(n =>
            $"<article><h2>{(n.IsImportant ? "★ " : "")}{WebUtility.HtmlEncode(n.Title)}</h2>" +
            $"<p class='meta'>{WebUtility.HtmlEncode(n.Category)} · {n.UpdatedAt:dd/MM/yyyy HH:mm}</p>" +
            $"<div>{WebUtility.HtmlEncode(n.Content).Replace("\n", "<br>")}</div></article>"));

        var activities = string.Join("", OrderedActivities().Select(a =>
            $"<li class='{(a.IsCompleted ? "done" : "")}'>" +
            $"<strong>{WebUtility.HtmlEncode(a.Title)}</strong> — " +
            $"{a.Date:dd/MM/yyyy} {(a.Time?.ToString(@"hh\:mm") ?? "Todo el día")} · " +
            $"{WebUtility.HtmlEncode(a.Category)}<br>" +
            $"{WebUtility.HtmlEncode(a.Description)}</li>"));

        return $@"<!doctype html>
<html lang=""es"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>Apuntador - Exportación</title>
    <style>
        body {{ font: 16px system-ui; max-width: 920px; margin: auto; padding: 32px; line-height: 1.55; color: #171717; }}
        h1 {{ border-bottom: 3px solid #e50914; }}
        article {{ padding: 18px 0; border-bottom: 1px solid #ddd; }}
        .meta {{ color: #666; }}
        li {{ margin: 12px 0; }}
        .done {{ text-decoration: line-through; color: #777; }}
        @media (max-width: 600px) {{ body {{ padding: 18px; }} }}
    </style>
</head>
<body>
    <h1>Apuntador</h1>
    <p>Exportado: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    <h1>Notas</h1>
    {notes}
    <h1>Actividades</h1>
    <ul>{activities}</ul>
</body>
</html>";
    }

    public string CreateCalendarIcs()
    {
        static string Ics(string value) => value.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\r", "").Replace("\n", "\\n");
        var sb = new StringBuilder("BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:-//Apuntador//ES\r\nCALSCALE:GREGORIAN\r\n");
        foreach (var item in OrderedActivities())
        {
            var start = item.Date.Date + (item.Time ?? TimeSpan.Zero);
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{item.Id}@apuntador");
            sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}");
            if (item.Time.HasValue)
            {
                sb.AppendLine($"DTSTART:{start:yyyyMMdd'T'HHmmss}");
                sb.AppendLine($"DTEND:{start.AddHours(1):yyyyMMdd'T'HHmmss}");
            }
            else
            {
                sb.AppendLine($"DTSTART;VALUE=DATE:{start:yyyyMMdd}");
                sb.AppendLine($"DTEND;VALUE=DATE:{start.AddDays(1):yyyyMMdd}");
            }
            sb.AppendLine($"SUMMARY:{Ics(item.Title)}");
            sb.AppendLine($"DESCRIPTION:{Ics(item.Description)}");
            sb.AppendLine($"CATEGORIES:{Ics(item.Category)}");
            sb.AppendLine($"STATUS:{(item.IsCompleted ? "COMPLETED" : "CONFIRMED")}");
            sb.AppendLine("END:VEVENT");
        }
        sb.AppendLine("END:VCALENDAR");
        return sb.ToString();
    }

    public async Task<(bool Success, string Message)> ImportAsync(string json)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<BackupEnvelope>(json, _jsonOptions);
            var imported = envelope?.Data ?? JsonSerializer.Deserialize<AppData>(json, _jsonOptions);
            if (imported is null) return (false, "El archivo no contiene información válida.");
            Normalize(imported);
            Data = imported;
            await SaveAsync();
            return (true, "La copia de seguridad se importó correctamente.");
        }
        catch (JsonException)
        {
            return (false, "El archivo JSON está dañado o no pertenece a Apuntador.");
        }
    }

    private IEnumerable<NoteItem> OrderedNotes() => Data.Notes.OrderByDescending(x => x.IsImportant).ThenByDescending(x => x.UpdatedAt);
    private IEnumerable<ActivityItem> OrderedActivities() => Data.Activities.OrderBy(x => x.Date).ThenBy(x => x.Time).ThenBy(x => x.Title);
    private static string Csv(object? value) => $"\"{Convert.ToString(value, CultureInfo.InvariantCulture)?.Replace("\"", "\"\"")}\"";
    private static string EscapeMarkdown(string value) => value.Replace("#", "\\#").Replace("*", "\\*").Replace("_", "\\_");
    private static void Normalize(AppData data)
    {
        data.Notes ??= [];
        data.Activities ??= [];
        data.NoteCategories ??= ["General", "Trabajo", "Estudio", "Personal", "Ideas"];
        data.Theme = data.Theme == "light" ? "light" : "dark";
    }
}
