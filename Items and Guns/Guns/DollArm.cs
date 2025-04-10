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
    class DollArm : GunBehaviour
    {
        public static int encounterTimes;
        public static string consoleID;
        private static string spriteID;

        public static ItemStats stats = new ItemStats();

        public static void Add()
        {
            consoleID = "doll_arm";
            spriteID = "doll_arm";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Doll Arm", spriteID); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods:" + consoleID, Module.MOD_PREFIX + ":" + consoleID);
            gun.gameObject.AddComponent<DollArm>();

            //Gun descriptions
            gun.SetShortDescription("Everlasting");
            gun.SetLongDescription("When reloaded, destroys nearby projectiles, increasing the damage of the next clip.\n\n The arm of a wooden doll that has seen numerous deaths and caused even more.");

            // Sprite setup
            gun.SetupSprite(null, spriteID + "_idle_001", 12);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();
            gun.carryPixelOffset += new IntVector2(5, 0);
            gun.gunHandedness = GunHandedness.HiddenOneHanded;
            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(15) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(145) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = "samus";
            gun.reloadTime = 1.1f;
            gun.DefaultModule.cooldownTime = 0.3f;
            gun.DefaultModule.numberOfShotsInClip = 8;
            gun.DefaultModule.angleVariance = 6f;
            gun.SetBaseMaxAmmo(250);
            gun.gunClass = GunClass.SILLY;
            gun.barrelOffset.transform.localPosition += new Vector3(-1f / 16f, 0f / 16f, 0);
     
            // Gun tuning
            gun.quality = PickupObject.ItemQuality.C;
            // Sound adding
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(145) as Gun).gunSwitchGroup;

            //Adding projectile to gun
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;



            // More projectile setup
            projectile.baseData.damage = 5f;
            projectile.baseData.speed = 26f;
            projectile.baseData.force = 8f;
            projectile.transform.parent = gun.barrelOffset;

            projectile.SetProjectileSpriteRight("doll_arm_projectile", 8, 6);
            projectile.shouldRotate = true;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;

            stats.name = gun.EncounterNameOrDisplayName;
        }
        public static int ID;

        private bool extraDamage = false;

        public override void PostProcessProjectile(Projectile projectile)
        {
            if (extraDamage)
            {
                projectile.baseData.damage *= 1.15f;
            }
            
        }

        public override void OnReloadedPlayer(PlayerController owner, Gun gun)
        {
            extraDamage = false;

            GameObject proj = SpawnManager.SpawnProjectile((PickupObjectDatabase.GetById(15) as Gun).DefaultModule.projectiles[0].gameObject, gun.barrelOffset.position, Quaternion.Euler(0, 0, gun.CurrentAngle));
            ProjectileSlashingBehaviour behaviour = proj.AddComponent<ProjectileSlashingBehaviour>();
            behaviour.slashParameters.projInteractMode = SlashDoer.ProjInteractMode.DESTROY;
            behaviour.slashParameters.damage = 0;
            behaviour.slashParameters.enemyKnockbackForce = 40;
            behaviour.slashParameters.playerKnockbackForce = 0;
            behaviour.slashParameters.soundEvent = "";
            behaviour.slashParameters.OnHitBullet += OnHitBullet;
            behaviour.SlashDamageUsesBaseProjectileDamage = false;
            Projectile component = proj.GetComponent<Projectile>();
            if (component != null)
            {
                component.Owner = owner;
                component.Shooter = owner.specRigidbody;
                component.m_renderer.enabled = false;

            }
        }

        private void OnHitBullet(Projectile p)
        {
            if (!extraDamage)
            {
                extraDamage = true;
            }
        }

        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            stats.encounterAmount++;
            Module.UpdateStatList();
            base.OnPlayerPickup(playerOwner);
        }
    }
}
