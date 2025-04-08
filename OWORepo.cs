using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using static HurtCollider;
using static SemiFunc;
using System.Collections.Generic;
using static RunManager;


namespace OWO_REPO
{
    [BepInPlugin("org.bepinex.plugins.OWO_REPO", "OWO_REPO", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        #pragma warning disable CS0109
        internal static new ManualLogSource Log;
        #pragma warning restore CS0109

        public static OWOSkin owoSkin;
        public static OWOInteractables interactables;        
        public static float explosionDistance = 10;
        public static string lastPlayerState = "";        

        private void Awake()
        {
            Log = Logger;
            Logger.LogMessage("OWO_REPO plugin is loaded!");
            owoSkin = new OWOSkin();
            interactables = new OWOInteractables(owoSkin, explosionDistance);

            var harmony = new Harmony("owo.patch.repo");
            harmony.PatchAll();
        }

        public static bool IsLocalPlayerNear(float range, Vector3 position)
        {
            foreach (PlayerAvatar playerAvatar in GameDirector.instance.PlayerList)
            {
                if (Traverse.Create(playerAvatar).Field("IsLocal").GetValue<bool>())
                {
                    Vector3 position2 = playerAvatar.PlayerVisionTarget.VisionTransform.position;
                    float num = Vector3.Distance(position, position2);
                    if (num > range)
                    {
                        return false;
                    }
                    return true;
                }
                continue;
            }
            return false;
        }

        #region Player

        #region PlayerController

        [HarmonyPatch(typeof(PlayerController), "ChangeState")]
        public class OnChangeState
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerController __instance)
            {
                if (!owoSkin.CanFeel()) return;

                if ( __instance.Crouching && lastPlayerState != "crouching")
                {
                    lastPlayerState = "crouching";
                    owoSkin.Feel("Crouch", 2);
                }

                if (!__instance.Crouching && !__instance.Crawling) lastPlayerState = "standing";

                //if (__instance.sprinting) owoSkin.LOG($"PlayerController ChangeState - sprinting");
                //if (__instance.Crawling) owoSkin.LOG($"PlayerController ChangeState - Crawling");
                //if (__instance.Sliding) owoSkin.LOG($"PlayerController ChangeState - Sliding");
                //if (__instance.moving) owoSkin.LOG($"PlayerController ChangeState - moving");
            }
        }
        
        [HarmonyPatch(typeof(PlayerController), "Revive")]
        public class OnRevive
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerController __instance)
            {
                bool isMine = __instance.playerAvatar.GetComponent<PlayerAvatar>().photonView.IsMine;

                if (!owoSkin.playing && (isMine|| !GameManager.Multiplayer()))
                {
                    owoSkin.playing = true;
                    
                    owoSkin.Feel("Revive", 3);
                }
            }
        }

        #endregion

        #region PlayerHealth

        [HarmonyPatch(typeof(PlayerHealth), "Hurt")]
        public class OnHurt
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerHealth __instance, int damage, bool savingGrace, int enemyIndex = -1)
            {
                if (!owoSkin.CanFeel()) return; // No sentimos si no estamos jugando

                PhotonView photonView = Traverse.Create(__instance).Field("photonView").GetValue<PhotonView>();
                if (damage > 0 && (photonView.IsMine || !GameManager.Multiplayer()))
                {
                    owoSkin.Feel("Hurt", 3, Mathf.Clamp((damage/70*100),30,100));
                    //owoSkin.LOG($"Hurt: {damage} - SavingGrace: {savingGrace} - EnemyIndex: {enemyIndex}");
                }
            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "HurtOther")]
        public class OnHurtOther
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerHealth __instance,int damage, Vector3 hurtPosition, bool savingGrace, int enemyIndex = -1)
            {
                if (!owoSkin.CanFeel() && !GameManager.Multiplayer()) return;

                PhotonView photonView = Traverse.Create(__instance).Field("photonView").GetValue<PhotonView>();
                if (photonView.IsMine)
                {
                    owoSkin.Feel("Hurt", 3, 30);                    
                }
            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "Heal")]
        public class OnHeal
        {
            [HarmonyPostfix]
            public static void Postfix(int healAmount, bool effect = true)
            {
                if (!owoSkin.CanFeel()) return;

                owoSkin.Feel("Heal", 2);                
                //owoSkin.LOG($"Playerhealth Heal - HealAmount: {healAmount} - Effect: {effect}");
            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "Death")]
        public class OnDeath
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerHealth __instance)
            {
                PlayerAvatar player = Traverse.Create(__instance).Field("playerAvatar").GetValue<PlayerAvatar>();                

                if (owoSkin.CanFeel() && (player.photonView.IsMine || !GameManager.Multiplayer()))
                {
                    owoSkin.playing = false;

                    owoSkin.StopAllHapticFeedback();
                    owoSkin.Feel("Death", 4);
                }
            }
        }
        #endregion

        [HarmonyPatch(typeof(CameraJump), "Jump")]
        public class OnJump
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.CanFeel()) return;

                owoSkin.Feel("Jump", 2);
            }
        }

        [HarmonyPatch(typeof(CameraJump), "Land")]
        public class OnLand
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.CanFeel()) return;

                owoSkin.Feel("Landing", 2);
            }
        }


        #endregion

        #region Items

        [HarmonyPatch(typeof(CameraGlitch), "PlayUpgrade")]
        public class OnPlayUpgrade
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.CanFeel()) return;

                owoSkin.Feel("Upgrade", 2);
            }
        }

        #endregion

        #region GrabBeam

        [HarmonyPatch(typeof(PhysGrabber), "PhysGrabStartEffects")]
        public class OnPhysGrabStartEffects
        {
            [HarmonyPostfix]
            public static void Postfix(PhysGrabber __instance)
            {
                PhysGrabObject grabbedPhysGrabObject = Traverse.Create(__instance).Field("grabbedPhysGrabObject").GetValue<PhysGrabObject>();
                //owoSkin.LOG($"GrabBeamStart {__instance.gameObject.name} - {grabbedPhysGrabObject.rb.mass}");

                if (__instance.isLocal && owoSkin.CanFeel()) 
                {
                        owoSkin.BeamIntensity(grabbedPhysGrabObject ? grabbedPhysGrabObject.rb.mass : 0.5f);
                        owoSkin.StartBeam();
                        //owoSkin.LOG($"PhysGrabber PhysGrabStartEffects - {componentInChildren.itemVolume}");
                }
            }
        }

        [HarmonyPatch(typeof(PhysGrabber), "PhysGrabEndEffects")]
        public class OnPhysGrabEndEffects
        {
            [HarmonyPostfix]
            public static void Postfix(PhysGrabber __instance)
            {
                if(__instance.isLocal && owoSkin.CanFeel()) owoSkin.StopBeam();
                    
                //owoSkin.LOG($"PhysGrabber PhysGrabEndEffects");
            }
        }
        #endregion

        #region Start/Stop Feeling Logic

        [HarmonyPatch(typeof(RunManager), "ChangeLevel")]
        public class OnChangeLevel
        {
            [HarmonyPostfix]
            public static void Postfix(RunManager __instance)
            {
                if (!owoSkin.suitEnabled) return;

                //owoSkin.LOG($"#### Level: {__instance.levelCurrent}");

                if (__instance.levelCurrent != __instance.levelLobbyMenu && !owoSkin.playing)
                {
                    owoSkin.playing = true;
                    //owoSkin.LOG($"<YOU CAN FEEL NOW>");
                }
                else if(owoSkin.playing)
                {
                    owoSkin.playing = false;
                    owoSkin.StopAllHapticFeedback();
                    //owoSkin.LOG($"<STOP FEELING THE GAME>");
                }
            }
        }

        [HarmonyPatch(typeof(RunManager), "UpdateLevel")]
        public class OnUpdateLevel
        {
            [HarmonyPostfix]
            public static void Postfix(RunManager __instance)
            {
                if (!owoSkin.suitEnabled) return;

                //owoSkin.LOG($"#### Level: {__instance.levelCurrent}");

                if (__instance.levelCurrent != __instance.levelLobbyMenu && !owoSkin.playing)
                {
                    owoSkin.playing = true;
                    //owoSkin.LOG($"<YOU CAN FEEL NOW>");
                }
                else if (owoSkin.playing)
                {
                    owoSkin.playing = false;
                    owoSkin.StopAllHapticFeedback();
                    //owoSkin.LOG($"<STOP FEELING THE GAME>");
                }
            }
        }

        [HarmonyPatch(typeof(RunManager), "LeaveToMainMenu")]
        public class OnLeaveToMainMenu
        {
            [HarmonyPostfix]
            public static void Postfix(RunManager __instance)
            {
                if (!owoSkin.suitEnabled || !owoSkin.playing) return;
                
                owoSkin.playing = false;
                owoSkin.StopAllHapticFeedback();
            }
        }

        #endregion

        //#region Laser

        //[HarmonyPatch(typeof(SemiLaser), "LaserActive")]
        //public class OnLaserActive
        //{
        //    [HarmonyPostfix]
        //    public static void PostFix(SemiLaser __instance, Vector3 _startPosition, Vector3 _endPosition, bool _isHitting)
        //    {
        //        if(IsLocalPlayerNear(explosionDistance, _startPosition) || IsLocalPlayerNear(explosionDistance, _endPosition))
        //            owoSkin.LOG($"SemiLaser LaserActive");
        //    }
        //}

        //#endregion


    }
}
