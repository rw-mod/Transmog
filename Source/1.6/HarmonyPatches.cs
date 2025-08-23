using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Transmog;

[StaticConstructorOnStartup]
public static class HarmonyPatches
{
    static HarmonyPatches()
    {
        Harmony harmony = new Harmony("rw.mod.transmog");

        Type patchType = typeof(HarmonyPatches);

        HarmonyMethod transpilerReplaceWornApparel = new HarmonyMethod(patchType, nameof(Transpiler_ReplaceWornApparel));
        HarmonyMethod transpilerRemoveCountCheck = new HarmonyMethod(patchType, nameof(Transpiler_RemoveWornApparelCountCheck));

        harmony.Patch(AccessTools.Method(typeof(PawnRenderUtility), "DrawEquipmentAndApparelExtras"), transpiler: transpilerReplaceWornApparel);
        harmony.Patch(AccessTools.Method(typeof(PawnRenderTree), "AdjustParms"), transpiler: transpilerReplaceWornApparel);

        Type innerType = typeof(DynamicPawnRenderNodeSetup_Apparel)
            .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(type => type.Name.Contains("GetDynamicNodes"));

        if (innerType is not null && AccessTools.Method(innerType, "MoveNext") is { } moveNextMethod)
        {
            harmony.Patch(moveNextMethod, transpiler: transpilerReplaceWornApparel);
            harmony.Patch(moveNextMethod, transpiler: transpilerRemoveCountCheck);
        }

        harmony.Patch(AccessTools.PropertyGetter(typeof(CompShield), "ShouldDisplay"), prefix: new HarmonyMethod(patchType, nameof(Prefix_Shield)));
        harmony.Patch(AccessTools.PropertySetter(typeof(Pawn_DraftController), "Drafted"), postfix: new HarmonyMethod(patchType, nameof(Postfix_Drafted)));
    }

    private static readonly MethodInfo WornApparelGetter = AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.WornApparel));
    private static readonly MethodInfo TransmogApparelMethod = AccessTools.Method(typeof(Utility), nameof(Utility.TransmogApparel));
    private static readonly FieldInfo PawnApparelField = AccessTools.Field(typeof(Pawn), nameof(Pawn.apparel));

    public static IEnumerable<CodeInstruction> Transpiler_ReplaceWornApparel(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        List<CodeInstruction> codes = instructions.ToList();

        bool patched = false;

        for (int i = 0; i < codes.Count - 1; i++)
        {
            if (!codes[i].LoadsField(PawnApparelField) || !codes[i + 1].Calls(WornApparelGetter)) continue;

            codes[i].opcode = OpCodes.Nop;
            codes[i].operand = null;

            codes[i + 1].opcode = OpCodes.Call;
            codes[i + 1].operand = TransmogApparelMethod;

            patched = true;

            break;
        }

        if (patched)
        {
            Log.Message($"Transmog: Successfully patched {original.DeclaringType?.Name}.{original.Name}.");
        }
        else
        {
            Log.Warning($"Transmog: Failed to find pattern in {original.DeclaringType?.Name}.{original.Name}. The mod may not work as expected.");
        }

        return codes.AsEnumerable();
    }

    private static readonly MethodInfo WornApparelCountGetter = AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.WornApparelCount));
    public static IEnumerable<CodeInstruction> Transpiler_RemoveWornApparelCountCheck(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = instructions.ToList();

        for (int i = 0; i < codes.Count - 1; i++)
        {
            if (!codes[i].Calls(WornApparelCountGetter) || !codes[i - 1].LoadsField(PawnApparelField) || codes[i + 1].opcode.FlowControl != FlowControl.Cond_Branch) continue;
            
            codes[i - 1].opcode = OpCodes.Nop;
            codes[i].opcode = OpCodes.Nop;
            
            CodeInstruction branchInstruction = codes[i + 1];
            if (branchInstruction.opcode == OpCodes.Brfalse_S)
            {
                branchInstruction.opcode = OpCodes.Br_S;
            }
            else if (branchInstruction.opcode == OpCodes.Brfalse)
            {
                branchInstruction.opcode = OpCodes.Br;
            }

            break;
        }

        return codes.AsEnumerable();
    }

    public static bool Prefix_Shield(ref CompShield __instance, ref bool __result)
    {
        if (__instance.parent is Apparel { Wearer: not null })
        {
            __result = false;
            return false;
        }

        return true;
    }

    public static void Postfix_Drafted(ref Pawn_DraftController __instance)
    {
        __instance.pawn.apparel?.Notify_ApparelChanged();
    }
}