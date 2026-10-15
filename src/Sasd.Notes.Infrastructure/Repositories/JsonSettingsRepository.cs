using System.Text.Json;
using Sasd.Notes.Application.Interfaces;
using Sasd.Notes.Domain.Models;

namespace Sasd.Notes.Infrastructure.Repositories;

/// <summary>
/// JSON-basierte Persistenz der Benutzereinstellungen im lokalen Benutzerprofil.
/// </summary>
public sealed class JsonSettingsRepository : ISettingsRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Vollständiger Dateipfad der Einstellungsdatei.
    /// </summary>
    private readonly string _settingsFilePath;

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="JsonSettingsRepository"/>-Klasse.
    /// </summary>
    public JsonSettingsRepository()
    {
        string basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SASD",
            "SASD Notes");

        Directory.CreateDirectory(basePath);
        _settingsFilePath = Path.Combine(basePath, "settings.json");
    }

    /// <inheritdoc />
    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new AppSettings();
        }

        await using FileStream stream = File.OpenRead(_settingsFilePath);

        try
        {
            AppSettings? settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, SerializerOptions, cancellationToken);

            // Fehlende oder null serialisierte Listen werden auf einen sicheren Standard zurückgeführt.
            if (settings is null)
            {
                return new AppSettings();
            }

            settings.RecentFolders ??= new List<RecentFolderEntry>();
            return settings;
        }
        catch (JsonException)
        {
            // Eine beschädigte Einstellungsdatei darf den Start der Anwendung nicht verhindern.
            return new AppSettings();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        string temporaryPath = _settingsFilePath + ".tmp-" + Guid.NewGuid().ToString("N");

        try
        {
            // Die Einstellungen werden zuerst vollständig in eine temporäre Datei geschrieben.
            await using (FileStream stream = new(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.SequentialScan | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, settings, SerializerOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            // Erst danach wird die bisherige Einstellungsdatei ersetzt.
            File.Move(temporaryPath, _settingsFilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
