using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Transmog
{
    class ITab_Pawn_Transmog : ITab
    {
        Vector2 scrollPosition = Vector2.zero;

        Pawn Pawn => SelPawn ?? (SelThing as Corpse).InnerPawn;
        CompTransmog Preset => Pawn.Preset();
        readonly Texture2D PaintTex = ContentFinder<Texture2D>.Get("UI/Designators/Paint_Top");
        readonly Texture2D RevertTex = ContentFinder<Texture2D>.Get("UI/Revert");

        public ITab_Pawn_Transmog()
        {
            size = new Vector2(504, 400);
            labelKey = "Transmog.Transmog".Translate();
        }

        protected override void FillTab()
        {
            var margin = 16f;
            var inRect = new Rect(margin, 0, size.x - 2 * margin, size.y - margin);
            var height = 32f;
            var width = inRect.width - margin;
            var curY = 40f;
            var gap = 8f;

            Text.Font = GameFont.Small;

            Widgets.Label(new Rect(inRect.xMin, curY + 5, width / 2, 22f), "Enable".Translate());
            if (Widgets.ButtonImage(new Rect(inRect.center.x - gap - height, curY + 4, 24, 24), Preset.Enabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                Preset.Enabled ^= true;

            Widgets.Label(new Rect(inRect.center.x + gap, curY + 5, width / 2, 22f), "Transmog.DraftedTransmogEnabled".Translate());
            if (Widgets.ButtonImage(new Rect(inRect.xMax - height, curY + 4, 24, 24), Preset.DraftedTransmogEnabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                Preset.DraftedTransmogEnabled ^= true;

            curY += height + gap;

            if (Widgets.ButtonText(new Rect(inRect.x, curY, width / 3 - gap, height), "Transmog.CopyFromApparel".Translate()))
                Preset.CopyFromApparel();

            if (!Preset.History.EnumerableNullOrEmpty() && Widgets.ButtonImage(new Rect(inRect.xMax - height, inRect.yMax - height, height, height), RevertTex))
                Preset.TryRevert();

            if (Widgets.ButtonText(new Rect(inRect.x + 1 * width / 3 + gap / 2, curY, width / 3 - gap, height), "Add".Translate()))
                Find.WindowStack.Add(new Dialog_AddTransmog(Pawn));

            if (Widgets.ButtonText(new Rect(inRect.x + 2 * width / 3 + gap, curY, width / 3 - gap, height), "Transmog.Preset".Translate()))
                Find.WindowStack.Add(
                    new FloatMenu(
                        PresetManager
                            .presets.Select(
                                preset =>
                                    new FloatMenuOption(
                                        preset.Key,
                                        () =>
                                        {
                                            if (Event.current.shift)
                                                PresetManager.DelPreset(preset.Key);
                                            else
                                                Preset.CopyFromPreset(preset.Value);
                                        }
                                    )
                            )
                            .Append(new FloatMenuOption("Transmog.Save".Translate(), () => Find.WindowStack.Add(new Dialog_SavePreset(Preset))))
                            .ToList()
                    )
                );

            curY += height + gap;

            var scrollviewHeight = Preset.Apparel.Count * height;
            Widgets.BeginScrollView(new Rect(inRect.x, curY, width, inRect.height - curY), ref scrollPosition, new Rect(inRect.x, curY, width - margin, scrollviewHeight));

            int indexToMoveup = -1;
            int indexToRemove = -1;
            for (var i = 0; i < Preset.Transmog.Count; ++i)
            {
                var transmog = Preset.Transmog[i];
                var rowRect = new Rect(inRect.x, curY, width, height);

                if (i != 0 && Widgets.ButtonImage(new Rect(inRect.x, rowRect.y, height / 2, height / 2), TexButton.ReorderUp))
                    indexToMoveup = i;
                if (i != Preset.Transmog.Count - 1 && Widgets.ButtonImage(new Rect(inRect.x, rowRect.y + height / 2, height / 2, height / 2), TexButton.ReorderDown))
                    indexToMoveup = i + 1;

                Widgets.ThingIcon(new Rect(inRect.x + height * 0.5f + gap, curY, height, height), transmog.GetApparel());
                Widgets.Label(new Rect(inRect.x + height * 1.5f + gap * 2, curY + 5f, width, height - 10f), transmog.GetApparel().def.LabelCap);

                var rowRight = new WidgetRow(rowRect.xMax - margin, rowRect.y, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(TexButton.Delete))
                    indexToRemove = i;
                if (rowRight.ButtonIcon(PaintTex))
                {
                    transmog.Pawn = Pawn;
                    Find.WindowStack.Add(new Dialog_EditTransmog(transmog));
                }
                curY += height;
            }
            if (indexToMoveup != -1)
                Preset.Moveup(indexToMoveup);
            if (indexToRemove != -1)
                Preset.RemoveAt(indexToRemove);
            Widgets.EndScrollView();
        }
    }
}
