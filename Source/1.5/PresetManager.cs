using System.Collections.Generic;
using System.IO;
using Verse;

namespace Transmog
{
    static class PresetManager
    {
        static readonly string path = Path.Combine(GenFilePaths.ConfigFolderPath, "Transmog.xml");
        public static Dictionary<string, List<TransmogApparel>> presets = new Dictionary<string, List<TransmogApparel>>();

        public static void DelPreset(string name)
        {
            presets.Remove(name);
            SavePresets();
        }

        public static void AddPreset(string name, CompTransmog preset)
        {
            presets[name] = preset.Transmog;
            SavePresets();
        }

        public static void LoadPresets()
        {
            if (!File.Exists(path))
                return;
            Scribe.loader.InitLoading(path);
            ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.None, true);
            Scribe_Collections.Look(ref presets, "presets", LookMode.Value, LookMode.Deep);
            Scribe.loader.FinalizeLoading();
        }

        public static void SavePresets() =>
            SafeSaver.Save(
                path,
                "TransmogPresets",
                () =>
                {
                    ScribeMetaHeaderUtility.WriteMetaHeader();
                    Scribe_Collections.Look(ref presets, "presets", LookMode.Value, LookMode.Deep);
                }
            );
    }
}
