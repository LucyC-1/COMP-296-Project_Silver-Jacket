using System;
using System.Collections;
using System.Collections.Generic;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using BepInEx;
using Alexandria.SoundAPI;
using Alexandria.BreakableAPI;

namespace SilverJacket
{
    class FerrymanOar : GunBehaviour
    {
        public static int encounterTimes;
        public static string consoleID;
        private static string spriteID;

        public static ItemStats stats = new ItemStats();
        public static void Add()
        {
            consoleID = "ferryman_oar";
            spriteID = "ferryman_oar";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Ferryman Oar", spriteID); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods:" + consoleID, Module.MOD_PREFIX + ":" + consoleID);
            gun.gameObject.AddComponent<GunTemplate>();

            //Gun descriptions
            gun.SetShortDescription("Tsk Tsk");
            gun.SetLongDescription(" ");

            // Sprite setup
            gun.SetupSprite(null, spriteID + "_idle_001", 20);
            gun.SetAnimationFPS(gun.shootAnimation, 18);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();

            // Projectile setup

            for(int i = 0; i < 5; i++)
            {
                gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(15) as Gun, true, false);
            }
            
            gun.barrelOffset.transform.localPosition += new Vector3(32f / 16f, 30f / 16f, 0);

            foreach (ProjectileModule projectileModule in gun.Volley.projectiles)
            {
                projectileModule.ammoCost = 1;
                projectileModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
                projectileModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
                projectileModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
                projectileModule.customAmmoType = "Tear";
                projectileModule.cooldownTime = 0.7f;
                projectileModule.numberOfShotsInClip = 1;
                projectileModule.angleVariance = 6f;
                //Adding projectile to gun
                Projectile projectile = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(404) as Gun).DefaultModule.projectiles[0]);
                projectile.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(projectile.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(projectile);
                projectileModule.projectiles[0] = projectile;
                gun.DefaultModule.projectiles[0] = projectile;

                // More projectile setup
                projectile.baseData.damage = 3f;
                projectile.baseData.range = 30f;
                projectile.baseData.force = 8f;
                projectile.transform.parent = gun.barrelOffset;

                if(projectileModule != gun.DefaultModule)
                {
                    projectileModule.ammoCost = 0;
                }
                
            }

            ProjectileSlashingBehaviour behaviour = gun.Volley.projectiles[0].projectiles[0].gameObject.AddComponent<ProjectileSlashingBehaviour>();
            gun.Volley.projectiles[0].angleVariance = 0;
            behaviour.DestroyBaseAfterFirstSlash = true;
            SlashData data = behaviour.slashParameters;
            data.projInteractMode = SlashDoer.ProjInteractMode.IGNORE;
            data.enemyKnockbackForce = 60;
            data.damage = 8;
            data.soundEvent = "";

            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(404) as Gun).muzzleFlashEffects;
            
            gun.reloadTime = 1f;
            
            gun.SetBaseMaxAmmo(200);
            gun.gunClass = GunClass.SILLY;
            

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            // Sound adding
            gun.gunSwitchGroup = Module.MOD_PREFIX + consoleID;

            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "Play_ENV_water_splash_01");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", new SwitchedEvent("Play_WPN_Gun_Shot_01", "WPN_Guns", "Siren"));


            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
            stats.name = gun.EncounterNameOrDisplayName;
        }

        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            stats.encounterAmount++;
            Module.UpdateStatList();
            base.OnPlayerPickup(playerOwner);
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            
        }
        public static int ID;
    }
}
