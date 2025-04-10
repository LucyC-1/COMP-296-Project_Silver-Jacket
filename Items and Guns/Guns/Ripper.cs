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
using Alexandria.VisualAPI;
using Newtonsoft.Json;

namespace SilverJacket
{
    class Ripper : GunBehaviour
    {
        public static int encounterTimes;
        public static string consoleID;
        private static string spriteID;

        public static ItemStats stats = new ItemStats();

        public static void Add()
        {
            consoleID = "ripper";
            spriteID = "ripper";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Ripper", spriteID); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods:" + consoleID, Module.MOD_PREFIX + ":" + consoleID);
            gun.gameObject.AddComponent<Ripper>();

            //Gun descriptions
            gun.SetShortDescription("Jinkies!");
            gun.SetLongDescription("Attacks with a 3-part combo.\n\nA pair of salon scissors imbued with an unsual emotion that allows them to transform in the heat of battle. The name 'Riyo' is inscribed on the side of the white blade.");

            // Sprite setup
            gun.SetupSprite(null, spriteID + "_idle_001", 12);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();
            gun.carryPixelOffset += new IntVector2(8, 0);
            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(17) as Gun, true, false);
            gun.muzzleFlashEffects.type = VFXPoolType.None;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.gunHandedness = GunHandedness.TwoHanded;
            gun.DefaultModule.customAmmoType = "mega";
            gun.reloadTime = 1.1f;
            gun.DefaultModule.cooldownTime = 0.35f;
            gun.DefaultModule.numberOfShotsInClip = 1;
            gun.DefaultModule.angleVariance = 6f;
            gun.DefaultModule.usesOptionalFinalProjectile = false;
            gun.DefaultModule.numberOfFinalProjectiles = 0;
            gun.SetBaseMaxAmmo(250);
            gun.gunClass = GunClass.SILLY;
            gun.barrelOffset.transform.localPosition = new Vector3(37f / 16f, 7f / 16f, 0);

            GameObject lungeTransform = new GameObject("lungeTransform");
            lungeTransform.transform.SetParent(gun.transform);
            lungeTransform.transform.localPosition = gun.barrelOffset.transform.localPosition + new Vector3(0f / 16f, 0f / 16f, 0);

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.EXCLUDED;
            // Sound adding
            gun.gunSwitchGroup = Module.MOD_PREFIX + consoleID;

            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01");

            //Adding projectile to gun
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            

            Projectile projectile2 = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile2.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile2.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile2);

            projectile2.transform.parent = gun.barrelOffset;

            Projectile projectile3 = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile3.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile3.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile3);

            gun.DefaultModule.projectiles[0] = projectile;

            projectile3.transform.parent = gun.barrelOffset;

            List<string> spritePaths1 = new List<string>
            {
                "SilverJacket/Resources/VFX/Slashes/Ripper/ripper_cut2_001",
                "SilverJacket/Resources/VFX/Slashes/Ripper/ripper_cut2_002",
                "SilverJacket/Resources/VFX/Slashes/Ripper/ripper_cut2_003",
            };

            VFXPool snipSlash = VFXBuilder.CreateVFXPool(Module.MOD_PREFIX + "_" + consoleID + "_snip_slash", spritePaths1, 12, new IntVector2(46, 45), tk2dBaseSprite.Anchor.MiddleCenter, true, .1f);

            List<string> spritePaths2 = new List<string>
            {
                "SilverJacket/Resources/VFX/Slashes/Ripper/ripper_slash_001",
                "SilverJacket/Resources/VFX/Slashes/Ripper/ripper_slash_002",
                "SilverJacket/Resources/VFX/Slashes/Ripper/ripper_slash_003",
            };

            VFXPool swingSlash = VFXBuilder.CreateVFXPool(Module.MOD_PREFIX + "_" + consoleID + "_swing_slash", spritePaths2, 12, new IntVector2(38, 69), tk2dBaseSprite.Anchor.MiddleCenter, true, .1f);

            List<string> spritePaths3 = new List<string>
            {
                "SilverJacket/Resources/VFX/Slashes/Ripper/ripper_stab2_001",
                "SilverJacket/Resources/VFX/Slashes/Ripper/ripper_stab2_002",
                "SilverJacket/Resources/VFX/Slashes/Ripper/ripper_stab2_003",
            };

            VFXPool stabSlash = VFXBuilder.CreateVFXPool(Module.MOD_PREFIX + "_" + consoleID + "_stab_slash", spritePaths3, 12, new IntVector2(83, 43), tk2dBaseSprite.Anchor.MiddleCenter, true, .1f);


            // More projectile setup
            projectile.baseData.damage = 10f;
            projectile.baseData.speed = 26f;
            projectile.baseData.range = 30f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = gun.barrelOffset;
            ProjectileSlashingBehaviour slashingBehaviour1 = projectile.gameObject.AddComponent<ProjectileSlashingBehaviour>();
            slashingBehaviour1.DestroyBaseAfterFirstSlash = true;
            SlashData data1 = slashingBehaviour1.slashParameters;
            data1.VFX = snipSlash;
            data1.slashDegrees = 40;
            data1.slashRange = 2f;
            data1.playerKnockbackForce = 10;

            projectile2.baseData.damage = 15f;
            projectile2.baseData.speed = 26f;
            projectile2.baseData.range = 30f;
            projectile2.baseData.force = 10f;
            projectile2.transform.parent = gun.barrelOffset;
            ProjectileSlashingBehaviour slashingBehaviour2 = projectile2.gameObject.AddComponent<ProjectileSlashingBehaviour>();
            slashingBehaviour2.DestroyBaseAfterFirstSlash = true;
            SlashData data2 = slashingBehaviour2.slashParameters;
            data2.VFX = swingSlash;
            data2.playerKnockbackForce = 20;

            projectile3.baseData.damage = 35f;
            projectile3.baseData.speed = 26f;
            projectile3.baseData.range = 30f;
            projectile3.baseData.force = 20f;
            projectile3.transform.parent = gun.barrelOffset;
            ProjectileSlashingBehaviour slashingBehaviour3 = projectile3.gameObject.AddComponent<ProjectileSlashingBehaviour>();
            slashingBehaviour3.DestroyBaseAfterFirstSlash = true;
            SlashData data3 = slashingBehaviour3.slashParameters;
            data3.VFX = stabSlash;
            data3.slashDegrees = 10;
            data3.slashRange = 5;
            data3.playerKnockbackForce = 60;

            gun.DefaultModule.projectiles.Add(projectile2);
            gun.DefaultModule.projectiles.Add(projectile3);


            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;

            stats.name = gun.EncounterNameOrDisplayName;
        }
        public static int ID;

        private int attackNum = 0;

        

        public override void OwnedUpdatePlayer(PlayerController owner, GunInventory inventory)
        {
            if (owner.CurrentGun.PickupObjectId == ID) return;
            attackNum = 0;
            doReload = false;

        }

        private bool comboTimerRunning = false;

        private IEnumerator ComboTimer()
        {
            comboTimerRunning = true;
            yield return new WaitForSeconds(2f);
            comboTimerRunning = false;
            attackNum = 0;
            yield break;
        }

        private bool doReload = false;

        private bool? cachedContactImmune = null;

        public override Projectile OnPreFireProjectileModifier(Gun gun, Projectile projectile, ProjectileModule module)
        {
            Projectile p = gun.DefaultModule.projectiles[0];
            if (comboTimerRunning)
            {
                GameManager.Instance.StopCoroutine(ComboTimer());
                comboTimerRunning = false;
            }
            
            //ETGModConsole.Log("attack num after coroutine reset: " + attackNum);
            if(attackNum == 0)
            {
                attackNum = 1;
            }
            else if (attackNum == 1)
            {
                gun.spriteAnimator.StopAndResetFrameToDefault();
                gun.spriteAnimator.Play(gun.criticalFireAnimation);
                p = gun.DefaultModule.projectiles[1];
                attackNum = 2;
            }
            else if(attackNum == 2)
            {
                gun.spriteAnimator.StopAndResetFrameToDefault();
                gun.spriteAnimator.Play(gun.finalShootAnimation);
                p = gun.DefaultModule.projectiles[2];
                doReload = true;
                attackNum = 0;

                if (gun.GunPlayerOwner())
                {
                    PlayerController player = gun.GunPlayerOwner();
                    cachedContactImmune = player.ReceivesTouchDamage;

                    player.ReceivesTouchDamage = false;
                    GameManager.Instance.StartCoroutine(TempContactImmune(player));
                }

            }
            //ETGModConsole.Log("attack num after spawn projectile: " + attackNum);
            return p;
        }

        private IEnumerator TempContactImmune(PlayerController player)
        {
            yield return new WaitForSeconds(.4f);
            if(cachedContactImmune != null)
            {
                player.ReceivesTouchDamage = (bool)cachedContactImmune;
            }
            yield break;
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            if (!doReload)
            {
                if (gun.CurrentAmmo > 0)
                {
                    gun.ClipShotsRemaining = 2;
                    gun.ClearReloadData();
                }
            }
        }

        public override void OnReloadedPlayer(PlayerController owner, Gun gun)
        {
            if (doReload)
            {
                if (comboTimerRunning)
                {
                    GameManager.Instance.StopCoroutine(ComboTimer());
                    comboTimerRunning = false;
                }
            }
        }

        public override void OnReloadEndedPlayer(PlayerController owner, Gun gun)
        {
            if (doReload)
            {
                attackNum = 0;
                doReload = false;
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
