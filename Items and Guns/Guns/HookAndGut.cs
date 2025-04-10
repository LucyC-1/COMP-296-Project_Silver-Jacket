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
using System.Linq;

namespace SilverJacket
{
    class HookAndGut : GunBehaviour
    {
        public static int encounterTimes;
        public static string consoleID;
        private static string spriteID;

        private static GameObject vfxPrefab;

        public static ItemStats stats = new ItemStats();

        public static void Add()
        {
            consoleID = "hook_and_gut";
            spriteID = "hook_and_gut";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("Hook And Gut", spriteID); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods:" + consoleID, Module.MOD_PREFIX + ":" + consoleID);
            gun.gameObject.AddComponent<HookAndGut>();

            //Gun descriptions
            gun.SetShortDescription("No Release");
            gun.SetLongDescription("Hooks enemies on hit. If an enemy is under 10% HP, reload to reel them in and perform a devastating finishing attack with invincibility frames. " +
                "Otherwise, if the enemy is over 10% HP, deal damage and create a pool of blood under them on reload. Increases Curse by 1 while held.\n\n" +
                "An unusual harpoon launcher with a hollow stock that serves as a sheathe for a wicked hunting knife.");

            // Sprite setup
            gun.SetupSprite(null, spriteID + "_idle_001", 12);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.TrimGunSprites();

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(15) as Gun, true, false);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(26) as Gun).muzzleFlashEffects;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.ARROW;
            gun.reloadTime = 1.5f;
            gun.DefaultModule.cooldownTime = 1f;
            gun.DefaultModule.numberOfShotsInClip = 1;
            gun.DefaultModule.angleVariance = 0f;
            gun.SetBaseMaxAmmo(150);
            gun.gunClass = GunClass.RIFLE;
            gun.carryPixelOffset += new IntVector2(8, 0);
            gun.barrelOffset.transform.localPosition += new Vector3(18f / 16f, 9f / 16f, 0);
            gun.AddPassiveStatModifier(PlayerStats.StatType.Curse, 1);

            GunBehaviour[] behavs = gun.GetComponents<GunBehaviour>();           

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.B;
            // Sound adding
            gun.gunSwitchGroup = Module.MOD_PREFIX + consoleID;

            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01", "Play_OBJ_hook_shot_01");
            SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", "Play_WPN_m1rifle_shot_01");

            //Adding projectile to gun
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;

            // More projectile setup
            projectile.baseData.damage = 10f;
            projectile.baseData.speed = 40f;
            projectile.baseData.force = 20f;
            projectile.transform.parent = gun.barrelOffset;
  

            List<string> spritePaths = new List<string>
            {
                "SilverJacket/Resources/VFX/Slashes/HookAndGut/hook_and_gut_slash_001",
                "SilverJacket/Resources/VFX/Slashes/HookAndGut/hook_and_gut_slash_002",
                "SilverJacket/Resources/VFX/Slashes/HookAndGut/hook_and_gut_slash_003",
                "SilverJacket/Resources/VFX/Slashes/HookAndGut/hook_and_gut_slash_004",
            };

            GameObject g = new GameObject { };
            g = BasicVFXCreator.MakeBasicVFX(Module.MOD_PREFIX + "_" + spriteID + "_slash", spritePaths, 12, new IntVector2(15, 25), tk2dBaseSprite.Anchor.MiddleCenter);
            vfxPrefab = g;

            projectile.SetProjectileSpriteRight("hook_projectile_2", 8, 7, true);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
            stats.name = gun.EncounterNameOrDisplayName;
        }

        private bool hooking = false;

        public static int ID;

        public List<AIActor> hookedActors = new List<AIActor> { };

        public override void OwnedUpdatePlayer(PlayerController owner, GunInventory inventory)
        {
            if (owner.CurrentGun.PickupObjectId == ID) return;

            if (hookedActors.Any())
            {
                foreach(AIActor a in hookedActors)
                {
                    if (a.gameObject.GetComponent<ChainToEnemy>())
                    {
                        a.gameObject.GetComponent<ChainToEnemy>().DestroyChain();
                        Destroy(a.gameObject.GetComponent<ChainToEnemy>());
                    }
                }
                hookedActors.Clear();
            }

            if (hooking)
            {
                GameManager.Instance.StopCoroutine(GutEnemies(hookedActors, owner));
                ClearHook(owner);
            }

        }

        public override void AutoreloadOnEmptyClip(GameActor owner, Gun gun, ref bool autoreload)
        {
            autoreload = false;
        }

        public override void PostProcessProjectile(Projectile projectile)
        {
            projectile.shouldRotate = true;
            StickProjectileOnHit comp = projectile.gameObject.AddComponent<StickProjectileOnHit>();
            comp.sourceGun = gun;
            projectile.gameObject.AddComponent<ChainProjectileToPlayer>();
            projectile.gameObject.GetComponent<ChainProjectileToPlayer>().player = (gun.CurrentOwner as PlayerController);
        }
        public override void OnReloadedPlayer(PlayerController owner, Gun gun)
        {
            List<AIActor> actorsToGut = new List<AIActor> { };
            foreach (AIActor a in hookedActors)
            {
                if (a.gameObject.GetComponent<ChainToEnemy>())
                {
                    a.gameObject.GetComponent<ChainToEnemy>().DestroyChain();
                }
                if (!a.healthHaver.IsDead)
                {
                    if (a.healthHaver.IsBoss || a.healthHaver.IsSubboss)
                    {
                        a.healthHaver.ApplyDamage(15 * owner.stats.GetStatValue(PlayerStats.StatType.Damage), Vector2.zero, owner.ActorName, CoreDamageTypes.None, DamageCategory.Normal);
                        AkSoundEngine.PostEvent("Play_BOSS_blobulord_burst_01", gameObject);
                        GoopDefinition bloodGoop = (PickupObjectDatabase.GetById(272) as IronCoinItem).BloodDefinition;
                        DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(bloodGoop).TimedAddGoopCircle(a.sprite.WorldCenter, 1.2f, .3f);
                        SpawnManager.SpawnVFX((PickupObjectDatabase.GetById(692) as Gun).DefaultModule.projectiles[0].hitEffects.enemy.effects[0].effects[0].effect, a.sprite.WorldCenter, new Quaternion(0, 0, 0, 0));

                    }
                    else
                    {
                        float healthPercent = a.healthHaver.GetCurrentHealthPercentage();
                        
                        if (healthPercent <= .2f || a.healthHaver.GetCurrentHealth() <= (15 * owner.stats.GetStatValue(PlayerStats.StatType.Damage)))
                        {
                            if(a.behaviorSpeculator != null)
                            {
                                a.behaviorSpeculator.ImmuneToStun = false;
                                a.behaviorSpeculator.Stun(100, true);
                            }
                            if (a.aiShooter != null)
                            {
                                a.aiShooter.CeaseAttack();
                            }
                            a.MovementSpeed = 0;
                            actorsToGut.Add(a);
                        }
                        else
                        {
                            a.healthHaver.ApplyDamage(15 * owner.stats.GetStatValue(PlayerStats.StatType.Damage), Vector2.zero, owner.ActorName, CoreDamageTypes.None, DamageCategory.Normal);
                            AkSoundEngine.PostEvent("Play_BOSS_blobulord_burst_01", gameObject);
                            GoopDefinition bloodGoop = (PickupObjectDatabase.GetById(272) as IronCoinItem).BloodDefinition;
                            bloodGoop.eternal = false;
                            DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(bloodGoop).TimedAddGoopCircle(a.sprite.WorldCenter,1.2f, .3f);
                            if (a.knockbackDoer != null)
                            {
                                a.knockbackDoer.ApplyKnockback(owner.sprite.WorldCenter - a.sprite.WorldCenter, 15);
                            }
                            
                            SpawnManager.SpawnVFX((PickupObjectDatabase.GetById(692) as Gun).DefaultModule.projectiles[0].hitEffects.enemy.effects[0].effects[0].effect, a.sprite.WorldCenter, new Quaternion(0, 0, 0, 0));

                        }
                    }
                }
                if (a.gameObject.GetComponent<ChainToEnemy>())
                {
                    Destroy(a.gameObject.GetComponent<ChainToEnemy>());
                }
            }
            if(actorsToGut.Any())
            {
                GameManager.Instance.StartCoroutine(GutEnemies(actorsToGut, owner));
            }
            hookedActors.Clear();
        }

        private IEnumerator MakePlayerInvincible(PlayerController player)
        {
            player.healthHaver.IsVulnerable = false;
            yield return new WaitForSeconds(2f);
            player.healthHaver.IsVulnerable = true;
            yield break;
        }

        private IEnumerator GutEnemies(List<AIActor> actorsToGut, PlayerController player)
        {
            hooking = true;
            gun.CanBeDropped = false;
            gun.CanBeSold = false;

            player.inventory.GunLocked.SetOverride("hook_and_gut", true);
            player.SetInputOverride("hook_and_gut");

            GameManager.Instance.StartCoroutine(MakePlayerInvincible(player));

            foreach(AIActor a in actorsToGut)
            {
                a.gameObject.AddComponent<KillKBWhenNearPlayer>();
                a.gameObject.GetComponent<KillKBWhenNearPlayer>().player = player;
            }

            yield return new WaitForSeconds(.3f);

            player.CurrentGun.spriteAnimator.StopAndResetFrameToDefault();
            player.CurrentGun.spriteAnimator.Play(gun.criticalFireAnimation);

          

            yield return new WaitForSeconds(1f);

            SpawnManager.SpawnVFX(vfxPrefab, player.CurrentGun.barrelOffset.position, Quaternion.Euler(0, 0, player.CurrentGun.CurrentAngle));

            foreach (AIActor a in actorsToGut)
            {
                a.healthHaver.ApplyDamage(a.healthHaver.AdjustedMaxHealth, Vector2.zero, "gutted", CoreDamageTypes.None, DamageCategory.Unstoppable, true, null, true);
                SpawnManager.SpawnVFX((PickupObjectDatabase.GetById(692) as Gun).DefaultModule.projectiles[0].hitEffects.enemy.effects[0].effects[0].effect, a.sprite.WorldCenter, Quaternion.Euler(0, 0, 0)); 
            }
            ClearHook(player);
            yield break;
        }

        private void ClearHook(PlayerController player)
        {
            gun.CanBeDropped = true;
            gun.CanBeSold = true;

            player.inventory.GunLocked.SetOverride("hook_and_gut", false);
            player.ClearInputOverride("hook_and_gut");

        }

        public override void OnSwitchedAwayFromPlayer(PlayerController owner, GunInventory inventory, Gun newGun, bool isNewGun)
        {
            foreach(AIActor a in hookedActors)
            {
                a.gameObject.GetComponent<ChainToEnemy>().DestroyChain();
                Destroy(a.gameObject.GetComponent<ChainToEnemy>());
            }
            hookedActors.Clear();
        }

        public override void OnDropped()
        {
            foreach (AIActor a in hookedActors)
            {
                a.gameObject.GetComponent<ChainToEnemy>().DestroyChain();
                Destroy(a.gameObject.GetComponent<ChainToEnemy>());
            }
            hookedActors.Clear();
        }

        public override void OnGunThrown(Gun gun, GameActor owner, Projectile thrownGunProjectile)
        {
            foreach (AIActor a in hookedActors)
            {
                a.gameObject.GetComponent<ChainToEnemy>().DestroyChain();
                Destroy(a.gameObject.GetComponent<ChainToEnemy>());
            }
            hookedActors.Clear();
        }

        public override void OnDestroy()
        {
            foreach (AIActor a in hookedActors)
            {
                a.gameObject.GetComponent<ChainToEnemy>().DestroyChain();
                Destroy(a.gameObject.GetComponent<ChainToEnemy>());
            }
        }

        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            stats.encounterAmount++;
            Module.UpdateStatList();
            base.OnPlayerPickup(playerOwner);
        }

        class ChainProjectileToPlayer : MonoBehaviour
        {
            public PlayerController player;

            private Projectile projectile;

            public tk2dTiledSprite chainSprite;

            void Start()
            {
                projectile = gameObject.GetComponent<Projectile>();
                chainSprite = Instantiate<tk2dTiledSprite>((PickupObjectDatabase.GetById(250) as GrapplingHookItem).GrapplePrefab.GetComponentInChildren<tk2dTiledSprite>());
                chainSprite.transform.position = projectile.sprite.WorldCenter;
                PhysicsEngine.Instance.OnPostRigidbodyMovement += UpdateChain;
                projectile.OnDestruction += Projectile_OnDestruction;
            }

            private void Projectile_OnDestruction(Projectile obj)
            {
                DestroyChain();
                Destroy(chainSprite);
                
            }

            public void UpdateChain()
            {
                if (player != null)
                {
                    chainSprite.transform.position = projectile.sprite.WorldCenter;
                    Vector2 v = projectile.sprite.WorldCenter - (Vector2)(player.CurrentGun.barrelOffset.position);
                    int num = Mathf.RoundToInt(v.magnitude / 0.0625f);
                    chainSprite.dimensions = new Vector2(num, chainSprite.dimensions.y);
                    float z = BraveMathCollege.Atan2Degrees(v);
                    chainSprite.transform.rotation = Quaternion.Euler(0f, 0f, z);
                }
                if (player == null)
                {
                    DestroyChain();
                }
            }

            public void DestroyChain()
            {
                if(chainSprite != null)
                {
                    Destroy(chainSprite);
                }
                PhysicsEngine.Instance.OnPostRigidbodyMovement -= UpdateChain;
            }
        }

        class StickProjectileOnHit : MonoBehaviour
        {
            public PlayerController player;

            private Projectile projectile;

            private AIActor hitEnemy;

            public Gun sourceGun;

            void Start()
            {
                projectile = gameObject.GetComponent<Projectile>();
                projectile.OnHitEnemy += OnHitEnemy;
            }

            private void OnHitEnemy(Projectile p, SpeculativeRigidbody enemy, bool fatal)
            {
                if(enemy.gameObject.GetComponent<AIActor>() != null)
                {
                    hitEnemy = enemy.aiActor;
                    if (!fatal)
                    {
                        if(sourceGun.gameObject.GetComponent<HookAndGut>() != null)
                        {
                            if (hitEnemy.gameObject.GetComponent<ChainToEnemy>() == null)
                            {
                                
                                
                                if (p.Owner.GetComponent<PlayerController>())
                                {
                                    player = p.Owner.GetComponent<PlayerController>();
                                    sourceGun.gameObject.GetComponent<HookAndGut>().hookedActors.Add(hitEnemy);
                                    hitEnemy.gameObject.AddComponent<ChainToEnemy>();
                                    hitEnemy.gameObject.GetComponent<ChainToEnemy>().player = player;
                                }
                            }
                            
                        }
                        
                    }
                }
            }           
        }

        class ChainToEnemy : MonoBehaviour
        {

            public PlayerController player;

            private tk2dTiledSprite chainSprite;

            void Start()
            {
                chainSprite = Instantiate<tk2dTiledSprite>((PickupObjectDatabase.GetById(250) as GrapplingHookItem).GrapplePrefab.GetComponentInChildren<tk2dTiledSprite>());
                chainSprite.transform.position = gameObject.GetComponent<AIActor>().sprite.WorldCenter;
                PhysicsEngine.Instance.OnPostRigidbodyMovement += UpdateChain;
            }

            

            public void UpdateChain()
            {
                if(player != null)
                {
                    chainSprite.transform.position = gameObject.GetComponent<AIActor>().sprite.WorldCenter;
                    Vector2 v = gameObject.GetComponent<AIActor>().sprite.WorldCenter - (Vector2)(player.CurrentGun.barrelOffset.position);
                    int num = Mathf.RoundToInt(v.magnitude / 0.0625f);
                    chainSprite.dimensions = new Vector2(num, chainSprite.dimensions.y);
                    float z = BraveMathCollege.Atan2Degrees(v);
                    chainSprite.transform.rotation = Quaternion.Euler(0f, 0f, z);
                }
                if(player == null)
                {
                    DestroyChain();
                }
            }

            public void DestroyChain()
            {
                if(chainSprite != null)
                {
                    Destroy(chainSprite);
                }
                PhysicsEngine.Instance.OnPostRigidbodyMovement -= UpdateChain; 
            }

            void OnDestroy()
            {
                DestroyChain();
            }
        }

        class KillKBWhenNearPlayer : MonoBehaviour
        {
            public AIActor enemy;
            public PlayerController player;
            private KnockbackDoer kbDoer;
            private Vector2 cachedKBDirection;
            private int kbID;


            void Start()
            {

                enemy = gameObject.GetComponent<AIActor>();
                kbDoer = enemy.knockbackDoer;      
                cachedKBDirection = player.sprite.WorldCenter - enemy.sprite.WorldCenter;  
                kbDoer.m_isImmobile.ClearOverrides();
                kbDoer.m_isImmobile.SetOverride("get yoinked", false);
                kbID = kbDoer.ApplyContinuousKnockback(cachedKBDirection, 60);

            }

            void Update()
            {
                if (player)
                {
                    if (Mathf.Abs(Vector2.Distance(enemy.sprite.WorldCenter, player.sprite.WorldCenter)) < 1)
                    {
                        kbDoer.EndContinuousKnockback(kbID);
                        kbDoer.knockbackMultiplier = 0;
                        kbDoer.m_isImmobile.ClearOverrides();
                        kbDoer.SetImmobile(true, "hook_and_gut");
                    }
                    if (Mathf.Abs(Vector2.Distance(enemy.sprite.WorldCenter, player.sprite.WorldCenter)) > 1)
                    {
                        if((player.sprite.WorldCenter - enemy.sprite.WorldCenter) != cachedKBDirection)
                        {
                            cachedKBDirection = player.sprite.WorldCenter - enemy.sprite.WorldCenter;
                            kbDoer.UpdateContinuousKnockback(cachedKBDirection, 60, kbID);
                        }
                    }
                }
                if (!player)
                {
                    Destroy(gameObject.GetComponent<KillKBWhenNearPlayer>());
                }
            }
        }
    }
}

