using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace Transmog;

public class Dialog_EditTransmog : Window
{
    private readonly TransmogApparel _transmog;
    private static bool AlphaChannelEnabled => Transmog.settings.AlphaChannelEnabled;
    private static int MaxLength => AlphaChannelEnabled ? 8 : 6;
    public override Vector2 InitialSize => new Vector2(360, AlphaChannelEnabled ? 384 : 336);
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

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        Rect iconRect = new Rect(inRect.x, inRect.y, 128, 128);
        Rect styleRect = new Rect(inRect.x + 112, inRect.y + 112, 16, 16);
        Rect favoriteColorRect = new Rect(inRect.x + 160, inRect.y, 160, 32);
        Rect ideoColorRect = new Rect(inRect.x + 160, inRect.y + 48, 160, 32);
        Rect customColorRect = new Rect(inRect.x + 160, inRect.y + 96, 160, 32);
        Rect hexCodeLabelRect = new Rect(inRect.x, inRect.y + 144 + 5, 12, 22);
        Rect hexCodeTextRect = new Rect(inRect.x + 12, inRect.y + 144, MaxLength * 11, 32);
        Rect confirmButtonRect = new Rect(inRect.x + 160, inRect.y + 144, 160, 32);
        Rect rRect = new Rect(inRect.x, inRect.y + 192, 320, 32);
        Rect gRect = new Rect(inRect.x, inRect.y + 240, 320, 32);
        Rect bRect = new Rect(inRect.x, inRect.y + 288, 320, 32);
        Rect aRect = new Rect(inRect.x, inRect.y + 336, 320, 32);

        Widgets.ThingIcon(iconRect, _transmog.GetApparel());
        if (_transmog.ApparelDef.GetStyles().Count > 1 && Widgets.ButtonImage(styleRect, TexButton.SelectOverlappingNext))
        {
            Find.WindowStack.Add(
                new FloatMenu(
                    _transmog
                        .ApparelDef.GetStyles()
                        .Select(style => new FloatMenuOption(style?.Category?.LabelCap ?? style?.defName ?? "None".Translate(), () => _transmog.StyleDef = style))
                        .ToList()
                )
            );
        }

        if (ModsConfig.IdeologyActive && Widgets.RadioButtonLabeled(favoriteColorRect, "Transmog.SetFavoriteColor".Translate(), _transmog.FavoriteColor))
        {
            _transmog.FavoriteColor ^= true;
        }
        if (ModsConfig.IdeologyActive && Widgets.RadioButtonLabeled(ideoColorRect, "Transmog.SetIdeoColor".Translate(), _transmog.IdeoColor))
        {
            _transmog.IdeoColor ^= true;
        }
        if (Widgets.RadioButtonLabeled(customColorRect, "Transmog.SetCustomColor".Translate(), !(_transmog.FavoriteColor || _transmog.IdeoColor)))
        {
            _transmog.FavoriteColor = _transmog.IdeoColor = false;
        }
        Widgets.Label(hexCodeLabelRect, "#");
        GUI.SetNextControlName("Hexcode");
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
        if (Widgets.ButtonText(confirmButtonRect, "Confirm".Translate()))
        {
            Find.WindowStack.TryRemove(this);
        }

        Color color = _transmog.Color;
        color.r = Widgets.HorizontalSlider(rRect, color.r, 0, 1);
        color.g = Widgets.HorizontalSlider(gRect, color.g, 0, 1);
        color.b = Widgets.HorizontalSlider(bRect, color.b, 0, 1);
        if (AlphaChannelEnabled)
        {
            color.a = Widgets.HorizontalSlider(aRect, color.a, 0, 1);
        }
        if (color != _transmog.Color)
        {
            _hexCode = color.ToString(AlphaChannelEnabled);
        }
    }
}