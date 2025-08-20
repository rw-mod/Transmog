using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Transmog;

public class CompProperties_Transmog : CompProperties
{
    public CompProperties_Transmog() => compClass = typeof(CompTransmog);
}

public class CompTransmog : ThingComp
{
    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            Update();
        }
    }
    private bool _enabled;

    public bool DraftedTransmogEnabled
    {
        get => _draftedTransmogEnabled;
        set
        {
            _draftedTransmogEnabled = value;
            Update();
        }
    }
    private bool _draftedTransmogEnabled;

    public List<TransmogApparel> Transmog
    {
        get => DraftedTransmogEnabled && Pawn.Drafted ? _draftedTransmog : _transmog;
        private set
        {
            if (DraftedTransmogEnabled && Pawn.Drafted)
            {
                _draftedTransmog = value;
            }
            else
            {
                _transmog = value;
            }
        }
    }

    public Stack<List<TransmogApparel>> History => DraftedTransmogEnabled && Pawn.Drafted ? _draftedHistory : _history;

    private List<TransmogApparel> _transmog = new List<TransmogApparel>();
    private readonly Stack<List<TransmogApparel>> _history = new Stack<List<TransmogApparel>>();

    private List<TransmogApparel> _draftedTransmog = new List<TransmogApparel>();
    private readonly Stack<List<TransmogApparel>> _draftedHistory = new Stack<List<TransmogApparel>>();
    private Pawn Pawn => parent as Pawn;
    public List<Apparel> Apparel => Transmog.Select(transmog => transmog.GetApparel()).ToList();

    private void Save() => History.Push(Transmog.Select(transmog => transmog.DuplicateForPawn(Pawn)).ToList());

    public void CopyFromApparel()
    {
        IEnumerable<TransmogApparel> newTransmog = Pawn.apparel.WornApparel.Select(apparel =>
            new TransmogApparel
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
        _enabled = true;
        Update();
    }

    public void CopyFromPreset(List<TransmogApparel> preset)
    {
        IEnumerable<TransmogApparel> newTransmog = preset.Where(apparel => apparel.ApparelDef?.apparel.PawnCanWear(Pawn) ?? false).Select(apparel => apparel.DuplicateForPawn(Pawn));
        if (!Transmog.SequenceEqual(newTransmog))
        {
            Save();
            Transmog = newTransmog.ToList();
        }
        _enabled = true;
        Update();
    }

    public void TryRevert()
    {
        if (History.Count == 0) return;
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

    public void MoveUp(int index)
    {
        Transmog.Reverse(index - 1, 2);
        Update();
    }

    public void Update() => Pawn.apparel.Notify_ApparelChanged();

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref _enabled, "transmogEnabled");
        Scribe_Values.Look(ref _draftedTransmogEnabled, "draftedTransmogEnabled");
        Scribe_Collections.Look(ref _transmog, "transmog");
        Scribe_Collections.Look(ref _draftedTransmog, "draftedTransmog");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            _transmog ??= new List<TransmogApparel>();
            _draftedTransmog ??= new List<TransmogApparel>();
            _transmog.ForEach(transmog => transmog.Pawn = Pawn);
            _draftedTransmog.ForEach(transmog => transmog.Pawn = Pawn);
        }
    }
}