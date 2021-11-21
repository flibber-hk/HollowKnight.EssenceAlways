using System;
using System.Collections.Generic;
using Modding;
using MonoMod.Cil;

namespace EssenceAlways
{
    public class GS { public bool? EmissionState = true; }

    public class EssenceAlways : Mod, IGlobalSettings<GS>, IMenuMod
    {
        public static GS GS { get; set; } = new GS();
        public GS OnSaveGlobal() => GS;
        public void OnLoadGlobal(GS s) => GS = s;


        internal static EssenceAlways instance;
        
        public EssenceAlways() : base(null)
        {
            instance = this;
        }
        
		public override string GetVersion()
		{
			return "1.0";
		}

        public override int LoadPriority()
        {
            return -10000;
        }

        public override void Initialize()
        {
            Log("Initializing Mod...");

            CheckShouldEmitEssence += RespectSetting;
        }

        private bool RespectSetting(bool orig) => GS.EmissionState ?? orig;

        private void ShouldEmitEssence(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(UnityEngine.Random), "Range")))
            {
                cursor.EmitDelegate<Func<int, int>>(x => _checkShouldEmitEssence.Invoke(x == 0) ? 0 : 1);
            }
        }

        private event Func<bool, bool> _checkShouldEmitEssence;
        public event Func<bool, bool> CheckShouldEmitEssence
        {
            add
            {
                if (_checkShouldEmitEssence == null) IL.EnemyDeathEffects.EmitEssence += ShouldEmitEssence;
                _checkShouldEmitEssence += value;
            }
            remove
            {
                _checkShouldEmitEssence -= value;
                if (_checkShouldEmitEssence == null) IL.EnemyDeathEffects.EmitEssence -= ShouldEmitEssence;
            }
        }

        public bool ToggleButtonInsideMenu => false;
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? _)
        {
            return new List<IMenuMod.MenuEntry>()
            {
                new IMenuMod.MenuEntry
                {
                    Name = "Essence emission",
                    Description = String.Empty,
                    Values = new[]{"Always", "Never", "Default" },
                    Saver = x => GS.EmissionState = x switch { 0 => true, 1 => false, _ => null },
                    Loader = () => GS.EmissionState switch { true => 0, false => 1, null => 2 }
                }
            };
        }
    }
}