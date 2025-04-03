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
                if (__instance.Crouching) owoSkin.LOG($"PlayerController ChangeState - Crouching");
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
                if (enemyIndex == -1) return; // Animación de daño para escenas

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
                owoSkin.LOG($"Playerhealth Heal - HealAmount: {healAmount} - Effect: {effect}"); //it works!
            }
        }

        [HarmonyPatch(typeof(PlayerHealth), "Death")]
        public class OnDeath
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"Playerhealth Death");
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
                owoSkin.LOG($"CameraJump Jump");
            }
        }

        [HarmonyPatch(typeof(CameraJump), "Land")]
        public class OnLand
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"CameraJump Land");
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
                if ((!GameManager.Multiplayer() || __instance.photonView.IsMine)) 
                {
                    ItemVolume componentInChildren = __instance.GetComponentInChildren<ItemVolume>();
                    if ((bool)componentInChildren)
                    {
                        owoSkin.LOG($"PhysGrabber PhysGrabStartEffects - {componentInChildren.itemVolume}");
                    
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PhysGrabber), "PhysGrabEndEffects")]
        public class OnPhysGrabEndEffects
        {
            [HarmonyPostfix]
            public static void Postfix(PhysGrabber __instance)
            {
                if((!GameManager.Multiplayer() || __instance.photonView.IsMine))
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
