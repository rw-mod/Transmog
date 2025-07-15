using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Transmog
{
    class CompProperties_Transmog : CompProperties
    {
        public CompProperties_Transmog() => compClass = typeof(CompTransmog);
    }

    class CompTransmog : ThingComp
    {
        bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                Update();
            }
        }

        bool draftedTransmogEnabled;
        public bool DraftedTransmogEnabled
        {
            get => draftedTransmogEnabled;
            set
            {
                draftedTransmogEnabled = value;
                Update();
            }
        }

        public List<TransmogApparel> Transmog
        {
            get => DraftedTransmogEnabled && Pawn.Drafted ? draftedTransmog : transmog;
            set
            {
                if (DraftedTransmogEnabled && Pawn.Drafted)
                    draftedTransmog = value;
                else
                    transmog = value;
            }
        }
        public Stack<List<TransmogApparel>> History => DraftedTransmogEnabled && Pawn.Drafted ? draftedHistory : history;

        List<TransmogApparel> transmog = new List<TransmogApparel>();
        Stack<List<TransmogApparel>> history = new Stack<List<TransmogApparel>>();

        List<TransmogApparel> draftedTransmog = new List<TransmogApparel>();
        Stack<List<TransmogApparel>> draftedHistory = new Stack<List<TransmogApparel>>();
        Pawn Pawn => parent as Pawn;
        public List<Apparel> Apparel => Transmog.Select(transmog => transmog.GetApparel()).ToList();

        public void Save() => History.Push(Transmog.Select(transmog => transmog.DuplicateForPawn(Pawn)).ToList());

        public void CopyFromApparel()
        {
            var newTransmog = Pawn.apparel.WornApparel.Select(
                apparel =>
                    new TransmogApparel()
                    {
                        Pawn = Pawn,
                        ApparelDef = apparel.def,
                        StyleDef = apparel.StyleDef,
                        Color = apparel.DrawColor
                    }
            );
            if (!Transmog.SequenceEqual(newTransmog))
            {
                Save();
                Transmog = newTransmog.ToList();
            }
            enabled = true;
            Update();
        }

        public void CopyFromPreset(List<TransmogApparel> preset)
        {
            var newTransmog = preset.Where(apparel => apparel.ApparelDef?.apparel.PawnCanWear(Pawn) ?? false).Select(apparel => apparel.DuplicateForPawn(Pawn));
            if (!Transmog.SequenceEqual(newTransmog))
            {
                Save();
                Transmog = newTransmog.ToList();
            }
            enabled = true;
            Update();
        }

        public void TryRevert()
        {
            if (History.Count == 0)
                return;
            Transmog = History.Pop();
            Update();
        }

        public void Add(TransmogApparel transmog)
        {
            Save();
            Transmog.Add(transmog);
            Update();
        }

        public void RemoveAt(int index)
        {
            Save();
            Transmog.RemoveAt(index);
            Update();
        }

        public void Moveup(int index)
        {
            Transmog.Reverse(index - 1, 2);
            Update();
        }

        public void Update() => Pawn.apparel.Notify_ApparelChanged();

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref enabled, "transmogEnabled");
            Scribe_Values.Look(ref draftedTransmogEnabled, "draftedTransmogEnabled");
            Scribe_Collections.Look(ref transmog, "transmog");
            Scribe_Collections.Look(ref draftedTransmog, "draftedTransmog");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (transmog == null)
                    transmog = new List<TransmogApparel>();
                if (draftedTransmog == null)
                    draftedTransmog = new List<TransmogApparel>();
                transmog.ForEach(transmog => transmog.Pawn = Pawn);
                draftedTransmog.ForEach(transmog => transmog.Pawn = Pawn);
            }
        }
    }
}
