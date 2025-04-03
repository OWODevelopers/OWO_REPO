using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using static HurtCollider;

namespace OWO_REPO
{
    public class OWOInteractables
    {
        static OWOSkin owoSkin;
        public static float explosionDistance = 20;

        public OWOInteractables(OWOSkin owoSkinGiven, float explosionReference) 
        {
            owoSkin = owoSkinGiven;
            explosionDistance = explosionReference;
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

        #region WorldInteractable

        #region Explosives

        [HarmonyPatch(typeof(PropaneTankTrap), "Explode")]
        public class OnPropaneTankTrapExplode
        {
            [HarmonyPostfix]
            public static void Postfix(PropaneTankTrap __instance)
            {
                if (!IsLocalPlayerNear(explosionDistance, __instance.transform.position)) return;
                    owoSkin.LOG($"PropaneTankTrap Explode");
            }
        }

        [HarmonyPatch(typeof(BarrelValuable), "Explode")]
        public class OnBarrelValuableExplode
        {
            [HarmonyPostfix]
            public static void Postfix(BarrelValuable __instance)
            {
                if (!IsLocalPlayerNear(explosionDistance, __instance.transform.position)) return;
                    owoSkin.LOG($"BarrelValuable Explode");
            }
        }

        [HarmonyPatch(typeof(FlamethrowerValuable), "Explode")]
        public class OnFlamethrowerValuableExplode
        {
            [HarmonyPostfix]
            public static void Postfix(FlamethrowerValuable __instance)
            {
                if (!IsLocalPlayerNear(explosionDistance, __instance.transform.position)) return;
                    owoSkin.LOG($"FlamethrowerValuable Explode");
            }
        }

        [HarmonyPatch(typeof(PowerCrystalValuable), "Explode")]
        public class OnPowerCrystalValuableExplode
        {
            [HarmonyPostfix]
            public static void Postfix(PowerCrystalValuable __instance)
            {
                if (!IsLocalPlayerNear(explosionDistance, __instance.transform.position)) return;
                    owoSkin.LOG($"PowerCrystalValuable Explode");
            }
        }

        [HarmonyPatch(typeof(ToiletFun), "Explosion")]
        public class OnToiletFunExplosion
        {
            [HarmonyPostfix]
            public static void Postfix(ToiletFun __instance)
            {
                if (!IsLocalPlayerNear(explosionDistance, __instance.transform.position)) return;
                    owoSkin.LOG($"ToiletFun Explosion");
            }
        }
        
        [HarmonyPatch(typeof(ItemMeleeInflatableHammer), "ExplosionRPC")]
        public class OnItemMeleeInflatableHammerExplosionRPC
        {
            [HarmonyPostfix]
            public static void Postfix(ItemMeleeInflatableHammer __instance)
            {
                if (!IsLocalPlayerNear(explosionDistance, __instance.transform.position)) return;
                    owoSkin.LOG($"ItemMeleeInflatableHammer ExplosionRPC");
            }
        }

        #region Grenades

        [HarmonyPatch(typeof(ItemGrenadeDuctTaped), "Explosion")]
        public class OnItemGrenadeDuctTapedExplosion
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenadeDuctTaped __instance)
            {
                if (IsLocalPlayerNear(explosionDistance, __instance.transform.position))
                    owoSkin.LOG($"ItemGrenadeDuctTaped Explosion");
            }
        }

        [HarmonyPatch(typeof(ItemGrenadeExplosive), "Explosion")]
        public class OnItemGrenadeExplosiveExplosion
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenadeExplosive __instance)
            {
                if (IsLocalPlayerNear(explosionDistance, __instance.transform.position))
                    owoSkin.LOG($"ItemGrenadeExplosive Explosion");
            }
        }

        [HarmonyPatch(typeof(ItemGrenadeHuman), "Explosion")]
        public class OnItemGrenadeHumanExplosion
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenadeHuman __instance)
            {
                if (IsLocalPlayerNear(explosionDistance, __instance.transform.position))
                    owoSkin.LOG($"ItemGrenadeHuman Explosion");
            }
        }

        [HarmonyPatch(typeof(ItemGrenadeShockwave), "Explosion")]
        public class OnItemGrenadeShockwave
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenadeShockwave __instance)
            {
                if (IsLocalPlayerNear(explosionDistance, __instance.transform.position))
                    owoSkin.LOG($"ItemGrenadeShockwave Explosion");
            }
        }

        [HarmonyPatch(typeof(ItemGrenadeStun), "Explosion")]
        public class OnItemGrenadeStun
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenadeStun __instance)
            {
                if (IsLocalPlayerNear(explosionDistance, __instance.transform.position))
                    owoSkin.LOG($"ItemGrenadeStun Explosion");
            }
        }

        [HarmonyPatch(typeof(ItemGrenade), "TickEnd")]
        public class OnItemGrenadeTickEnd
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenade __instance)
            {
                if (!IsLocalPlayerNear(explosionDistance, __instance.transform.position)) return;

                ItemEquippable itemEquippable = Traverse.Create(__instance).Field("itemEquippable").GetValue<ItemEquippable>();
                bool isEquipped = Traverse.Create(itemEquippable).Field("isEquipped").GetValue<bool>();

                if (isEquipped) return;

                owoSkin.LOG($"ItemGrenade TickEnd");
            }
        }

        #endregion


        #endregion

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

        #region BreakObject
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

        #region Valuables
        [HarmonyPatch(typeof(ValuableLovePotion), "StateIdle")]
        public class OnValuableLovePotionStateIdle
        {
            [HarmonyPostfix]
            public static void Postfix(ValuableLovePotion __instance)
            {
                bool flag = Traverse.Create(__instance).Field("flag").GetValue<bool>();
                PhysGrabObject physGrabObject = Traverse.Create(__instance).Field("physGrabObject").GetValue<PhysGrabObject>();

                foreach (PhysGrabber physGrabber in physGrabObject.playerGrabbing)
                {
                    if (physGrabber.isLocal)
                    {
                        if (flag)
                        {
                            owoSkin.LOG($"ValuableLovePotion StateIdle");
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Shop
        [HarmonyPatch(typeof(ExtractionPoint), "OnShopClick")]
        public class OnOnShopClick
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG($"ExtractionPoint OnShopClick"); //It works!
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
        #endregion
    }
}
