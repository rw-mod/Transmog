using RimWorld;
using UnityEngine;
using Verse;

namespace Transmog
{
    class Dialog_SavePreset : Window
    {
        readonly CompTransmog preset;

        public override Vector2 InitialSize => new Vector2(280, 175);

        public Dialog_SavePreset(CompTransmog preset)
        {
            forcePause = true;
            closeOnAccept = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            this.preset = preset;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                flag = true;
                Event.current.Use();
            }
            Rect rect = new Rect(inRect);
            Text.Font = GameFont.Medium;
            rect.height = Text.LineHeight + 10f;
            Widgets.Label(rect, "Transmog.PresetName".Translate());
            Text.Font = GameFont.Small;
            name = Widgets.TextField(new Rect(0f, rect.height, inRect.width, 35f), name);
            if (Widgets.ButtonText(new Rect(15f, inRect.height - 35f - 10f, inRect.width - 15f - 15f, 35f), "Confirm".Translate()) || flag)
            {
                PresetManager.AddPreset(name, preset);
                Find.WindowStack.TryRemove(this);
            }
        }

        public string name;
    }
}
