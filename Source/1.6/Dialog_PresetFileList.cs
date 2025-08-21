using RimWorld;
using System;
using System.IO;
using System.Threading.Tasks;
using Verse;

namespace Transmog;

public abstract class Dialog_PresetFileList : Dialog_FileList
{
    private Task _loadSavesTask;

    private void ReloadFilesTask()
    {
        Parallel.ForEach(files, file =>
        {
            try
            {
                file.LoadData();
            }
            catch (Exception arg)
            {
                Log.Error($"Exception loading {file.FileInfo.Name}: {arg}");
            }
        });
    }

    protected override void ReloadFiles()
    {
        if (_loadSavesTask != null && _loadSavesTask.Status != TaskStatus.RanToCompletion)
        {
            _loadSavesTask.Wait();
        }
        files.Clear();
        foreach (FileInfo allSavedPresetFile in PresetDataSaveLoader.AllPresetFiles)
        {
            try
            {
                SaveFileInfo item = new SaveFileInfo(allSavedPresetFile);
                files.Add(item);
            }
            catch (Exception arg)
            {
                Log.Error($"Exception loading {allSavedPresetFile.Name}: {arg}");
            }
        }
        _loadSavesTask = Task.Run(ReloadFilesTask);
    }
}