using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using static HurtCollider;
using static SemiFunc;
using System.Collections.Generic;


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
        public static float explosionDistance = 20;
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

                if (__instance.sprinting) owoSkin.LOG($"PlayerController ChangeState - sprinting");
                if (__instance.Crawling) owoSkin.LOG($"PlayerController ChangeState - Crawling");
                if (__instance.Sliding) owoSkin.LOG($"PlayerController ChangeState - Sliding");
                if (__instance.moving) owoSkin.LOG($"PlayerController ChangeState - moving");
            }
        }
        
        [HarmonyPatch(typeof(PlayerController), "Revive")]
        public class OnRevive
        {
            [HarmonyPostfix]
            public static void Postfix(Vector3 _rotation)
            {
                owoSkin.LOG($"PlayerController Revive - _rotation: {_rotation}");
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
                if (!owoSkin.CanFeel() || enemyIndex == -1) return; // No sentimos si no estamos jugando

                PhotonView photonView = Traverse.Create(__instance).Field("photonView").GetValue<PhotonView>();
                if (damage > 0 && !PlayerAvatar.instance.photonView.IsMine) 
                {
                    owoSkin.LOG($"Hurt: {damage} - SavingGrace: {savingGrace} - EnemyIndex: {enemyIndex}");
                }

                //owoSkin.LOG($"Playerhealth Hurt - Damage: {damage} - SavingGrace: {savingGrace} - EnemyIndex: {enemyIndex}");
                //owoSkin.LOG($"Playerhealth Hurt - isMine: {photonView.IsMine}");
                //owoSkin.LOG($"Prueba yo - {PlayerAvatar.instance.photonView.IsMine}");
            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "Heal")]
        public class OnHeal
        {
            [HarmonyPostfix]
            public static void Postfix(int healAmount, bool effect = true)
            {
                //if (!owoSkin.CanFeel()) return;
                
                owoSkin.LOG($"Playerhealth Heal - HealAmount: {healAmount} - Effect: {effect}"); //it works!
            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "Death")]
        public class OnDeath
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (owoSkin.CanFeel())
                {
                    owoSkin.playing = false;

                    owoSkin.StopAllHapticFeedback();
                    owoSkin.Feel("Death", 4);
                }

                //owoSkin.LOG($"Playerhealth Death");
            }
        }
        #endregion

        [HarmonyPatch(typeof(PlayerAvatar), "TumbleStart")]
        public class OnTumbleStart
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"PlayerTumble TumbleStart");
            }
        }

        [HarmonyPatch(typeof(PlayerReviveEffects), "Trigger")]
        public class OnTrigger
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"PlayerReviveEffects Trigger");
            }
        }

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
                if (!owoSkin.suitEnabled) return;

                if(owoSkin.playing) owoSkin.Feel("Landing", 2); //Activamos el inicio del juego
                else owoSkin.playing = true;
            }
        }



        #endregion

        #region Items

        [HarmonyPatch(typeof(ItemUpgrade), "PlayerUpgrade")]
        public class OnPlayerUpgrade
        {
            [HarmonyPostfix]
            public static void Postfix(ItemUpgrade __instance)
            {
                owoSkin.LOG($"ItemUpgrade PlayerUpgrade");

                bool upgradeDone = Traverse.Create(__instance).Field("upgradeDone").GetValue<bool>();
                ItemToggle itemToggle = Traverse.Create(__instance).Field("itemToggle").GetValue<ItemToggle>();
                int playerTogglePhotonID = Traverse.Create(itemToggle).Field("playerTogglePhotonID").GetValue<int>();

                if (!upgradeDone)
                {
                    PlayerAvatar playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(playerTogglePhotonID);
                    if (playerAvatar.photonView.IsMine || !GameManager.Multiplayer())
                    {
                        owoSkin.LOG($"YO - ItemUpgrade PlayerUpgrade - YO");
                    }
                }
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
                        owoSkin.BeamIntensity(grabbedPhysGrabObject.rb.mass);
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
                    
                owoSkin.LOG($"PhysGrabber PhysGrabEndEffects");
            }
        }



        #endregion

        #region GameState

        [HarmonyPatch(typeof(GameDirector), "gameStateLoad")]
        public class OngameStateLoad
        {
            [HarmonyPostfix]
            public static void Prefix(GameDirector __instance)
            {
                bool gameStateStartImpulse = Traverse.Create(__instance).Field("gameStateStartImpulse").GetValue<bool>();
                if (gameStateStartImpulse)
                owoSkin.LOG($"GameDirector gameStateLoad");
            }
        }

        [HarmonyPatch(typeof(GameDirector), "gameStateOutro")]
        public class OngameStateOutro
        {
            [HarmonyPostfix]
            public static void Prefix(GameDirector __instance)
            {
                bool gameStateStartImpulse = Traverse.Create(__instance).Field("gameStateStartImpulse").GetValue<bool>();
                if (gameStateStartImpulse)
                owoSkin.LOG($"GameDirector gameStateOutro"); //Cuando sale el camion
            }
        }

        [HarmonyPatch(typeof(GameDirector), "gameStateEnd")]
        public class OngameStateEnd
        {
            [HarmonyPostfix]
            public static void Prefix(GameDirector __instance)
            {
                bool gameStateStartImpulse = Traverse.Create(__instance).Field("gameStateStartImpulse").GetValue<bool>();
                if (gameStateStartImpulse)
                owoSkin.LOG($"GameDirector gameStateEnd");
            }
        }


        [HarmonyPatch(typeof(GameDirector), "gameStateStart")]
        public class OngameStateStart
        {
            [HarmonyPostfix]
            public static void Prefix(GameDirector __instance)
            {
                if (!LevelGenerator.Instance.Generated) return;

                bool gameStateStartImpulse = Traverse.Create(__instance).Field("gameStateStartImpulse").GetValue<bool>();
                if (gameStateStartImpulse)
                owoSkin.LOG($"GameDirector gameStateEnd");
            }
        }

        #endregion

        #region Laser

        [HarmonyPatch(typeof(SemiLaser), "LaserActive")]
        public class OnLaserActive
        {
            [HarmonyPostfix]
            public static void PostFix(SemiLaser __instance, Vector3 _startPosition, Vector3 _endPosition, bool _isHitting)
            {
                if(IsLocalPlayerNear(explosionDistance, _startPosition) || IsLocalPlayerNear(explosionDistance, _endPosition))
                    owoSkin.LOG($"SemiLaser LaserActive");
            }
        }

        #endregion


    }
}
