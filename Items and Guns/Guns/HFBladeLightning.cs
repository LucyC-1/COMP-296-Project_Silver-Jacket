using Alexandria.ItemAPI;
using Alexandria.VisualAPI;
using Gungeon;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Alexandria.CharacterAPI;

namespace SilverJacket
{
    class HFBladeLightning : GunBehaviour
    {
        public static string consoleID;
        private static string spriteID;
        public static void Add()
        {
            consoleID = "hf_blade_lightning";
            spriteID = "hf_blade_lightning";
            // New gun base
            Gun gun = ETGMod.Databases.Items.NewGun("HF Blade Lightning", spriteID); // First string is plain text name, second string is sprite prefix
            Game.Items.Rename("outdated_gun_mods:" + consoleID, Module.MOD_PREFIX + ":" + consoleID);
            gun.gameObject.AddComponent<HFBladeLightning>();

            //Gun descriptions
            gun.SetShortDescription("Not Your Sword");
            gun.SetLongDescription("After killing 6 enemies, gain the ability to enter Ripper Mode by manually pressing the reload button. " +
                "While in Ripper Mode, gain increased speed, your uncharged attacks deal a bit more damage, and your charged attacks take much less time to charge and deal a lot more damage. " +
                "Cannot switch weapons while in Ripper Mode. Increases Curse by 2 while held." +
                "\n\nThe former sword of a famous mercenary who really just likes killing evil people.");

            // Sprite setup
            gun.SetupSprite(null, spriteID + "_idle_001", 20);
            gun.SetAnimationFPS(gun.shootAnimation, 12);
            gun.SetAnimationFPS(gun.reloadAnimation, 12);
            gun.SetAnimationFPS(gun.chargeAnimation, 8);
            gun.SetAnimationFPS(gun.criticalFireAnimation, 18);
            gun.TrimGunSprites();

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.chargeAnimation).loopStart = 20;

            gun.carryPixelOffset += new IntVector2(14, 0);

            // Projectile setup
            gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(17) as Gun, true, false);
            gun.muzzleFlashEffects.type = VFXPoolType.None;
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Charged;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = "pulse blue";
            gun.reloadTime = 1f;
            gun.DefaultModule.cooldownTime = .4f;
            gun.DefaultModule.triggerCooldownForAnyChargeAmount = true;
            gun.DefaultModule.numberOfShotsInClip = 1;
            gun.DefaultModule.angleVariance = 0f;
            gun.SetBaseMaxAmmo(250);
            gun.gunClass = GunClass.CHARGE;
            gun.barrelOffset.transform.localPosition += new Vector3(20f / 16f, 19f / 16f, 0);
            gun.AddPassiveStatModifier(PlayerStats.StatType.Curse, 2);

            // Gun tuning
            gun.quality = PickupObject.ItemQuality.A;
            // Sound adding
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(390) as Gun).gunSwitchGroup;

            //SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Reload_01");
           // SoundManager.AddCustomSwitchData("WPN_Guns", gun.gunSwitchGroup, "Play_WPN_Gun_Shot_01", new SwitchedEvent("Play_WPN_Gun_Shot_01", "WPN_Guns", "Thor"));

            GameObject obj = new GameObject();
            obj.transform.SetParent(gun.barrelOffset.transform);
            obj.transform.localPosition = gun.barrelOffset.transform.localPosition += new Vector3(12f / 16f, 4f / 16f);

            //Adding projectile to gun
            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);

            // More projectile setup
            projectile.baseData.damage = 15f;
            projectile.baseData.force = 10f;
            projectile.transform.parent = obj.transform;

            ProjectileIDMarker marker1 = projectile.gameObject.AddComponent<ProjectileIDMarker>();
            marker1.ID = "hf_noharge";

            ProjectileSlashingBehaviourForked behaviour1 = projectile.gameObject.AddComponent<ProjectileSlashingBehaviourForked>();
            behaviour1.SlashDamageUsesBaseProjectileDamage = true;
            behaviour1.slashParameters.projInteractMode = SlashDoerForked.ProjInteractMode.DESTROY;
            behaviour1.DestroyBaseAfterFirstSlash = true;
            behaviour1.slashParameters.soundEvent = null;
            behaviour1.slashParameters.slashRange = 2.5f;

            ProjectileModule.ChargeProjectile chargeProjectile = new ProjectileModule.ChargeProjectile
            {
                Projectile = projectile,
                ChargeTime = 0,                
            };

            Projectile projectile2 = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile2.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile2.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile2);

            projectile2.baseData.damage = 35f;
            projectile2.baseData.force = 40;
            projectile2.transform.parent = gun.barrelOffset;

            ProjectileIDMarker marker2 = projectile2.gameObject.AddComponent<ProjectileIDMarker>();
            marker2.ID = "hf_fullcharge";

            ProjectileSlashingBehaviourForked behaviour2 = projectile2.gameObject.AddComponent<ProjectileSlashingBehaviourForked>();
            
            behaviour2.SlashDamageUsesBaseProjectileDamage = true;
            behaviour2.DestroyBaseAfterFirstSlash = true;
            SlashDataForked slashData = behaviour2.slashParameters;
            slashData.slashDegrees = 210f;
            slashData.slashRange = 2.5f;
            slashData.soundEvent = null;
            //slashData.soundEvent = new SwitchedEvent("Play_WPN_Gun_Shot_01", "WPN_Guns", "Miner").EventName;
            
            List<string> spritePaths = new List<string>
            {
                "SilverJacket/Resources/VFX/Slashes/HFBladeLightning/lightning_slash_001",
                "SilverJacket/Resources/VFX/Slashes/HFBladeLightning/lightning_slash_002",
                "SilverJacket/Resources/VFX/Slashes/HFBladeLightning/lightning_slash_003",
                "SilverJacket/Resources/VFX/Slashes/HFBladeLightning/lightning_slash_004",
            };

            VFXPool lightningSlash = VFXBuilder.CreateVFXPool(Module.MOD_PREFIX + "_" + consoleID + "_slash", spritePaths, 12,  new IntVector2(99, 94), tk2dBaseSprite.Anchor.MiddleCenter, true, .1f);

            slashData.VFX = lightningSlash;

            ProjectileModule.ChargeProjectile chargeProjectile2 = new ProjectileModule.ChargeProjectile
            {
                Projectile = projectile2,
                ChargeTime = 2f,
                OverrideShootAnimation = gun.criticalFireAnimation,
                UsedProperties = ProjectileModule.ChargeProjectileProperties.shootAnim
            };

            gun.DefaultModule.chargeProjectiles = new List<ProjectileModule.ChargeProjectile> { };
            gun.DefaultModule.chargeProjectiles.Add(chargeProjectile);
            gun.DefaultModule.chargeProjectiles.Add(chargeProjectile2);

            ETGMod.Databases.Items.Add(gun, false, "ANY");
            ID = gun.PickupObjectId;
        }

        public bool canDoRipper = false;

        private bool isRipperMode = false;

        [SerializeField]
        public int kills = 0;

        public override void PostProcessProjectile(Projectile projectile)
        {
            if (projectile.gameObject.GetComponent<ProjectileIDMarker>() != null)
            {
                if (projectile.gameObject.GetComponent<ProjectileIDMarker>().ID == "hf_fullcharge")
                {
                    ProjectileSlashingBehaviourForked b = projectile.gameObject.GetComponent<ProjectileSlashingBehaviourForked>();
                    
                    b.slashParameters.OnHitTarget += OnHitTargetCharged;

                    if (isRipperMode)
                    {
                        b.slashParameters.damage *= 2f;
                    }
                }
                else if(projectile.gameObject.GetComponent<ProjectileIDMarker>().ID == "hf_nocharge")
                {
                    ProjectileSlashingBehaviourForked b = projectile.gameObject.GetComponent<ProjectileSlashingBehaviourForked>();
                    b.slashParameters.OnHitTarget += OnHitTarget;
                    b.slashParameters.damage *= 1.2f;
                    
                }
            }

            if(gun.CurrentAmmo > 0)
            {
                gun.ClipShotsRemaining = 2;
                gun.ClearReloadData();
            }
        }
        private void OnHitTarget(GameActor enemy, bool fatal)
        {
            if (fatal)
            {  
                if (!isRipperMode)
                {
                    kills += 1;
                    if (kills > 6)
                    {
                        kills = 6;
                    }
                }
                
            }

            if (kills == 6 && !canDoRipper && !isRipperMode)
            {
                AkSoundEngine.PostEvent("Play_WPN_energy_impact_01", gameObject);
                SpawnManager.SpawnVFX((PickupObjectDatabase.GetById(21) as Gun).DefaultModule.chargeProjectiles[4].Projectile.hitEffects.overrideMidairDeathVFX, gun.CurrentOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, 0));
                canDoRipper = true;
            }

        }

        private void OnHitTargetCharged(GameActor enemy, bool fatal)
        {
            if (fatal)
            {
                if (!isRipperMode)
                {
                    kills += 1;
                    if (kills > 6)
                    {
                        kills = 6;
                    }
                }
                if (isRipperMode)
                {
                    if(!enemy.healthHaver.IsBoss && !enemy.healthHaver.IsSubboss & enemy.healthHaver)
                    {
                        if(enemy.gameObject.GetComponent<PreventDupeSlicing>() == null)
                        {
                            enemy.gameObject.AddComponent<PreventDupeSlicing>();
                            enemy.gameObject.GetComponent<PreventDupeSlicing>().AssignDeathEffect();

                            Texture2D fullTexure = GunTools.DesheetTexture(enemy.sprite.GetCurrentSpriteDef());

                            Texture2D topSpriteHalf = new Texture2D(fullTexure.width, Mathf.CeilToInt(fullTexure.height / 2f));

                            Texture2D bottomSpriteHalf = new Texture2D(fullTexure.width, Mathf.FloorToInt(fullTexure.height / 2f));

                            //ETGModConsole.Log("Full sprite: width = " + fullTexure.width + " height = " + fullTexure.height);

                            //ETGModConsole.Log($"Top half: width = {fullTexure.width} height = {Mathf.FloorToInt(fullTexure.height / 2f)} starting pos = 0, {Mathf.CeilToInt(fullTexure.height / 2f)}");

                            //ETGModConsole.Log($"Bottom half: width = {fullTexure.width} height = {Mathf.FloorToInt(fullTexure.height / 2f)} starting pos = 0, {Mathf.FloorToInt(fullTexure.height / 2f)}");

                            topSpriteHalf.SetPixels(fullTexure.GetPixels(0, Mathf.FloorToInt(fullTexure.height / 2f), fullTexure.width, Mathf.CeilToInt(fullTexure.height / 2f)));




                            bottomSpriteHalf.SetPixels(fullTexure.GetPixels(0, 0, fullTexure.width, Mathf.FloorToInt(fullTexure.height / 2f)));

                            GameObject topHalf = SpriteBuilder.SpriteFromTexture(topSpriteHalf, enemy.name + "_topSpriteHalf");
                            topHalf.GetComponent<tk2dSprite>().HeightOffGround += .1f;

                            //topHalf.transform.Rotate(0, -90, 0);

                            Vector2 topDir = Random.insideUnitCircle * 2.5f;
                            if (topDir.y < .5f) { topDir.y = .5f; }

                            MoveAndKillAfterDelay topBehav = topHalf.AddComponent<MoveAndKillAfterDelay>();

                            topBehav.startingPos = (enemy.sprite.WorldTopCenter + enemy.sprite.WorldCenter) / 2f;

                            topBehav.targetPos = topDir + topBehav.startingPos;

                            //ETGModConsole.Log($"Top starting pos: {topBehav.startingPos}, top target pos: {topBehav.targetPos}");

                            topBehav.StartTimer();

                            topHalf.transform.position = topBehav.startingPos;
                            topHalf.SetActive(true);

                            GameObject bottomHalf = SpriteBuilder.SpriteFromTexture(bottomSpriteHalf, enemy.name + "_bottomSpriteHalf");
                            bottomHalf.GetComponent<tk2dSprite>().HeightOffGround += .1f;

                            //bottomHalf.transform.Rotate(0, -90, 0);

                            Vector2 bottomDir = Random.insideUnitCircle * 2.5f;

                            if (bottomDir.y > -.5f) { bottomDir.y = -.5f; }

                            MoveAndKillAfterDelay bottomBehav = bottomHalf.AddComponent<MoveAndKillAfterDelay>();

                            bottomBehav.startingPos = (enemy.sprite.WorldBottomCenter + enemy.sprite.WorldCenter) / 2f;
                            bottomBehav.targetPos = bottomDir + bottomBehav.startingPos;

                            bottomBehav.StartTimer();

                            //ETGModConsole.Log($"bottom starting pos: {bottomBehav.startingPos}, bottom target pos: {bottomBehav.targetPos}");


                            bottomHalf.transform.position = bottomBehav.startingPos;
                            bottomHalf.SetActive(true);

                            //enemy.healthHaver.deathEffect = null;
                            enemy.StealthDeath = true;

                            enemy.aiActor.CorpseObject = null;
                            enemy.aiActor.CorpseShadow = false;
                            
                        }
                    }
                }
            }

            if (kills == 6 && !canDoRipper && !isRipperMode)
            {
                AkSoundEngine.PostEvent("Play_WPN_energy_impact_01", gameObject);
                SpawnManager.SpawnVFX((PickupObjectDatabase.GetById(21) as Gun).DefaultModule.chargeProjectiles[4].Projectile.hitEffects.overrideMidairDeathVFX, gun.CurrentOwner.sprite.WorldCenter, Quaternion.Euler(0f, 0f, 0));
                canDoRipper = true;
            }

        }

        public override void OwnedUpdatePlayer(PlayerController owner, GunInventory inventory)
        {
            if(Key(GungeonActions.GungeonActionType.Reload, owner))
            {
                if(owner.CurrentGun.PickupObjectId == ID)
                {
                    if (kills == 6 && canDoRipper && !isRipperMode)
                    {
                        
                        GameManager.Instance.StartCoroutine(RipperMode());
                    }
                }
            }
        }

        private float elapsed = 0;

        private IEnumerator RipperMode()
        {
            PlayerController player = gun.GunPlayerOwner();

            AkSoundEngine.PostEvent("Play_OBJ_bloodybullet_proc_01", gameObject);

            Color cachedOverrideMatColor = SpriteOutlineManager.GetOutlineMaterial(player.sprite).GetColor("_OverrideColor");

            SpriteOutlineManager.GetOutlineMaterial(player.sprite).SetColor("_OverrideColor", new Color(149 / 9, 0, 0, 255));

            //Play_ENM_bombshee_scream_01

            gun.AddStatToGun(PlayerStats.StatType.MovementSpeed, 1.4f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            gun.AddStatToGun(PlayerStats.StatType.RateOfFire, 1.8f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            gun.AddStatToGun(PlayerStats.StatType.ChargeAmountMultiplier, 3.5f, StatModifier.ModifyMethod.MULTIPLICATIVE);
            gun.gameObject.AddComponent<BloodDripFromSword>();
            player.stats.RecalculateStats(player, true);

            canDoRipper = false;
            isRipperMode = true;
            kills = 0;

            elapsed = 0;

            gun.CanBeDropped = false;
            gun.CanBeSold = false;

            player.inventory.GunLocked.SetOverride(Module.MOD_PREFIX + "_Ripper Mode", true);

            yield return new WaitForSeconds(12);

            //Do end effects

            gun.CanBeDropped = true;
            gun.CanBeSold = true;

            player.inventory.GunLocked.SetOverride(Module.MOD_PREFIX + "_Ripper Mode", false);

            gun.RemoveStatFromGun(PlayerStats.StatType.MovementSpeed);
            gun.RemoveStatFromGun(PlayerStats.StatType.RateOfFire);
            gun.RemoveStatFromGun(PlayerStats.StatType.ChargeAmountMultiplier);
            player.stats.RecalculateStats(player, true);

            SpriteOutlineManager.GetOutlineMaterial(player.sprite).SetColor("_OverrideColor", cachedOverrideMatColor);
            Destroy(gun.gameObject.GetComponent<BloodDripFromSword>());
            canDoRipper = false;
            isRipperMode = false;
            kills = 0;

            yield break;
        }

        public bool Key(GungeonActions.GungeonActionType action, PlayerController user)
        {
            return BraveInput.GetInstanceForPlayer(user.PlayerIDX).ActiveActions.GetActionFromType(action).IsPressed;
        }

        

        class BloodDripFromSword : MonoBehaviour
        {
            float elapsed = 0;
            Gun thisGun;
            private void Awake()
            {
                thisGun = gameObject.GetComponent<Gun>();
            }

            private void Update()
            {
                elapsed += Time.deltaTime;
                if(elapsed > .1f)
                {
                    Vector3 vector = thisGun.sprite.WorldBottomLeft.ToVector3ZisY(0f);
                    Vector3 vector2 = thisGun.sprite.WorldTopRight.ToVector3ZisY(0f);
                    float num = (vector2.y - vector.y) * (vector2.x - vector.x);
                    float num2 = 40 * num;
                    int num3 = Mathf.CeilToInt(Mathf.Max(1f, num2 * BraveTime.DeltaTime));
                    int num4 = num3;
                    Vector3 minPosition = vector;
                    Vector3 maxPosition = vector2;
                    Vector3 up = Vector3.up;
                    float angleVariance = 120f;
                    float magnitudeVariance = 0.5f;
                    float? startLifetime = new float?(UnityEngine.Random.Range(1f, 1.65f));
                    GlobalSparksDoer.DoRandomParticleBurst(num4, minPosition, maxPosition, up, angleVariance, magnitudeVariance, null, startLifetime, null, GlobalSparksDoer.SparksType.BLOODY_BLOOD);
                    elapsed = 0;
                }
            }
        }

        class PreventDupeSlicing : MonoBehaviour
        {
            AIActor actor;
            public void AssignDeathEffect()
            {
                actor = gameObject.GetComponent<AIActor>();
                actor.healthHaver.OnPreDeath += OnPreDeath;
            }

            private void OnPreDeath(Vector2 pos)
            {
                Destroy(gameObject);
            }
        }

        class MoveAndKillAfterDelay : MonoBehaviour
        {
            public float duration = .3f;
            private float speed = 5f;

            public Vector2 targetPos;

            public Vector2 startingPos;


            public void StartTimer()
            {
                GameManager.Instance.StartCoroutine(DieTimer());
            }

            private void Update()
            {
                float distanceTraveled = Mathf.Abs(Vector2.Distance(startingPos, targetPos));
                float step = speed * Time.deltaTime * Mathf.Pow(distanceTraveled, 1f/3f);
                
                transform.position = Vector2.MoveTowards(transform.position, targetPos, step / 2);

            }

            private IEnumerator DieTimer()
            {
                yield return new WaitForSeconds(duration);
                Destroy(gameObject);
                yield break;
            }
        }

        public static int ID;
    }

    public class ProjectileIDMarker : MonoBehaviour
    {
        public string ID = "test";
    }

    class ElectrolytePack : PlayerItem
    {
        public static int ID;
        public static void Init()
        {
            string itemName = "Electrolyte Pack";

            string resourceName = "SilverJacket/Resources/Actives/electrolyte_pack";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<ElectrolytePack>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "The Gatorade Eucharist";
            string longDesc = "Instantly allows the user to enter Ripper Mode if the held gun is the HF Blade Lightning. Only for debug purposes.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 10f);

            item.consumable = false;
            item.quality = ItemQuality.EXCLUDED;
            item.sprite.IsPerpendicular = true;
            ID = item.PickupObjectId;
        }


        public override void DoEffect(PlayerController user)
        {
            if(user.CurrentGun.PickupObjectId == HFBladeLightning.ID)
            {
                SpawnManager.SpawnVFX((PickupObjectDatabase.GetById(21) as Gun).DefaultModule.chargeProjectiles[4].Projectile.hitEffects.overrideMidairDeathVFX, user.sprite.WorldCenter, Quaternion.Euler(0f, 0f, 0));
                AkSoundEngine.PostEvent("Play_WPN_energy_impact_01", gameObject);
                user.CurrentGun.gameObject.GetComponent<HFBladeLightning>().kills = 6;
                user.CurrentGun.gameObject.GetComponent<HFBladeLightning>().canDoRipper = true;
            }
        }
    }
}
