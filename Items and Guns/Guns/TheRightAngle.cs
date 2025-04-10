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
    class TheRightAngle : GunBehaviour
    {
        public static int encounterTimes;
        public static string consoleID;
        private static string spriteID;

        public static ItemStats stats = new ItemStats();

        public static void Add()
        {
            consoleID = "the_right_angle";
            spriteID = "the_right_angle";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("The Right Angle", spriteID); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods:" + consoleID, Module.MOD_PREFIX + ":" + consoleID);
            gun.gameObject.AddComponent<TheRightAngle>();

            //Gun descriptions
            gun.SetShortDescription("Projectile Dysfunction");
            gun.SetLongDescription("Fires bullets at a 90 degree angle.\n\nHappens to the best of us.");

            // Sprite setup
            gun.SetupSprite(null, spriteID + "_idle_001", 12);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();

            gun.carryPixelOffset += new IntVector2(6, 0);

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(15) as Gun, true, false);
            gun.muzzleFlashEffects.type = VFXPoolType.None;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = .6f;
            gun.DefaultModule.cooldownTime = 0.14f;
            gun.DefaultModule.numberOfShotsInClip = 25;
            gun.DefaultModule.angleVariance = 6f;
            gun.DefaultModule.angleFromAim = -90;
            gun.SetBaseMaxAmmo(450);
            gun.gunClass = GunClass.RIFLE;

            
            gun.barrelOffset.transform.localPosition += new Vector3(11f / 16f, 1.5f / 16f, 0);

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.D;
            // Sound adding
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(15) as Gun).gunSwitchGroup;

            //Adding projectile to gun
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;


            // More projectile setup
            projectile.transform.parent = gun.barrelOffset;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
            stats.name = gun.EncounterNameOrDisplayName;
        }
        public static int ID;


        public override void OnPostFired(PlayerController player, Gun gun)
        {          
            SpawnManager.SpawnVFX((PickupObjectDatabase.GetById(15) as Gun).muzzleFlashEffects.effects[0].effects[0].effect, gun.barrelOffset.position, Quaternion.Euler(0f, 0f, (gun.CurrentAngle - 90)));
        }

        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            stats.encounterAmount++;
            Module.UpdateStatList();
            base.OnPlayerPickup(playerOwner);
        }


    }
}
