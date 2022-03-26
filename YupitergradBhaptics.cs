using System;

using MelonLoader;
using HarmonyLib;
using MyBhapticsTactsuit;
using Game.Scripts.GrapplingHook;
using Game.Scripts.Player;

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
            public static void Postfix(GrapplingHook __instance)
            {
                if (tactsuitVr.suitDisabled) return;

                string hand = (__instance.name == "GraplingHookLeft") ? "L" : "R";
                tactsuitVr.PlaybackHaptics("GrapplingVest_"+hand);
                tactsuitVr.PlaybackHaptics("Grappling_"+hand);
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), "OnCollisionEnter")]
        public class bhaptics_OnCollisionEnter
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerPhysics __instance)
            {
                if (tactsuitVr.suitDisabled) return;

                if (__instance.Velocity.magnitude > 10f)
                {
                    float intensity = (float)Math.Log10((double)(__instance.Velocity.magnitude - 10));
                    tactsuitVr.LOG("INTENSITY "+ intensity);
                    tactsuitVr.PlaybackHaptics("ImpactShort", intensity);
                }
            }
        }
    }
}
