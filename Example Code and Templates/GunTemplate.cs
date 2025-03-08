using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using Alexandria.SoundAPI;
using Alexandria.BreakableAPI;

namespace SilverJacket
{
    class GunTemplate : GunBehaviour
    {
        public static string consoleID;
        public static void Add()
        {
            consoleID = $"{Module.MOD_PREFIX}: ";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun(" ", " "); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods: ", consoleID);
            gun.gameObject.AddComponent<GunTemplate>();

            //Gun descriptions
            gun.SetShortDescription(" ");
            gun.SetLongDescription(" ");

            // Sprite setup
            gun.SetupSprite(null, "_idle_001", 20);
            gun.SetAnimationFPS(gun.shootAnimation, 32);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(26) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(26) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1f;
            gun.DefaultModule.cooldownTime = 0.1f;
            gun.DefaultModule.numberOfShotsInClip = 25;
            gun.DefaultModule.angleVariance = 6f;
            gun.SetBaseMaxAmmo(500);
            gun.gunClass = GunClass.PISTOL;
            gun.barrelOffset.transform.localPosition += new Vector3(5f / 16f, 9f / 16f, 0); 

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.EXCLUDED;
            // Sound adding
            gun.gunSwitchGroup = Module.MOD_PREFIX + "_GunName";

            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "ReloadSoundYouWant");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", "ShootSoundYouWant");

            //Adding projectile to gun
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 3f;
            projectile.baseData.speed = 26f;
            projectile.baseData.range = 30f;
            projectile.baseData.force = 8f;
            projectile.transform.parent = gun.barrelOffset;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }
        public static int ID;
    }
}
