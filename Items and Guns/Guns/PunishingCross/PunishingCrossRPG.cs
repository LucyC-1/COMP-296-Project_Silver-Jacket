using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using Alexandria.SoundAPI;
using Alexandria.BreakableAPI;
using System.Collections.Generic;

namespace SilverJacket
{
    class PunishingCrossRPG : GunBehaviour
    {
        public static string consoleID;
        private static string spriteID;
        public static void Add()
        {
            consoleID = "punishing_cross_rocket";
            spriteID = "punishing_cross_rocket";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("punishing_cross_rocket", spriteID); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods:" + consoleID, Module.MOD_PREFIX + ":" + consoleID);
            gun.gameObject.AddComponent<PunishingCrossRPG>();

            //Gun descriptions
            gun.SetShortDescription("Wrong Gun Version. Use MG Version");
            gun.SetLongDescription("Wrong Version.");

            // Sprite setup
            gun.SetupSprite(null, spriteID + "_idle_001", 12);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(39) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(39) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 10;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.GRENADE;
            gun.reloadTime = 1.6f;
            gun.DefaultModule.cooldownTime = 1f;
            gun.DefaultModule.numberOfShotsInClip = 1;
            gun.DefaultModule.angleVariance = 0f;
            gun.SetBaseMaxAmmo(500);
            gun.gunClass = GunClass.EXPLOSIVE;
            gun.barrelOffset.transform.localPosition = new Vector3(57f / 16f, 17f / 16f, 0);

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.EXCLUDED;
            // Sound adding
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(39) as Gun).gunSwitchGroup;

            //Adding projectile to gun
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 35f;
            projectile.baseData.speed = 50f;
            projectile.baseData.range = 10000f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;
            projectile.BossDamageMultiplier = 1;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;
    }
}
