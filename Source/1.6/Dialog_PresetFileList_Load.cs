using System.Collections.Generic;
using Verse;

namespace Transmog;

public class Dialog_PresetFileList_Load : Dialog_PresetFileList
{
    protected override bool FocusSearchField => true;

    private readonly CompTransmog _preset;

    public Dialog_PresetFileList_Load(CompTransmog preset)
    {
        interactButLabel = "Load".Translate();
        _preset = preset;
    }

    protected override void DoFileInteraction(string saveFileName)
    {
        if (PresetDataSaveLoader.TryLoadPreset(saveFileName, out List<TransmogApparel> preset))
        {
            _preset.CopyFromPreset(preset);
        }
        Close();
    }
}