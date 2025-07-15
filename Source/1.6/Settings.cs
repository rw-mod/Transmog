using UnityEngine;
using Verse;

namespace Transmog;

public class Settings : ModSettings
{
    public bool DisplayAllStyles;
    public bool DisabledOnDraft;
    public bool AlphaChannelEnabled;

    public void DoWindowContents(Rect inRect)
    {
        const float height = 32f;
        Listing_Standard ls = new Listing_Standard();
        ls.Begin(inRect);
        {
            Rect rowRect = ls.GetRect(height);
            WidgetRow row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
            row.Label("Transmog.DisplayAllStyles".Translate());
            WidgetRow rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
            if (rowRight.ButtonIcon(DisplayAllStyles ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
            {
                DisplayAllStyles = !DisplayAllStyles;
            }
        }
        {
            Rect rowRect = ls.GetRect(height);
            WidgetRow row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
            row.Label("Transmog.DisabledOnDraft".Translate());
            WidgetRow rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
            if (rowRight.ButtonIcon(DisabledOnDraft ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
            {
                DisabledOnDraft = !DisabledOnDraft;
            }
        }
        {
            Rect rowRect = ls.GetRect(height);
            WidgetRow row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
            row.Label("Transmog.AlphaChannelEnabled".Translate());
            WidgetRow rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
            if (rowRight.ButtonIcon(AlphaChannelEnabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
            {
                AlphaChannelEnabled = !AlphaChannelEnabled;
            }
        }
        ls.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref DisplayAllStyles, "displayAllStyles");
        Scribe_Values.Look(ref DisabledOnDraft, "disabledOnDraft");
        Scribe_Values.Look(ref AlphaChannelEnabled, "alphaChannelEnabled");
    }
}

public class Transmog : Mod
{
    public static Settings settings;

    public Transmog(ModContentPack content) : base(content)
    {
        settings = GetSettings<Settings>();
    }

    public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);

    public override string SettingsCategory() => "Transmog.Transmog".Translate();
}
