using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Transmog;

public static class Utility
{
    public static CompTransmog Preset(this Pawn pawn) => pawn.GetComp<CompTransmog>();

    public static List<Apparel> TransmogApparel(Pawn pawn)
    {
        return (pawn.Preset()?.Enabled ?? false) && !(Transmog.settings.DisabledOnDraft && pawn.Drafted) ? pawn.Preset().Apparel : pawn.apparel.WornApparel;
    }

    private static readonly Dictionary<ThingDef, List<ThingStyleDef>> Styles = new Dictionary<ThingDef, List<ThingStyleDef>>();

    public static List<ThingStyleDef> GetStyles(this ThingDef def)
    {
        if (Styles.TryGetValue(def, out List<ThingStyleDef> styles)) return styles;

        Styles[def] = new List<ThingStyleDef> { null };
        if (!def.randomStyle.NullOrEmpty())
        {
            foreach (ThingStyleChance styleChance in def.randomStyle)
            {
                if (styleChance.StyleDef.graphicData != null)
                {
                    Styles[def].Add(styleChance.StyleDef);
                }
            }
        }
        foreach (StyleCategoryDef styleCat in DefDatabase<StyleCategoryDef>.AllDefsListForReading)
        {
            Styles[def].Add(styleCat.GetStyleForThingDef(def));
        }
        Styles[def].RemoveDuplicates();
        return Styles[def];
    }

    public static Color ToColor(this string hexCode)
    {
        return new Color(
            Convert.ToInt32(hexCode[..2], 16) / 255f,
            Convert.ToInt32(hexCode.Substring(2, 2), 16) / 255f,
            Convert.ToInt32(hexCode.Substring(4, 2), 16) / 255f,
            hexCode.Length == 8 ? Convert.ToInt32(hexCode.Substring(6, 2), 16) / 255f : 1
        );
    }


    public static string ToString(this Color color, bool alphaChannelEnabled = false)
    {
        return $"{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}{(alphaChannelEnabled ? $"{(int)(color.a * 255):X2}" : "")}";
    }
}