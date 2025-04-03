﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using static HurtCollider;


namespace OWO_REPO
{
    [BepInPlugin("org.bepinex.plugins.OWO_REPO", "OWO_REPO", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
#pragma warning disable CS0109
        internal static new ManualLogSource Log;
#pragma warning restore CS0109

        public static OWOSkin owoSkin;

        private void Awake()
        {
            Log = Logger;
            Logger.LogMessage("OWO_REPO plugin is loaded!");
            owoSkin = new OWOSkin();

            var harmony = new Harmony("owo.patch.repo");
            harmony.PatchAll();
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
                owoSkin.LOG($"Playerhealth Heal - HealAmount: {healAmount} - Effect: {effect}");
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


        #region WorldInteractable

        #region Cauldron
        [HarmonyPatch(typeof(Cauldron), "CookStart")]
        public class OnCookStart
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"Cauldron CookStart");
            }
        }

        [HarmonyPatch(typeof(Cauldron), "EndCook")]
        public class OnEndCook
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"Cauldron EndCook");
            }
        }

        [HarmonyPatch(typeof(Cauldron), "Explosion")]
        public class OnExplode
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"Cauldron Explosion");
            }
        }
        #endregion

        [HarmonyPatch(typeof(ExtractionPoint), "OnShopClick")]
        public class OnOnShopClick
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"ExtractionPoint OnShopClick");
            }
        }

        [HarmonyPatch(typeof(MoneyValuable), "MoneyBurst")]
        public class OnMoneyBurst
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"MoneyValuable MoneyBurst");
            }
        }
        
        [HarmonyPatch(typeof(HurtCollider), "PhysObjectHurt")]
        public class OnPhysObjectHurt
        {
            [HarmonyPostfix]
            public static void Postfix(PhysGrabObject physGrabObject, BreakImpact impact, float hitForce, float hitTorque, bool apply, bool destroyLaunch)
            {
                PhotonView photonView = Traverse.Create(physGrabObject).Field("photonView").GetValue<PhotonView>();

                owoSkin.LOG($"HurtCollider PhysObjectHurt - physGrabObject: {physGrabObject} - impact: {impact} - hitForce: {hitForce} - hitTorque: {hitTorque} - apply: {apply} - destroyLaunch: {destroyLaunch} - isMine?: {photonView.IsMine}");
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
                    if (playerAvatar.photonView.IsMine)
                    {
                        owoSkin.LOG($"YO - ItemUpgrade PlayerUpgrade - YO");
                    }
                }
            }
        }

        #endregion

        #region GrabBeam

        [HarmonyPatch(typeof(PhysGrabBeam), "Start")]
        public class OnStart
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"PhysGrabBeam Start");
            }
        }

        [HarmonyPatch(typeof(PhysGrabBeam), "OnEnable")]
        public class OnOnEnable
        {
            [HarmonyPostfix]
            public static void Postfix(PhysGrabBeam __instance)
            {
                if (__instance.playerAvatar.photonView.IsMine) owoSkin.LOG($"PhysGrabBeam OnEnable - isMine");
            }
        }

        [HarmonyPatch(typeof(PhysGrabBeam), "OnDisable")]
        public class OnOnDisable
        {
            [HarmonyPostfix]
            public static void Postfix(PhysGrabBeam __instance)
            {
                if (__instance.playerAvatar.photonView.IsMine) owoSkin.LOG($"PhysGrabBeam OnDisable - isMine");
            }
        }

        [HarmonyPatch(typeof(PhysGrabObjectImpactDetector), "BreakRPC")]
        public class OnBreakRPC
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"PhysGrabObjectImpactDetector BreakRPC");
            }
        }
        
        #endregion

        





    }
}
