using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Transmog
{
    class Dialog_AddTransmog : Window
    {
        static readonly IEnumerable<ThingDef> apparel = DefDatabase<ThingDef>.AllDefsListForReading.Where(def => def.IsApparel);
        HashSet<ThingDef> invertedApparel = new HashSet<ThingDef>();
        readonly IEnumerable<ThingDef> apparelForPawn;
        IEnumerable<ThingDef> Filtered => apparelForPawn.Where(apparel => apparel.LabelCap.ToString().IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0);
        readonly Pawn pawn;
        public override Vector2 InitialSize => new Vector2(360, 720);
        Vector2 scrollPosition = Vector2.zero;
        string filter = "";
        bool focused;

        public Dialog_AddTransmog(Pawn pawn)
        {
            preventCameraMotion = false;
            draggable = true;
            resizeable = true;
            doCloseX = true;
            this.pawn = pawn;
            apparelForPawn = apparel.Where(apparel => apparel.apparel.PawnCanWear(pawn));
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            var height = 32;
            var gap = 8;
            var curY = inRect.y;
            var scrollviewHeight = Filtered.Count() * height;
            var selected = false;
            GUI.SetNextControlName("Filter");
            filter = Widgets.TextField(new Rect(inRect.x, inRect.yMax - height, inRect.width, height), filter);
            if (!focused)
            {
                UI.FocusControl("Filter", this);
                focused = true;
            }
            Widgets.BeginScrollView(
                new Rect(inRect.x, inRect.y, inRect.width, inRect.height - height - gap),
                ref scrollPosition,
                new Rect(inRect.x, inRect.y, inRect.width - 20f, scrollviewHeight)
            );
            foreach (var apparel in Filtered)
            {
                var rowRect = new Rect(inRect.x, curY, inRect.width, height);
                if (Mouse.IsOver(rowRect))
                    GUI.DrawTexture(rowRect, TexUI.HighlightTex);
                Widgets.Label(new Rect(rowRect.x, rowRect.y + 5f, rowRect.width, height - 10f), apparel.LabelCap);
                var styles = apparel.GetStyles();
                var displayAllStyles = Transmog.settings.displayAllStyles ^ invertedApparel.Contains(apparel);
                for (var i = 0; i < (displayAllStyles ? styles.Count : 1); ++i)
                    Widgets.ThingIcon(new Rect(rowRect.xMax - Margin - height * (i + 1), curY, height, height), apparel, thingStyleDef: styles[i]);
                if (styles.Count > 1 && Widgets.ButtonImage(new Rect(rowRect.xMax - Margin - height / 2, curY + height / 2, height / 2, height / 2), TexButton.Add))
                    _ = invertedApparel.Contains(apparel) ? invertedApparel.Remove(apparel) : invertedApparel.Add(apparel);
                if (Widgets.ButtonInvisible(rowRect))
                {
                    Select(apparel);
                    selected = true;
                }
                curY += height;
            }
            Widgets.EndScrollView();
            if (selected)
                Find.WindowStack.TryRemove(this);
        }

        public override void OnAcceptKeyPressed()
        {
            if (!Filtered.EnumerableNullOrEmpty())
                Select(Filtered.First());
            Find.WindowStack.TryRemove(this);
        }

        void Select(ThingDef apparel)
        {
            var transmog = new TransmogApparel
            {
                Pawn = pawn,
                ApparelDef = apparel,
                StyleDef = null,
                Color = Color.white
            };
            pawn.Preset().Add(transmog);
            Find.WindowStack.Add(new Dialog_EditTransmog(transmog));
        }
    }
}
