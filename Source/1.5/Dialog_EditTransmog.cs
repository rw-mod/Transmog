using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Transmog
{
    class Dialog_EditTransmog : Window
    {
        TransmogApparel transmog;
        public bool AlphaChannelEnabled => Transmog.settings.alphaChannelEnabled;
        public int MaxLength => AlphaChannelEnabled ? 8 : 6;
        public override Vector2 InitialSize => new Vector2(360, AlphaChannelEnabled ? 384 : 336);
        string hexcode;
        bool focused;

        public Dialog_EditTransmog(TransmogApparel transmog)
        {
            preventCameraMotion = false;
            draggable = true;
            doCloseX = true;
            this.transmog = transmog;
            hexcode = transmog.Color.toString(AlphaChannelEnabled);
        }

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();
            if (hexcode.Length == 6)
                transmog.Color = hexcode.toColor();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            var iconRect = new Rect(inRect.x, inRect.y, 128, 128);
            var styleRect = new Rect(inRect.x + 112, inRect.y + 112, 16, 16);
            var favoriteColorRect = new Rect(inRect.x + 160, inRect.y, 160, 32);
            var ideoColorRect = new Rect(inRect.x + 160, inRect.y + 48, 160, 32);
            var customColorRect = new Rect(inRect.x + 160, inRect.y + 96, 160, 32);
            var hexcodeLabelRect = new Rect(inRect.x, inRect.y + 144 + 5, 12, 22);
            var hexcodeTextRect = new Rect(inRect.x + 12, inRect.y + 144, MaxLength * 11, 32);
            var confirmButtonRect = new Rect(inRect.x + 160, inRect.y + 144, 160, 32);
            var rRect = new Rect(inRect.x, inRect.y + 192, 320, 32);
            var gRect = new Rect(inRect.x, inRect.y + 240, 320, 32);
            var bRect = new Rect(inRect.x, inRect.y + 288, 320, 32);
            var aRect = new Rect(inRect.x, inRect.y + 336, 320, 32);

            Widgets.ThingIcon(iconRect, transmog.GetApparel());
            if (transmog.ApparelDef.GetStyles().Count() > 1 && Widgets.ButtonImage(styleRect, TexButton.SelectOverlappingNext))
                Find.WindowStack.Add(
                    new FloatMenu(
                        transmog
                            .ApparelDef.GetStyles()
                            .Select(style => new FloatMenuOption(style?.Category?.LabelCap ?? style?.defName ?? "None".Translate(), () => transmog.StyleDef = style))
                            .ToList()
                    )
                );
            if (ModsConfig.IdeologyActive && Widgets.RadioButtonLabeled(favoriteColorRect, "Transmog.SetFavoriteColor".Translate(), transmog.FavoriteColor))
                transmog.FavoriteColor ^= true;
            if (ModsConfig.IdeologyActive && Widgets.RadioButtonLabeled(ideoColorRect, "Transmog.SetIdeoColor".Translate(), transmog.IdeoColor))
                transmog.IdeoColor ^= true;
            if (Widgets.RadioButtonLabeled(customColorRect, "Transmog.SetCustomColor".Translate(), !(transmog.FavoriteColor || transmog.IdeoColor)))
                transmog.FavoriteColor = transmog.IdeoColor = false;
            Widgets.Label(hexcodeLabelRect, "#");
            GUI.SetNextControlName("Hexcode");
            hexcode = Widgets.TextField(hexcodeTextRect, hexcode, MaxLength, new Regex("^[0-9a-fA-F]*$"));
            if (!focused)
            {
                UI.FocusControl("Hexcode", this);
                focused = true;
            }
            if (hexcode.Length == MaxLength)
                transmog.Color = hexcode.toColor();
            if (Widgets.ButtonText(confirmButtonRect, "Confirm".Translate()))
                Find.WindowStack.TryRemove(this);

            var color = transmog.Color;
            color.r = Widgets.HorizontalSlider(rRect, color.r, 0, 1);
            color.g = Widgets.HorizontalSlider(gRect, color.g, 0, 1);
            color.b = Widgets.HorizontalSlider(bRect, color.b, 0, 1);
            if (AlphaChannelEnabled)
                color.a = Widgets.HorizontalSlider(aRect, color.a, 0, 1);
            if (color != transmog.Color)
                hexcode = color.toString(AlphaChannelEnabled);
        }
    }
}
