using System;

using MelonLoader;
using HarmonyLib;
using MyBhapticsTactsuit;
using Game.Scripts.GrapplingHook;

namespace YupitergradBhaptics
{
    public class YupitergradBhaptics : MelonMod
    {
        public static TactsuitVR tactsuitVr;
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }
        
        [HarmonyPatch(typeof(GrapplingHook), "TriggerUsed", new Type[] { })]
        public class bhaptics_TriggerUsed
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("HeartBeat");
            }
        }
    }
}
