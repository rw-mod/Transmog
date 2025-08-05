using RimWorld;
using UnityEngine;
using Verse;

namespace Transmog;

public class TransmogApparel : IExposable
{
    private ThingDef _apparelDef;
    private ThingStyleDef _styleDef;
    private bool _favoriteColor = false;
    private bool _ideoColor = false;
    private Color _color;

    public override bool Equals(object obj) =>
        obj is TransmogApparel transmog
        && _apparelDef == transmog._apparelDef
        && _styleDef == transmog._styleDef
        && _favoriteColor == transmog._favoriteColor
        && _ideoColor == transmog._ideoColor
        && _color == transmog._color;

    public Pawn Pawn { get; set; }
    public ThingDef ApparelDef
    {
        get => _apparelDef;
        set
        {
            if (_apparelDef == value)
                return;
            _apparelDef = value;
            Update();
        }
    }
    public ThingStyleDef StyleDef
    {
        get => _styleDef;
        set
        {
            if (_styleDef == value)
                return;
            _styleDef = value;
            Update();
        }
    }
    public bool FavoriteColor
    {
        get => _favoriteColor;
        set
        {
            if (_favoriteColor == value)
                return;
            _favoriteColor = value;
            if (value)
                _ideoColor = false;
            Update();
        }
    }
    public bool IdeoColor
    {
        get => _ideoColor;
        set
        {
            if (_ideoColor == value)
                return;
            _ideoColor = value;
            if (value)
                _favoriteColor = false;
            Update();
        }
    }
    public Color Color
    {
        get => _color;
        set
        {
            if (_color == value)
                return;
            _color = value;
            Update();
        }
    }

    public Apparel GetApparel()
    {
        if (_apparelCached != null) return _apparelCached;
        
        _apparelCached = (Apparel)ThingMaker.MakeThing(_apparelDef, GenStuff.DefaultStuffFor(_apparelDef));
        _apparelCached.SetStyleDef(_styleDef);

        Color color = _ideoColor ? Pawn.Ideo.Color 
            : _favoriteColor ? Pawn.story.favoriteColor.color 
            : _color;
        _apparelCached.SetColor(color, false);
        _apparelCached.holdingOwner = Pawn.apparel.GetDirectlyHeldThings();
        
        return _apparelCached;
    }
    private Apparel _apparelCached;

    public TransmogApparel DuplicateForPawn(Pawn pawn)
    {
        return new TransmogApparel
        {
            Pawn = pawn,
            ApparelDef = ApparelDef,
            StyleDef = StyleDef,
            FavoriteColor = FavoriteColor,
            IdeoColor = IdeoColor,
            Color = Color
        };
    }
    
    private void Update()
    {
        _apparelCached = null;
        Pawn.Preset().Update();
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref _apparelDef, "apparelDef");
        Scribe_Defs.Look(ref _styleDef, "styleDef");
        Scribe_Values.Look(ref _favoriteColor, "favoriteColor");
        Scribe_Values.Look(ref _ideoColor, "ideoColor");
        Scribe_Values.Look(ref _color, "Color");
    }
}
