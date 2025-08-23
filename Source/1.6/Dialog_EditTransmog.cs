using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace Transmog;

public class Dialog_EditTransmog : Window
{
    private readonly TransmogApparel _transmog;
    private static bool AlphaChannelEnabled => TransmogMod.settings.AlphaChannelEnabled;
    private static int MaxLength => AlphaChannelEnabled ? 8 : 6;
    public override Vector2 InitialSize => new Vector2(360, AlphaChannelEnabled ? 376 : 336);
    private string _hexCode;
    private bool _focused;

    public Dialog_EditTransmog(TransmogApparel transmog)
    {
        preventCameraMotion = false;
        draggable = true;
        doCloseX = true;
        _transmog = transmog;
        _hexCode = transmog.Color.ToString(AlphaChannelEnabled);
    }

    public override void OnAcceptKeyPressed()
    {
        base.OnAcceptKeyPressed();
        if (_hexCode.Length == 6)
        {
            _transmog.Color = _hexCode.ToColor();
        }
    }

    private static readonly List<FloatMenuOption> StyleMenu = new List<FloatMenuOption>();
    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        const int gap = 4;
        const int height = 32;
        const int fullWidth = 320;

        const int iconSize = 128;
        Rect iconRect = new Rect(inRect.x, inRect.y, iconSize, iconSize);
        Widgets.ThingIcon(iconRect, _transmog.GetApparel());

        const int colorTypeWidth = 160;
        const int colorTypeHeight = 32;
        const int colorTypeXGap = 160;
        const int colorTypeYGap = 16;

        Rect favoriteColorRect = new Rect(inRect.x + colorTypeXGap, inRect.y, colorTypeWidth, colorTypeHeight);
        if (ModsConfig.IdeologyActive && Widgets.RadioButtonLabeled(favoriteColorRect, "Transmog.SetFavoriteColor".Translate(), _transmog.FavoriteColor))
        {
            _transmog.FavoriteColor ^= true;
        }

        Rect ideoColorRect = new Rect(inRect.x + colorTypeXGap, favoriteColorRect.y + favoriteColorRect.height + colorTypeYGap, colorTypeWidth, colorTypeHeight);
        if (ModsConfig.IdeologyActive && Widgets.RadioButtonLabeled(ideoColorRect, "Transmog.SetIdeoColor".Translate(), _transmog.IdeoColor))
        {
            _transmog.IdeoColor ^= true;
        }

        Rect customColorRect = new Rect(inRect.x + colorTypeXGap, ideoColorRect.y + ideoColorRect.height + colorTypeYGap, colorTypeWidth, colorTypeHeight);
        if (Widgets.RadioButtonLabeled(customColorRect, "Transmog.SetCustomColor".Translate(), !(_transmog.FavoriteColor || _transmog.IdeoColor)))
        {
            _transmog.FavoriteColor = _transmog.IdeoColor = false;
        }

        Rect styleButtonRect = new Rect(inRect.x, iconRect.y + iconRect.height + gap, fullWidth, height);
        if (_transmog.ApparelDef.GetStyles().Any() && Widgets.ButtonText(styleButtonRect, $"{"Style".Translate()} {_transmog.StyleDef?.Category?.LabelCap ?? _transmog.StyleDef?.defName ?? "None".Translate()}".StripTags().Truncate(styleButtonRect.width)))
        {
            StyleMenu.Clear();

            for (int i = 0; i < _transmog.ApparelDef.GetStyles().Count; i++)
            {
                ThingStyleDef styleDef = _transmog.ApparelDef.GetStyles()[i];
                StyleMenu.Add(new FloatMenuOption(styleDef?.Category?.LabelCap ?? styleDef?.defName ?? "None".Translate(), () => _transmog.StyleDef = styleDef));
            }

            Find.WindowStack.Add(new FloatMenu(StyleMenu));
        }

        Rect hexCodeLabelRect = new Rect(inRect.x, styleButtonRect.y + styleButtonRect.height + gap + 5, 12, 22);
        Widgets.Label(hexCodeLabelRect, "#");
        GUI.SetNextControlName("Hexcode");

        Rect hexCodeTextRect = new Rect(hexCodeLabelRect.width, styleButtonRect.y + styleButtonRect.height + gap, MaxLength * 11, height);
        _hexCode = Widgets.TextField(hexCodeTextRect, _hexCode, MaxLength, new Regex("^[0-9a-fA-F]*$"));
        if (!_focused)
        {
            UI.FocusControl("Hexcode", this);
            _focused = true;
        }
        if (_hexCode.Length == MaxLength)
        {
            _transmog.Color = _hexCode.ToColor();
        }

        Rect confirmButtonRect = new Rect(inRect.width / 2, styleButtonRect.y + styleButtonRect.height + gap, 160, height);
        if (Widgets.ButtonText(confirmButtonRect, "Confirm".Translate()))
        {
            Find.WindowStack.TryRemove(this);
        }

        Color color = _transmog.Color;
        const int colorSliderGap = 8;

        Rect rRect = new Rect(inRect.x, confirmButtonRect.y + confirmButtonRect.height + colorSliderGap, fullWidth, height);
        color.r = Widgets.HorizontalSlider(rRect, color.r, 0, 1);

        Rect gRect = new Rect(inRect.x, rRect.y + rRect.height + colorSliderGap, fullWidth, height);
        color.g = Widgets.HorizontalSlider(gRect, color.g, 0, 1);

        Rect bRect = new Rect(inRect.x, gRect.y + gRect.height + colorSliderGap, fullWidth, height);
        color.b = Widgets.HorizontalSlider(bRect, color.b, 0, 1);

        if (AlphaChannelEnabled)
        {
            Rect aRect = new Rect(inRect.x, bRect.y + bRect.height + colorSliderGap, fullWidth, height);
            color.a = Widgets.HorizontalSlider(aRect, color.a, 0, 1);
        }

        if (color != _transmog.Color)
        {
            _hexCode = color.ToString(AlphaChannelEnabled);
        }
    }
}