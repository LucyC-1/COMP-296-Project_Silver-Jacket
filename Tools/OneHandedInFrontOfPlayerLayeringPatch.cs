using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using Alexandria.ItemAPI;
using HarmonyLib;

namespace SilverJacket
{

    public class OneHandedInFrontOfPlayerGunLayeringBehavior : MonoBehaviour
    {
    }

    [HarmonyPatch]
    public class OneHandedInFrontOfPlayerLayeringPatch
    {
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.HandleGunDepthInternal))]
        [HarmonyPrefix]
        public static bool OverrideGunDepth(Gun targetGun, float gunAngle, bool isSecondary)
        {
            if (targetGun.GetComponent<OneHandedInFrontOfPlayerGunLayeringBehavior>() == null)
                return true;

            var sprite = targetGun.GetSprite();

            if (gunAngle > 0f && gunAngle <= 155f && gunAngle >= 25f)
                sprite.HeightOffGround = -0.075f;
            else
                sprite.HeightOffGround = 0.075f;

            return false;
        }
    }
}
