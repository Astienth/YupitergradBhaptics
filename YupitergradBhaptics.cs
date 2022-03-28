using System;

using MelonLoader;
using HarmonyLib;
using MyBhapticsTactsuit;
using Game.Scripts.GrapplingHook;
using Game.Scripts.Player;
using Game.Scripts.Death;

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
                tactsuitVr.PlaybackHaptics("GrapplingArms_"+hand);
            }
        }
        
        [HarmonyPatch(typeof(GrapplingHook), "GrabUsed", new Type[] { })]
        public class bhaptics_GrabUsed
        {
            [HarmonyPostfix]
            public static void Postfix(GrapplingHook __instance)
            {
                if (tactsuitVr.suitDisabled) return;

                if (__instance.name == "GraplingHookLeft")
                {
                    tactsuitVr.StartGrabUsedLeft();
                }
                else
                {
                    tactsuitVr.StartGrabUsedRight();
                }
            }
        }

        [HarmonyPatch(typeof(GrapplingHook), "GrabUnused", new Type[] { })]
        public class bhaptics_GrabUnused
        {
            [HarmonyPostfix]
            public static void Postfix(GrapplingHook __instance)
            {
                if (tactsuitVr.suitDisabled) return;

                if (__instance.name == "GraplingHookLeft")
                {
                    tactsuitVr.StopGrabUsedLeft();
                }
                else
                {
                    tactsuitVr.StopGrabUsedRight();
                }
            }
        }

        [HarmonyPatch(typeof(GrapplingHook), "FixedUpdate")]
        public class bhaptics_FixedUpdate
        {
            [HarmonyPostfix]
            public static void Postfix(GrapplingHook __instance)
            {
                if (tactsuitVr.suitDisabled) return;

                if (__instance.m_playerPhysics.Velocity.magnitude > 20f)
                {
                    tactsuitVr.StartThrust();
                }
                else
                {
                    tactsuitVr.StopThrust();
                }
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
                    tactsuitVr.PlaybackHaptics("ImpactShort", intensity);
                }
            }
        }

        [HarmonyPatch(typeof(DeathReasonDisplay), "SetDeathReason")]
        public class bhaptics_SetDeathReason
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (tactsuitVr.suitDisabled) return;

                tactsuitVr.PlaybackHaptics("Death");
                tactsuitVr.StopThreads();
            }
        }
    }
}
