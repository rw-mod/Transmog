using System.Linq;
using Verse;

namespace Transmog;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.Where(def => def.race?.Humanlike ?? false))
        {
            def.comps.Add(new CompProperties_Transmog());
            def.inspectorTabs.Add(typeof(ITab_Transmog));
            def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Transmog)));
        }

        PresetDataSaveLoader.StartupMigration();
    }
}