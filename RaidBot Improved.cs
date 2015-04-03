using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using CommonBehaviors.Actions;
using JetBrains.Annotations;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.Profiles;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals;

namespace RaidBot_Improved
{
    [UsedImplicitly]
    public class RaidBotImproved : BotBase
    {
        private Composite _root;
        public override string Name { get { return "RaidBot Improved"; } }
        public override Composite Root { get { return _root ?? (_root = new ActionRunCoroutine(ret => CreateRootBehavior())); } }
        public override PulseFlags PulseFlags { get { return PulseFlags.All & ~(PulseFlags.Targeting | PulseFlags.Looting); } }
        private bool IsPaused { get; set; }

        public override void Start()
        {
            TreeRoot.TicksPerSecond = 30;
            ProfileManager.LoadEmpty();

            HotkeysManager.Register("RaidBot Pause", Keys.X, ModifierKeys.Alt, hk =>
            {
                IsPaused = !IsPaused;
                if (IsPaused)
                {
                    StyxWoW.Overlay.AddToast(() => string.Format("RaidBot Paused!"),
                                               TimeSpan.FromSeconds(1.5),
                                               // Foreground Color, Background Color
                                               Colors.DarkOrange, Colors.Black,
                                               new FontFamily("Segoe UI"));
                    // Make the bot use less resources while paused.
                    TreeRoot.TicksPerSecond = 5;
                }
                else
                {
                    StyxWoW.Overlay.AddToast(() => string.Format("RaidBot Resumed!"),
                                               TimeSpan.FromSeconds(1.5),
                                               // Foreground Color, Background Color
                                               Colors.DarkOrange, Colors.Black,
                                               new FontFamily("Segoe UI"));
                    // Kick it back into overdrive!
                    TreeRoot.TicksPerSecond = 30;
                }
            });
        }

        private async Task<bool> CreateRootBehavior()
        {
            if (IsPaused)
                return true;

            if (!StyxWoW.Me.Combat)
                await RoutineManager.Current.PreCombatBuffBehavior.ExecuteCoroutine();

            if (!StyxWoW.Me.Combat) return false;

            await RoutineManager.Current.HealBehavior.ExecuteCoroutine();

            if (!StyxWoW.Me.GotTarget || StyxWoW.Me.CurrentTarget.IsDead) return false;

            await RoutineManager.Current.CombatBuffBehavior.ExecuteCoroutine();
            await RoutineManager.Current.CombatBehavior.ExecuteCoroutine();

            return false;
        }
    }
}
