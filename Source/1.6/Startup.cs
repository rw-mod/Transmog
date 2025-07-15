using System.Linq;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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
            def.inspectorTabs.Add(typeof(ITab_Pawn_Transmog));
            def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Transmog)));
        }
        
        PresetManager.LoadPresets();
    }
}

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    private static readonly Type PatchType = typeof(HarmonyPatches);
    private static readonly HarmonyMethod TranspilerRender = new HarmonyMethod(PatchType.GetMethod(nameof(Transpiler_Render), BindingFlags.Public | BindingFlags.Static));
    private static readonly HarmonyMethod PrefixGetDynamicNodes = new HarmonyMethod(PatchType.GetMethod(nameof(Prefix_GetDynamicNodes), BindingFlags.Public | BindingFlags.Static));
    private static readonly HarmonyMethod PrefixShield = new HarmonyMethod(PatchType.GetMethod(nameof(Prefix_Shield), BindingFlags.Public | BindingFlags.Static));
    private static readonly HarmonyMethod PostfixDrafted = new HarmonyMethod(PatchType.GetMethod(nameof(Postfix_Drafted), BindingFlags.Public | BindingFlags.Static));

    static HarmonyPatches()
    {
        Harmony harmony = new Harmony("rw.mod.transmog");

        harmony.Patch(AccessTools.Method(typeof(PawnRenderUtility), "DrawEquipmentAndApparelExtras"), transpiler: TranspilerRender);
        harmony.Patch(AccessTools.Method(typeof(PawnRenderTree), "AdjustParms"), transpiler: TranspilerRender);
        harmony.Patch(AccessTools.Method(typeof(DynamicPawnRenderNodeSetup_Apparel), "GetDynamicNodes"), prefix: PrefixGetDynamicNodes);
        harmony.Patch(AccessTools.PropertyGetter(typeof(CompShield), "ShouldDisplay"), prefix: PrefixShield);
        harmony.Patch(AccessTools.PropertySetter(typeof(Pawn_DraftController), "Drafted"), postfix: PostfixDrafted);
    }

    private static readonly Func<Apparel, bool> ShouldAddApparelNode = 
        (Func<Apparel, bool>)Delegate.CreateDelegate(
            typeof(Func<Apparel, bool>),
            typeof(DynamicPawnRenderNodeSetup_Apparel).GetMethod("ShouldAddApparelNode", BindingFlags.NonPublic | BindingFlags.Static)!
        );
    private static readonly MethodInfo ProcessApparelMethod = AccessTools.Method(typeof(DynamicPawnRenderNodeSetup_Apparel), "ProcessApparel");
    private static readonly object InstanceDynamicPawnRenderNodeSetupApparel = Activator.CreateInstance(typeof(DynamicPawnRenderNodeSetup_Apparel));
    
    public static bool Prefix_GetDynamicNodes(Pawn pawn, PawnRenderTree tree, ref IEnumerable<(PawnRenderNode, PawnRenderNode)> __result)
    {
        List<(PawnRenderNode, PawnRenderNode)> result = new List<(PawnRenderNode, PawnRenderNode)>();
        
        Dictionary<PawnRenderNode, int> layerOffsets = new Dictionary<PawnRenderNode, int>();
        PawnRenderNode node;
        PawnRenderNode headApparelNode = (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.ApparelHead, out node) ? node : null);
        PawnRenderNode node2;
        PawnRenderNode bodyApparelNode = (tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.ApparelBody, out node2) ? node2 : null);
        foreach (Apparel item in Utility.TransmogApparel(pawn))
        {
            if (!ShouldAddApparelNode(item))
            {
                continue;
            }
            
            IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> processApparel = (IEnumerable<(PawnRenderNode node, PawnRenderNode parent)>)ProcessApparelMethod.Invoke(InstanceDynamicPawnRenderNodeSetupApparel, new object[]
            {
                pawn, tree, item, headApparelNode, bodyApparelNode, layerOffsets
            });
            
            foreach ((PawnRenderNode node, PawnRenderNode parent) valueTuple in processApparel)
            {
                if (valueTuple.node != null)
                {
                    result.Add(valueTuple);
                }
                if (valueTuple.parent != null && !layerOffsets.TryAdd(valueTuple.parent, 1))
                {
                    layerOffsets[valueTuple.parent]++;
                }
            }
        }

        __result = result;
        return false;
    }
    
    public static IEnumerable<CodeInstruction> Transpiler_Render(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = instructions.ToList();

        for (int i = 0; i < codes.Count; i++)
        {
            yield return codes.ElementAt(i);

            if (i + 2 >= codes.Count) continue;
            CodeInstruction tempCode = codes.ElementAt(i + 2);
            if (tempCode.opcode != OpCodes.Callvirt) continue;
            if ((MethodInfo)tempCode.operand != AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), "WornApparel")) continue;

            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Utility), "TransmogApparel"));

            i += 2;
        }
    }

    public static bool Prefix_Shield(ref CompShield __instance, ref bool __result)
    {
        return __instance.parent is not Apparel { Wearer: null } || (__result = false);
    }

    public static void Postfix_Drafted(ref Pawn_DraftController __instance)
    {
        __instance.pawn.apparel?.Notify_ApparelChanged();
    }
}
