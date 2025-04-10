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
    class BreachFist : GunBehaviour
    {
        public static int encounterTimes;
        public static string consoleID;
        private static string spriteID;

        public static ItemStats stats = new ItemStats();

        public static void Add()
        {
            consoleID = "breach_fist";
            spriteID = "breach_fist";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Breach Fist", spriteID); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods:" + consoleID, Module.MOD_PREFIX + ":" + consoleID);
            gun.gameObject.AddComponent<BreachFist>();

            //Gun descriptions
            gun.SetShortDescription("Twenty Pounds of Iron");
            gun.SetLongDescription("Creates a friendly explosion in front of the player.\n\nThe hand of a BR-TT Model LSA combat mech that has been retrofitted for use by a person. Due to the fact it must rely on a smaller power source than normal, each attack requires a charge up and the heatsinks must be vented more often.");

            // Sprite setup
            gun.SetupSprite(null, spriteID + "_idle_001", 10);
            gun.SetAnimationFPS(gun.shootAnimation, 10);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.SetAnimationFPS(gun.chargeAnimation, 10);
            gun.TrimGunSprites();

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).loopStart = 7;

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(15) as Gun, true, false);
            gun.muzzleFlashEffects.type = VFXPoolType.None;

            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Charged;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.GRENADE;
            gun.reloadTime = 2f;
            gun.gunHandedness = GunHandedness.HiddenOneHanded;
            gun.DefaultModule.cooldownTime = .5f;
            gun.DefaultModule.numberOfShotsInClip = 2;
            gun.DefaultModule.angleVariance = 0f;
            gun.SetBaseMaxAmmo(150);
            gun.gunClass = GunClass.CHARGE;
            gun.carryPixelOffset += new IntVector2(0, 2);
            gun.barrelOffset.transform.localPosition += new Vector3(50f / 16f, 3f / 16f, 0);

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            // Sound adding
            gun.gunSwitchGroup = Module.MOD_PREFIX + consoleID;

            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "Play_WPN_plasmacell_reload_01");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", "Play_WPN_smallrocket_impact_01");

            //Adding projectile to gun
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            BreachExplosionDoer breachExplosion = projectile.gameObject.AddComponent<BreachExplosionDoer>();
            
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);

            ProjectileModule.ChargeProjectile chargeProjectile = new ProjectileModule.ChargeProjectile
            {
                Projectile = projectile,
                ChargeTime = .8f,
            };

            gun.DefaultModule.chargeProjectiles = new List<ProjectileModule.ChargeProjectile>();
            gun.DefaultModule.chargeProjectiles.Add(chargeProjectile);

            // More projectile setup
            projectile.baseData.damage = 35f;
            projectile.baseData.speed = 26f;
            projectile.baseData.range = 30f;
            projectile.baseData.force = 8f;
            projectile.transform.parent = gun.barrelOffset;

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

        class BreachExplosionDoer : MonoBehaviour
        {
            private void Awake()
            {
                Projectile projectile = gameObject.GetComponent<Projectile>();

                ExplosionData explosion = new ExplosionData { };
                explosion.CopyFrom(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData);
                explosion.damage = projectile.ModifiedDamage;
                explosion.damageToPlayer = 0;
                explosion.preventPlayerForce = true;
                Exploder.Explode(projectile.sprite.WorldCenter, explosion, Vector2.zero);

                projectile.DieInAir(true);
            }
        }

    }

    class BreachFistMuzzleFlash : MonoBehaviour
    {
        public static List<string> spritePaths = new List<string>
        {
            "SilverJacket/Resources/VFX/BreachFistMuzzleflash/breach_fist_muzzleflash_start_001",
            "SilverJacket/Resources/VFX/BreachFistMuzzleflash/breach_fist_muzzleflash_start_002",
            "SilverJacket/Resources/VFX/BreachFistMuzzleflash/breach_fist_muzzleflash_start_003",
            "SilverJacket/Resources/VFX/BreachFistMuzzleflash/breach_fist_muzzleflash_start_004",
        };
        public static void Init()
        {
            GameObject obj = BasicVFXCreator.MakeBasicVFX("breach_fist_muzzleflash", spritePaths, 10, new IntVector2(23, 22), tk2dBaseSprite.Anchor.MiddleCenter);
            obj.AddComponent<BreachFistMuzzleFlash>();
            breachMuzzleflashPrefab = obj;
        }
        public static void CreateEffect(Vector2 pos)
        {
            GameObject obj = Instantiate<GameObject>(breachMuzzleflashPrefab);
            obj.transform.position = pos;
            obj.SetActive(true);
        }
        public static GameObject breachMuzzleflashPrefab;
        private void Start()
        {
            this.gameObject.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("start");
        }
    }
}
