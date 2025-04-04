using HarmonyLib;
using Photon.Pun;
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
            owoSkin.LOG($"GrenadeIsLocal:{range} - {position}");


            foreach (PlayerAvatar playerAvatar in GameDirector.instance.PlayerList)
            {
                owoSkin.LOG($"GrenadeIsLocal - photonMine: {playerAvatar.photonView.IsMine}");

                if (playerAvatar.photonView.IsMine || !GameManager.Multiplayer())
                {
                    Vector3 position2 = playerAvatar.PlayerVisionTarget.VisionTransform.position;
                    float num = Vector3.Distance(position, position2);
                    owoSkin.LOG($"Grenade Distance:{position} - {position2} | Num:{num} > {range}");

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
            int distance = IsLocalPlayerNear(explosionDistance, __instance.transform.position);
            if (distance >= 0)
            {
                owoSkin.Feel("Explosion", 3, Mathf.Clamp((distance + 10) - (distance * 2) * 10, 30, 100));
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
                //RecieveExplosion(__instance);
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

        [HarmonyPatch(typeof(ItemGrenade), "TickEnd")]
        public class OnItemGrenadeTickEnd
        {
            [HarmonyPostfix]
            public static void Postfix(ItemGrenade __instance)
            {
                //if (!IsLocalPlayerNear(explosionDistance, __instance.transform.position)) return;

                ItemEquippable itemEquippable = Traverse.Create(__instance).Field("itemEquippable").GetValue<ItemEquippable>();
                bool isEquipped = Traverse.Create(itemEquippable).Field("isEquipped").GetValue<bool>();

                if (isEquipped) return;

                owoSkin.LOG($"ItemGrenade TickEnd");
            }
        }

        #endregion


        #endregion

        #region Cauldron
        [HarmonyPatch(typeof(Cauldron), "Explosion")]
        public class OnExplode
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!owoSkin.suitEnabled) return;

                owoSkin.Feel("Cauldron Explosion",2);
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
                //owoSkin.LOG($"BreakRPC - Lost:{valueLost} break:{breakLevel} loseValue:{_loseValue}");

                if (!owoSkin.CanFeel() && !_loseValue) return;

                PhysGrabObject physGrabObject = Traverse.Create(__instance).Field("physGrabObject").GetValue<PhysGrabObject>();
                bool heldByLocalPlayer = Traverse.Create(physGrabObject).Field("heldByLocalPlayer").GetValue<bool>();

                if (heldByLocalPlayer)
                {
                    owoSkin.Feel("Object Break", 3, 21);
                }

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
                bool heldByLocalPlayer = Traverse.Create(physGrabObject).Field("heldByLocalPlayer").GetValue<bool>();

                if (heldByLocalPlayer && flag)
                {
                    owoSkin.LOG($"ValuableLovePotion StateIdle");
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
