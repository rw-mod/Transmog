using RimWorld;
using UnityEngine;
using Verse;

namespace Transmog
{
    public class TransmogApparel : IExposable
    {
        Pawn pawn;
        ThingDef apparelDef;
        ThingStyleDef styleDef;
        bool favoriteColor = false;
        bool ideoColor = false;
        Color color;

        public override bool Equals(object obj) =>
            obj is TransmogApparel transmog
            && apparelDef == transmog.apparelDef
            && styleDef == transmog.styleDef
            && favoriteColor == transmog.favoriteColor
            && ideoColor == transmog.ideoColor
            && color == transmog.color;

        public Pawn Pawn
        {
            get => pawn;
            set => pawn = value;
        }
        public ThingDef ApparelDef
        {
            get => apparelDef;
            set
            {
                if (apparelDef == value)
                    return;
                apparelDef = value;
                Update();
            }
        }
        public ThingStyleDef StyleDef
        {
            get => styleDef;
            set
            {
                if (styleDef == value)
                    return;
                styleDef = value;
                Update();
            }
        }
        public bool FavoriteColor
        {
            get => favoriteColor;
            set
            {
                if (favoriteColor == value)
                    return;
                favoriteColor = value;
                if (value)
                    ideoColor = false;
                Update();
            }
        }
        public bool IdeoColor
        {
            get => ideoColor;
            set
            {
                if (ideoColor == value)
                    return;
                ideoColor = value;
                if (value)
                    favoriteColor = false;
                Update();
            }
        }
        public Color Color
        {
            get => color;
            set
            {
                if (color == value)
                    return;
                color = value;
                Update();
            }
        }

        Apparel apparelCached;

        public Apparel GetApparel()
        {
            if (apparelCached == null)
            {
                apparelCached = (Apparel)ThingMaker.MakeThing(apparelDef, GenStuff.DefaultStuffFor(apparelDef));
                apparelCached.SetStyleDef(styleDef);
                apparelCached.SetColor(
                    ideoColor
                        ? pawn.Ideo.Color
                        : favoriteColor
                            ? pawn.story.favoriteColor ?? default
                            : color,
                    false
                );
            }
            return apparelCached;
        }

        public TransmogApparel DuplicateForPawn(Pawn pawn) =>
            new TransmogApparel
            {
                Pawn = pawn,
                ApparelDef = ApparelDef,
                StyleDef = StyleDef,
                FavoriteColor = FavoriteColor,
                IdeoColor = IdeoColor,
                Color = Color
            };

        void Update()
        {
            apparelCached = null;
            Pawn.Preset().Update();
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref apparelDef, "apparelDef");
            Scribe_Defs.Look(ref styleDef, "styleDef");
            Scribe_Values.Look(ref favoriteColor, "favoriteColor");
            Scribe_Values.Look(ref ideoColor, "ideoColor");
            Scribe_Values.Look(ref color, "Color");
        }
    }
}
