using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Transmog;

public class Dialog_AddTransmog : Window
{
    private static readonly IEnumerable<ThingDef> Apparel = DefDatabase<ThingDef>.AllDefsListForReading.Where(def => def.IsApparel);
    private readonly HashSet<ThingDef> _invertedApparel = new HashSet<ThingDef>();
    private readonly IEnumerable<ThingDef> _apparelForPawn;
    private IEnumerable<ThingDef> Filtered => _apparelForPawn.Where(apparel => apparel.LabelCap.ToString().IndexOf(_filter, StringComparison.InvariantCultureIgnoreCase) >= 0);
    private readonly Pawn _pawn;
    public override Vector2 InitialSize => new Vector2(360, 720);
    private Vector2 _scrollPosition = Vector2.zero;
    private string _filter = "";
    private bool _focused;

    public Dialog_AddTransmog(Pawn pawn)
    {
        preventCameraMotion = false;
        draggable = true;
        resizeable = true;
        doCloseX = true;
        _pawn = pawn;
        _apparelForPawn = Apparel.Where(apparel => apparel.apparel.PawnCanWear(pawn));
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        const int height = 32;
        const int gap = 8;
        float curY = inRect.y;
        int scrollViewHeight = Filtered.Count() * height;
        bool selected = false;
        GUI.SetNextControlName("Filter");
        _filter = Widgets.TextField(new Rect(inRect.x, inRect.yMax - height, inRect.width, height), _filter);
        if (!_focused)
        {
            UI.FocusControl("Filter", this);
            _focused = true;
        }
        Widgets.BeginScrollView(
            new Rect(inRect.x, inRect.y, inRect.width, inRect.height - height - gap),
            ref _scrollPosition,
            new Rect(inRect.x, inRect.y, inRect.width - 20f, scrollViewHeight)
        );
        foreach (ThingDef apparel in Filtered)
        {
            Rect rowRect = new Rect(inRect.x, curY, inRect.width, height);
            if (Mouse.IsOver(rowRect))
            {
                GUI.DrawTexture(rowRect, TexUI.HighlightTex);
            }

            Widgets.Label(new Rect(rowRect.x, rowRect.y + 5f, rowRect.width, height - 10f), apparel.LabelCap);
            List<ThingStyleDef> styles = apparel.GetStyles();
            bool displayAllStyles = TransmogMod.settings.DisplayAllStyles ^ _invertedApparel.Contains(apparel);
            for (int i = 0; i < (displayAllStyles ? styles.Count : 1); i++)
            {
                Widgets.ThingIcon(new Rect(rowRect.xMax - Margin - height * (i + 1), curY, height, height), apparel, thingStyleDef: styles[i]);
            }

            if (styles.Count > 1 && Widgets.ButtonImage(new Rect(rowRect.xMax - Margin - height / 2, curY + height / 2, height / 2, height / 2), TexButton.Add))
            {
                _ = _invertedApparel.Contains(apparel) ? _invertedApparel.Remove(apparel) : _invertedApparel.Add(apparel);
            }

            if (Widgets.ButtonInvisible(rowRect))
            {
                Select(apparel);
                selected = true;
            }
            curY += height;
        }
        Widgets.EndScrollView();
        if (selected)
        {
            Find.WindowStack.TryRemove(this);
        }
    }

    public override void OnAcceptKeyPressed()
    {
        if (!Filtered.EnumerableNullOrEmpty())
        {
            Select(Filtered.First());
        }
        Find.WindowStack.TryRemove(this);
    }

    private void Select(ThingDef apparel)
    {
        TransmogApparel transmog = new TransmogApparel
        {
            Pawn = _pawn,
            ApparelDef = apparel,
            StyleDef = null,
            Color = Color.white
        };
        _pawn.Preset().Add(transmog);
        Find.WindowStack.Add(new Dialog_EditTransmog(transmog));
    }
}