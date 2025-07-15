using UnityEngine;
using Verse;

namespace Transmog
{
    class Settings : ModSettings
    {
        public bool displayAllStyles;
        public bool disabledOnDraft;
        public bool alphaChannelEnabled;

        public void DoWindowContents(Rect inRect)
        {
            var height = 32f;
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("Transmog.DisplayAllStyles".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(displayAllStyles ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    displayAllStyles = !displayAllStyles;
            }
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("Transmog.DisabledOnDraft".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(disabledOnDraft ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    disabledOnDraft = !disabledOnDraft;
            }
            {
                var rowRect = ls.GetRect(height);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                row.Label("Transmog.AlphaChannelEnabled".Translate());
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(alphaChannelEnabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    alphaChannelEnabled = !alphaChannelEnabled;
            }
            ls.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref displayAllStyles, "displayAllStyles");
            Scribe_Values.Look(ref disabledOnDraft, "disabledOnDraft");
            Scribe_Values.Look(ref alphaChannelEnabled, "alphaChannelEnabled");
        }
    }

    class Transmog : Mod
    {
        public static Settings settings;

        public Transmog(ModContentPack content)
            : base(content) => settings = GetSettings<Settings>();

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoWindowContents(inRect);

        public override string SettingsCategory() => "Transmog.Transmog".Translate();
    }
}
