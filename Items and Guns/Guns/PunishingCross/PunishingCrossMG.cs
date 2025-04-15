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
using Dungeonator;
using Alexandria.VisualAPI;

namespace SilverJacket
{
    class PunishingCrossMG : GunBehaviour
    {
        public static string consoleID;
        private static string spriteID;

        private static VFXPool slashFX;

        public static void Add()
        {
            consoleID = "punishing_cross_mg";
            spriteID = "punishing_cross_mg";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Punishing Cross MG", spriteID); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods:" + consoleID, Module.MOD_PREFIX + ":" + consoleID);
            gun.gameObject.AddComponent<PunishingCrossMG>();
            gun.gameObject.AddComponent<GunTransformationManager>();

            //Gun descriptions
            gun.SetShortDescription("Carry With No Vanity");
            gun.SetLongDescription("A powerful two-handed beast of a gun that switches between a machine gun and a rocket launcher on reload.\n" +
                "Destroys nearby bullets on transforming when from MG form and performs a damaging swing on transforming from RPG form.\n\n");

            // Sprite setup
            gun.SetupSprite(null, spriteID + "_idle_001", 12);
            gun.SetAnimationFPS(gun.shootAnimation, 20);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(96) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(98) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SMALL_BULLET;
            gun.reloadTime = 1.5f;
            gun.DefaultModule.cooldownTime = 0.1f;
            gun.DefaultModule.numberOfShotsInClip = 35;
            gun.DefaultModule.angleVariance = 6f;
            gun.SetBaseMaxAmmo(500);
            gun.gunClass = GunClass.FULLAUTO;
            gun.barrelOffset.transform.localPosition = new Vector3(48f / 16f, 17f / 16f, 0);


            GameObject go = new GameObject("mg offset");
            go.transform.SetParent(gun.transform);
            go.transform.localPosition = new Vector3(52f / 16f, 17f / 16f, 0);

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.S;
            // Sound adding
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(96) as Gun).gunSwitchGroup;

            //Adding projectile to gun
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 6.5f;
            projectile.baseData.speed = 26f;
            projectile.baseData.force = 10f;

            projectile.transform.parent = gun.barrelOffset;

            List<string> spritePaths = new List<string>
            {
                "SilverJacket/Resources/VFX/Slashes/Cross/cross_slash_001",
                 "SilverJacket/Resources/VFX/Slashes/Cross/cross_slash_002",
                  "SilverJacket/Resources/VFX/Slashes/Cross/cross_slash_003",
                   "SilverJacket/Resources/VFX/Slashes/Cross/cross_slash_004",
            };

            VFXPool crossSlash = VFXBuilder.CreateVFXPool(Module.MOD_PREFIX + "_" + consoleID + "_slash", spritePaths, 12, new IntVector2(53, 78), tk2dBaseSprite.Anchor.MiddleCenter, true, .1f);

            slashFX = crossSlash;

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }

        public override void OnReloadedPlayer(PlayerController owner, Gun gun)
        {
            reloadEnded = false;
            if (gun.ClipShotsRemaining == 0)
            {
                if (!doingTransformEnum)
                {
                    doingTransformEnum = true;
                    GameManager.Instance.StartCoroutine(ReloadTransformCR());
                }
                GunTransformationManager comp = gun.gameObject.GetComponent<GunTransformationManager>();
                if(comp.currentState == GunTransformationManager.GunTransformState.MG)
                {
                    GameManager.Instance.StartCoroutine(DestroyNearbyBullets(owner));
                    AkSoundEngine.PostEvent("Stop_WPN_All", gameObject);
                    AkSoundEngine.PostEvent("Play_WPN_SAA_reload_01", gameObject);
                }
                else if(comp.currentState == GunTransformationManager.GunTransformState.RPG)
                {
                    GameObject offset = null;
                    foreach(Transform t in gun.transform)
                    {
                        if(t.gameObject.name.Contains("mg offset"))
                        {
                            offset = t.gameObject;
                            break;
                        }
                    }
                    GameObject g = SpawnManager.SpawnProjectile((PickupObjectDatabase.GetById(15) as Gun).DefaultModule.projectiles[0].gameObject, gun.barrelOffset.TransformPoint(new Vector3( 16 / 16, 0 / 16)), Quaternion.Euler(0, 0, gun.CurrentAngle));
                    Projectile p = g.GetComponent<Projectile>();
                    if(p != null)
                    {
                        p.Owner = owner;
                        p.Shooter = owner.specRigidbody;
                        p.m_renderer.enabled = false;
                        ProjectileSlashingBehaviour slashingBehaviour = g.AddComponent<ProjectileSlashingBehaviour>();
                        slashingBehaviour.DestroyBaseAfterFirstSlash = true;
                        slashingBehaviour.SlashDamageUsesBaseProjectileDamage = false;
                        SlashData data = slashingBehaviour.slashParameters;
                        data.damage = 25f;
                        data.enemyKnockbackForce = 60;
                        data.playerKnockbackForce = 10;
                        data.VFX = slashFX;
                    }
                }
            }
            
        }

        private IEnumerator DestroyNearbyBullets(PlayerController owner)
        {
            yield return null;
            float elapsed = 0;
            while(elapsed < .4f)
            {
                elapsed += Time.deltaTime;
                SilencerInstance.DestroyBulletsInRange(owner.sprite.WorldCenter, 5f, true, false);
            }
            yield break;
        }

        public override void OwnedUpdatePlayer(PlayerController owner, GunInventory inventory)
        {
            if (owner)
            {
                if(owner.CurrentGun.PickupObjectId != ID && doingTransformEnum)
                {
                    GameManager.Instance.StopCoroutine(ReloadTransformCR());
                }
            }
        }

        private bool reloadEnded = false;

        public override void OnReloadEndedPlayer(PlayerController owner, Gun gun)
        {
            reloadEnded = true;
        }

        private bool doingTransformEnum = false;
 
        private IEnumerator ReloadTransformCR()
        {
            do
            {
                yield return null;
            } while (reloadEnded == false);
            gun.gameObject.GetComponent<GunTransformationManager>().Transform();
            doingTransformEnum = false;
            yield break;
        }

        public static int ID;

        
    }

    class GunTransformationManager : MonoBehaviour
    {
        private Gun gun;
        private static int mgID;
        private static int rpgID;

        public enum GunTransformState
        {
            MG,
            RPG
        }

        public GunTransformState currentState;

        private bool transformed = false;

        private void Awake()
        {
            gun = gameObject.GetComponent<Gun>();
            currentState = GunTransformState.MG;
            transformed = false;
        }

        private void Update()
        {
            if (Dungeon.IsGenerating || Dungeon.ShouldAttemptToLoadFromMidgameSave)
            {
                return;
            }
            if (gun && gun.CurrentOwner is PlayerController)
            {
                PlayerController playerController = gun.CurrentOwner as PlayerController;
                if (!gun.enabled)
                {
                    return;
                }
            }
            else if(gun && !gun.CurrentOwner && transformed)
            {
                Detransform();
            }
        }

        public void Transform()
        {
            if(currentState == GunTransformState.MG)
            {
                transformed = true;
                gun.TransformToTargetGun((PickupObjectDatabase.GetById(rpgID) as Gun));
                currentState = GunTransformState.RPG;
            }
            else if (currentState == GunTransformState.RPG)
            {
                Detransform();
            }
        }

        public void Detransform()
        {
            transformed = false;
            if (currentState == GunTransformState.MG)
            {
                return;
            }
            currentState = GunTransformState.MG;
            gun.TransformToTargetGun((PickupObjectDatabase.GetById(mgID) as Gun));
        }


        public static void InitialiseIDs()
        {
            mgID = PunishingCrossMG.ID;
            rpgID = PunishingCrossRPG.ID;
        }
    }
}
