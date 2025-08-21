using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace Transmog;

internal static class PresetDataSaveLoader
{
    private static readonly string OldFile = Path.Combine(GenFilePaths.ConfigFolderPath, "Transmog.xml");
    public static bool ExistsOldFile => File.Exists(OldFile);

    public static void StartupMigration()
    {
        if (ExistsOldFile && !Transmog.settings.Migration)
        {
            Migration();
        }
    }

    public static void Migration()
    {
        if (!ExistsOldFile) return;
        Dictionary<string, List<TransmogApparel>> presets = new Dictionary<string, List<TransmogApparel>>();
        Scribe.loader.InitLoading(OldFile);
        ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.None, true);
        Scribe_Collections.Look(ref presets, "presets", LookMode.Value, LookMode.Deep);
        Scribe.loader.FinalizeLoading();

        if (presets.Any())
        {
            foreach (KeyValuePair<string, List<TransmogApparel>> kvp in presets)
            {
                SavePreset(kvp.Key, kvp.Value);
            }
        }

        Transmog.settings.Migration = true;
        LoadedModManager.GetMod<Transmog>().WriteSettings();
    }
    
    private static string SavedPresetsFolderPath
    {
        get
        {
            string text = Path.Combine(GenFilePaths.SaveDataFolderPath, "Transmog");
            DirectoryInfo directoryInfo = new DirectoryInfo(text);
            if (!directoryInfo.Exists) directoryInfo.Create();
            return text;
        }
    }
    
    private static string FilePathForSavedPreset(string presetName) => Path.Combine(SavedPresetsFolderPath, presetName + ".trm");
    
    public static IEnumerable<FileInfo> AllPresetFiles
    {
        get
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(SavedPresetsFolderPath);
            if (!directoryInfo.Exists) directoryInfo.Create();
            
            return from f in directoryInfo.GetFiles()
                where f.Extension == ".trm"
                orderby f.LastWriteTime descending
                select f;
        }
    }

    public static void SavePreset(string fileName, List<TransmogApparel> preset)
    {
        try
        {
            SafeSaver.Save(FilePathForSavedPreset(fileName), "Transmog", () =>
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                Scribe_Collections.Look(ref preset, "preset", LookMode.Deep);
            });
        }
        catch (Exception ex)
        {
            Log.Error($"Exception while saving preset: {ex}");
        }
    }
    
    public static bool TryLoadPreset(string fileName, out List<TransmogApparel> preset)
    {
        preset = null;
        try
        {
            Scribe.loader.InitLoading(FilePathForSavedPreset(fileName));
            try
            {
                ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.ModList, logVersionConflictWarning: true);
                Scribe_Collections.Look(ref preset, "preset", LookMode.Deep);
                Scribe.loader.FinalizeLoading();
            }
            catch
            {
                Scribe.ForceStop();
                throw;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Exception loading preset: {ex}");
            preset = null;
            Scribe.ForceStop();
        }
        
        return preset != null;
    }
}