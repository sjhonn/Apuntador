namespace Apuntador.Models;

public sealed class NoteItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public bool IsImportant { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public sealed class ActivityItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public TimeSpan? Time { get; set; }
    public bool IsCompleted { get; set; }
    public string Category { get; set; } = "Personal";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public sealed class AppData
{
    public List<NoteItem> Notes { get; set; } = [];
    public List<ActivityItem> Activities { get; set; } = [];
    public List<string> NoteCategories { get; set; } = ["General", "Trabajo", "Estudio", "Personal", "Ideas"];
    public string Theme { get; set; } = "dark";
    public int Version { get; set; } = 1;
}

public sealed class BackupEnvelope
{
    public string Application { get; set; } = "Apuntador";
    public int Version { get; set; } = 1;
    public DateTime ExportedAt { get; set; } = DateTime.Now;
    public AppData Data { get; set; } = new();
}
