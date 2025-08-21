using RimWorld;
using Verse;

namespace Transmog;

public class Dialog_PresetFileList_Save : Dialog_PresetFileList
{
    protected override bool ShouldDoTypeInField => true;

    private readonly CompTransmog _preset;

    public Dialog_PresetFileList_Save(CompTransmog preset)
    {
        interactButLabel = "OverwriteButton".Translate();
        _preset = preset;
    }
    
    protected override void DoFileInteraction(string presetName)
    {
        presetName = GenFile.SanitizedFileName(presetName);
        LongEventHandler.QueueLongEvent(delegate
        {
            PresetDataSaveLoader.SavePreset(presetName, _preset.Transmog);
        }, "SavingLongEvent", doAsynchronously: false, null);
        Messages.Message("SavedAs".Translate(presetName), MessageTypeDefOf.SilentInput, historical: false);
        Close();
    }
}