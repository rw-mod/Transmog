using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using static HarmonyLib.AccessTools;

namespace Transmog
{
    [StaticConstructorOnStartup]
    static class Startup
    {
        static Startup()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading.Where(def => def.race?.Humanlike ?? false))
            {
                def.comps.Add(new CompProperties_Transmog());
                def.inspectorTabs.Add(typeof(ITab_Pawn_Transmog));
                def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Transmog)));
            }
            var harmony = new Harmony("localghost.transmog");
            var renderTranspiler = new HarmonyMethod(Method("Transmog.HarmonyPatches:RenderTranspiler"));
            foreach (
                var typeColonName in new List<string>
                {
                    "Verse.PawnRenderUtility:DrawEquipmentAndApparelExtras",
                    "Verse.PawnRenderTree:AdjustParms",
                    "Verse.PawnRenderTree:SetupApparelNodes"
                }
            )
                harmony.Patch(Method(typeColonName), transpiler: renderTranspiler);
            harmony.Patch(Method("Verse.PawnRenderTree:SetupApparelNodes"), transpiler: new HarmonyMethod(Method("Transmog.HarmonyPatches:SetupTranspiler")));
            harmony.Patch(PropertyGetter("RimWorld.CompShield:ShouldDisplay"), prefix: new HarmonyMethod(Method("Transmog.HarmonyPatches:ShieldPrefix")));
            harmony.Patch(PropertySetter("RimWorld.Pawn_DraftController:Drafted"), postfix: new HarmonyMethod(Method("Transmog.HarmonyPatches:DraftedPostfix")));
            PresetManager.LoadPresets();
        }
    }

    static class HarmonyPatches
    {
        static IEnumerable<CodeInstruction> RenderTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            for (int i = 0; i < instructions.Count(); ++i)
            {
                yield return instructions.ElementAt(i);
                if (
                    i + 2 < instructions.Count()
                    && instructions.ElementAt(i + 2).opcode == OpCodes.Callvirt
                    && (MethodInfo)instructions.ElementAt(i + 2).operand == PropertyGetter("RimWorld.Pawn_ApparelTracker:WornApparel")
                )
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, Method("Transmog.Utility:TransmogApparel"));
                    i += 2;
                }
            }
        }

        static IEnumerable<CodeInstruction> SetupTranspiler(IEnumerable<CodeInstruction> instructions) =>
            instructions.Where(
                instruction => instruction.opcode != OpCodes.Callvirt || (MethodInfo)instruction.operand != PropertyGetter("RimWorld.Pawn_ApparelTracker:WornApparelCount")
            );

        static bool ShieldPrefix(ref CompShield __instance, ref bool __result) => !(__instance.parent is Apparel apparel) || apparel.Wearer != null || (__result = false);

        static void DraftedPostfix(ref Pawn_DraftController __instance) => __instance.pawn.apparel?.Notify_ApparelChanged();
    }
}
