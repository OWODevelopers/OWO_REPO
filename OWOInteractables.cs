﻿using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using static HurtCollider;

namespace OWO_REPO
{
    public class OWOInteractables
    {
        static OWOSkin owoSkin;
        public static float explosionDistance = 10;

        public OWOInteractables(OWOSkin owoSkinGiven, float explosionReference)
        {
            owoSkin = owoSkinGiven;
            explosionDistance = explosionReference;
        }

        public static int IsLocalPlayerNear(float range, Vector3 position)
        {            
            foreach (PlayerAvatar playerAvatar in GameDirector.instance.PlayerList)
            {
                //owoSkin.LOG($"GrenadeIsLocal - photonMine: {playerAvatar.photonView.IsMine}");

                if (playerAvatar.photonView.IsMine || !GameManager.Multiplayer())
                {
                    Vector3 position2 = playerAvatar.PlayerVisionTarget.VisionTransform.position;
                    float num = Vector3.Distance(position, position2);                    

                    if (num > range)
                    {
                        return -1;
                    }
                    return (int)num;
                }
                continue;
            }
            return -1;
        }

        public static void RecieveExplosion(MonoBehaviour __instance)
        {
            if (!owoSkin.CanFeel()) return;

            int distance = IsLocalPlayerNear(explosionDistance, __instance.transform.position);
            if (distance >= 0)
            {
                owoSkin.Feel("Explosion", 3, Mathf.Clamp(((distance + 10) - (distance * 2)) * 10, 30, 100));
                //owoSkin.LOG($"ItemGrenadeHuman Explosion {(distance + 10) - (distance * 2)}");
            }
        }

        #region WorldInteractable

        #region Explosives

        [HarmonyPatch(typeof(PropaneTankTrap), "Explode")]
        public class OnPropaneTankTrapExplode
        {
            [HarmonyPostfix]
            public static void Postfix(PropaneTankTrap __instance)
            {
                RecieveExplosion(__instance);
            }
        }

        [HarmonyPatch(typeof(BarrelValuable), "Explode")]
        public class OnBarrelValuableExplode
        {
            [HarmonyPostfix]
            public static void Postfix(BarrelValuable __instance)
            {
                RecieveExplosion(__instance);                
            }
        }

        [HarmonyPatch(typeof(FlamethrowerValuable), "Explode")]
        public class OnFlamethrowerValuableExplode
        {
            [HarmonyPostfix]
            public static void Postfix(FlamethrowerValuable __instance)
            {
                RecieveExplosion(__instance);
            }
        }

        [HarmonyPatch(typeof(PowerCrystalValuable), "Explode")]
        public class OnPowerCrystalValuableExplode
        {
            [HarmonyPostfix]
            public static void Postfix(PowerCrystalValuable __instance)
            {
                RecieveExplosion(__instance);
            }
        }

        [HarmonyPatch(typeof(ToiletFun), "Explosion")]
        public class OnToiletFunExplosion
        {
            [HarmonyPostfix]
            public static void Postfix(ToiletFun __instance)
            {
                RecieveExplosion(__instance);
            }
        }

        [HarmonyPatch(typeof(ItemMeleeInflatableHammer), "ExplosionRPC")]
        public class OnItemMeleeInflatableHammerExplosionRPC
        {
            [HarmonyPostfix]
            public static void Postfix(ItemMeleeInflatableHammer __instance)
            {
                RecieveExplosion(__instance);
            }
        }

        [HarmonyPatch(typeof(ItemMine), "StateTriggered")]
        public class OnStateTriggered
        {
            [HarmonyPostfix]
            public static void Postfix(ItemMine __instance)
            {
                RecieveExplosion(__instance);
            }
        }

        #region Grenades

        [HarmonyPatch(typeof(ItemGrenadeDuctTaped), "Explosion")]
        public class OnItemGrenadeDuctTapedExplosion
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenadeDuctTaped __instance)
            {
                RecieveExplosion(__instance);
            }
        }

        [HarmonyPatch(typeof(ItemGrenadeExplosive), "Explosion")]
        public class OnItemGrenadeExplosiveExplosion
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenadeExplosive __instance)
            {
                RecieveExplosion(__instance);
            }
        }

        [HarmonyPatch(typeof(ItemGrenadeHuman), "Explosion")]
        public class OnItemGrenadeHumanExplosion
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenadeHuman __instance)
            {
                RecieveExplosion(__instance);
            }            
        }

        [HarmonyPatch(typeof(ItemGrenadeShockwave), "Explosion")]
        public class OnItemGrenadeShockwave
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenadeShockwave __instance)
            {
                RecieveExplosion(__instance);
            }
        }

        [HarmonyPatch(typeof(ItemGrenadeStun), "Explosion")]
        public class OnItemGrenadeStun
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenadeStun __instance)
            {
                RecieveExplosion(__instance);
            }
        }

        #endregion


        #endregion

        #region Cauldron
        [HarmonyPatch(typeof(Cauldron), "Explosion")]
        public class OnExplode
        {
            [HarmonyPostfix]
            public static void Postfix(Cauldron __instance)
            {
                if (!owoSkin.CanFeel()) return;

                RecieveExplosion(__instance);                
            }
        }
        #endregion

        #region BreakObject

        [HarmonyPatch(typeof(PhysGrabObjectImpactDetector), "BreakRPC")]
        public class OnBreakRPC
        {
            [HarmonyPostfix]
            public static void PostFix(PhysGrabObjectImpactDetector __instance, float valueLost, Vector3 _contactPoint, int breakLevel, bool _loseValue)
            {
                if (!owoSkin.CanFeel() && !_loseValue) return;

                PhysGrabObject physGrabObject = Traverse.Create(__instance).Field("physGrabObject").GetValue<PhysGrabObject>();
                bool heldByLocalPlayer = Traverse.Create(physGrabObject).Field("heldByLocalPlayer").GetValue<bool>();

                if (heldByLocalPlayer)
                {
                    owoSkin.Feel("Object Break", 3, Mathf.Clamp((int)(valueLost / 500) * 100, 25, 100));
                }

            }
        }

        #endregion
        #endregion
        
        [HarmonyPatch(typeof(ItemGun), "ShootRPC")]
        public class OnShootRPC
        {
            [HarmonyPostfix]
            public static void PostFix(ItemGun __instance)
            {
                if (!owoSkin.CanFeel()) return;

                bool isLocal = false;

                PhysGrabObject physGrabObject = Traverse.Create(__instance).Field("physGrabObject").GetValue<PhysGrabObject>();
                List<PhysGrabber> playerGrabbing = Traverse.Create(physGrabObject).Field("playerGrabbing").GetValue<List<PhysGrabber>>();

                if (GameManager.Multiplayer())
                {
                    foreach (PhysGrabber item in playerGrabbing)
                    {
                        if (item.photonView.IsMine)
                        {
                            isLocal = true;
                        }
                    }
                }
                else if (playerGrabbing.Count > 0)
                {
                    isLocal = true;
                }

                if (isLocal)
                    owoSkin.Feel("Recoil", 3);
            }
        }


    }
}
