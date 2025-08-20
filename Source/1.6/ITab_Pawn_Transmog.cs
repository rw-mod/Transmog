using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Transmog;

public class ITab_Pawn_Transmog : ITab
{
    private Vector2 _scrollPosition = Vector2.zero;

    private Pawn Pawn => SelPawn ?? (SelThing as Corpse)?.InnerPawn;
    private CompTransmog Preset => Pawn.Preset();
    private readonly Texture2D _paintTex = ContentFinder<Texture2D>.Get("UI/Designators/Paint_Top");
    private readonly Texture2D _revertTex = ContentFinder<Texture2D>.Get("UI/Revert");

    public ITab_Pawn_Transmog()
    {
        size = new Vector2(504, 400);
        labelKey = "Transmog.Transmog".Translate();
    }

    protected override void FillTab()
    {
        const float margin = 16f;
        Rect inRect = new Rect(margin, 0, size.x - 2 * margin, size.y - margin);
        const float height = 32f;
        float width = inRect.width - margin;
        float curY = 40f;
        const float gap = 8f;

        Text.Font = GameFont.Small;

        Widgets.Label(new Rect(inRect.xMin, curY + 5, width / 2, 22f), "Enable".Translate());
        if (Widgets.ButtonImage(new Rect(inRect.center.x - gap - height, curY + 4, 24, 24), Preset.Enabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
        {
            Preset.Enabled ^= true;
        }

        Widgets.Label(new Rect(inRect.center.x + gap, curY + 5, width / 2, 22f), "Transmog.DraftedTransmogEnabled".Translate());
        if (Widgets.ButtonImage(new Rect(inRect.xMax - height, curY + 4, 24, 24), Preset.DraftedTransmogEnabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
        {
            Preset.DraftedTransmogEnabled ^= true;
        }

        curY += height + gap;

        if (Widgets.ButtonText(new Rect(inRect.x, curY, width / 3 - gap, height), "Transmog.CopyFromApparel".Translate()))
        {
            Preset.CopyFromApparel();
        }

        if (!Preset.History.EnumerableNullOrEmpty() && Widgets.ButtonImage(new Rect(inRect.xMax - height, inRect.yMax - height, height, height), _revertTex))
        {
            Preset.TryRevert();
        }

        if (Widgets.ButtonText(new Rect(inRect.x + 1 * width / 3 + gap / 2, curY, width / 3 - gap, height), "Add".Translate()))
        {
            Find.WindowStack.Add(new Dialog_AddTransmog(Pawn));
        }

        if (Widgets.ButtonText(new Rect(inRect.x + 2 * width / 3 + gap, curY, width / 3 - gap, height), "Transmog.Preset".Translate()))
        {
            Find.WindowStack.Add(
                new FloatMenu(
                    PresetManager
                        .presets.Select(preset =>
                            new FloatMenuOption(
                                preset.Key,
                                () =>
                                {
                                    if (Event.current.shift)
                                    {
                                        PresetManager.DelPreset(preset.Key);
                                    }
                                    else
                                    {
                                        Preset.CopyFromPreset(preset.Value);
                                    }
                                }
                            )
                        )
                        .Append(new FloatMenuOption("Transmog.Save".Translate(), () => Find.WindowStack.Add(new Dialog_SavePreset(Preset))))
                        .ToList()
                )
            );
        }

        curY += height + gap;

        float scrollViewHeight = Preset.Apparel.Count * height;
        Widgets.BeginScrollView(new Rect(inRect.x, curY, width, inRect.height - curY), ref _scrollPosition, new Rect(inRect.x, curY, width - margin, scrollViewHeight));

        int indexToMoveUp = -1;
        int indexToRemove = -1;
        for (int i = 0; i < Preset.Transmog.Count; ++i)
        {
            TransmogApparel transmog = Preset.Transmog[i];
            Rect rowRect = new Rect(inRect.x, curY, width, height);

            if (i != 0 && Widgets.ButtonImage(new Rect(inRect.x, rowRect.y, height / 2, height / 2), TexButton.ReorderUp))
            {
                indexToMoveUp = i;
            }

            if (i != Preset.Transmog.Count - 1 && Widgets.ButtonImage(new Rect(inRect.x, rowRect.y + height / 2, height / 2, height / 2), TexButton.ReorderDown))
            {
                indexToMoveUp = i + 1;
            }

            Widgets.ThingIcon(new Rect(inRect.x + height * 0.5f + gap, curY, height, height), transmog.GetApparel());
            Widgets.Label(new Rect(inRect.x + height * 1.5f + gap * 2, curY + 5f, width, height - 10f), transmog.GetApparel().def.LabelCap);

            WidgetRow rowRight = new WidgetRow(rowRect.xMax - margin, rowRect.y, UIDirection.LeftThenDown);
            if (rowRight.ButtonIcon(TexButton.Delete))
            {
                indexToRemove = i;
            }
            if (rowRight.ButtonIcon(_paintTex))
            {
                transmog.Pawn = Pawn;
                Find.WindowStack.Add(new Dialog_EditTransmog(transmog));
            }
            curY += height;
        }
        if (indexToMoveUp != -1)
        {
            Preset.MoveUp(indexToMoveUp);
        }
        if (indexToRemove != -1)
        {
            Preset.RemoveAt(indexToRemove);
        }
        Widgets.EndScrollView();
    }
}