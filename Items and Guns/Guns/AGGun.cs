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
    class AGGun : GunBehaviour
    {
        public static int encounterTimes;
        public static string consoleID;

        public static ItemStats stats = new ItemStats();

        public static void Add()
        {
            consoleID = $"{Module.MOD_PREFIX}:ag_gun";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("AG Gun", "ag_gun"); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods:ag_gun", consoleID);
            gun.gameObject.AddComponent<GunTemplate>();

            //Gun descriptions
            gun.SetShortDescription("No Assembly Required");
            gun.SetLongDescription("The silver in this gun's bullets purify enemies of their status effects on hit, dealing 50% extra damage for each status effect removed." +
                "\n\nLegends tell of a gun that can kill anything with a single shot.\nThis gun is one step away from coming close to it.");

            // Sprite setup
            gun.SetupSprite(null, "ag_gun_idle_001");
            gun.SetAnimationFPS(gun.shootAnimation, 10);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(15) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(223) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = "noxin";
            gun.reloadTime = 1.2f;
            gun.DefaultModule.cooldownTime = 0.25f;
            gun.DefaultModule.numberOfShotsInClip = 1;
            gun.DefaultModule.angleVariance = 0f;
            gun.SetBaseMaxAmmo(92);
            gun.gunClass = GunClass.PISTOL;
            gun.gameObject.AddComponent<OneHandedInFrontOfPlayerGunLayeringBehavior>();
            gun.barrelOffset.transform.localPosition += new Vector3(19f / 16f, 5f / 16f, 0);

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.S;
            // Sound adding
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(53) as Gun).gunSwitchGroup;

            //Adding projectile to gun
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 20f;
            projectile.baseData.speed = 20f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;
            projectile.SetProjectileSpriteRight("ag_gun_projectile", 12, 12);
            projectile.gameObject.AddComponent<AGBulletDamageMod>();

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
            stats.name = gun.EncounterNameOrDisplayName;
        }
        public static int ID;

        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            stats.encounterAmount++;
            Module.UpdateStatList();
            base.OnPlayerPickup(playerOwner);
        }


        class AGBulletDamageMod : MonoBehaviour
        {
            private void Start()
            {
                Projectile p = gameObject.GetComponent<Projectile>();
                p.specRigidbody.OnPreRigidbodyCollision += PreCollide;
            }

            private void PreCollide(SpeculativeRigidbody myBody, PixelCollider myCollider, SpeculativeRigidbody otherBody, PixelCollider otherCollider)
            {
                if (otherBody.gameObject.GetComponent<AIActor>() != null)
                {
                    if (!otherBody.healthHaver.IsDead)
                    {
                        List<GameActorEffect> cached_effects = new List<GameActorEffect> { };
                        foreach(GameActorEffect effect in otherBody.aiActor.m_activeEffects)
                        {
                            if(effect.effectIdentifier != WetEffectTempImmunity.ID && effect.effectIdentifier != OilCoatedTempImmunity.ID && effect.effectIdentifier != PoisonFumeTempImmunity.ID)
                            {
                                gameObject.GetComponent<Projectile>().baseData.damage *= 1.5f;
                                cached_effects.Add(effect);
                            }
                        }
                        foreach(GameActorEffect e in cached_effects)
                        {
                            otherBody.aiActor.RemoveEffect(e.effectIdentifier);
                        }
                    }
                }
            }

        }
    }
}
