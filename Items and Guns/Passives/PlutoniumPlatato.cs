using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Random = UnityEngine.Random;
using System.Collections;
using Dungeonator;

namespace SilverJacket
{
    class PlutoniumPlatato : PassiveItem
    {
        public static int ID;

        public static void Init()
        {
            string itemName = "Plutonium Platato";

            string resourceName = "SilverJacket/Resources/plutonium_platato";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<PlutoniumPlatato>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Consult User Manual.";
            string longDesc = "NOTE: Thuroughly read this entry before continuing the run.\n\n" +
                "On pick up, sets a timer for a random time between 20 and 30 minutes, scaling down depending on the depth of your current floor. The timer does not start until you enter a combat room." +
                "Once you start the timer for the first time, you cannot drop this item until there are 30 seconds or less left on the timer, at which point you will be notified. On leaving a floor the timer is stopped and it will resume again once you enter a combat room." +
                "\n\n If the item is not dropped before the timer reaches zero, the player will be set to 1/2 a heart of health, all non-boss enemies in the room will be killed and no further waves will spawn, bosses will have their health lowered to 15% of their max hp or will be killed if they have less than 15% health left, and this item will be destroyed." +
                "\n\n Your health will not be affected if you are immune to explosions or already at 1/2 a heart. If you are immune to poison, your current health will instead be reduced by 50%, rounding down, to 1/2 a heart at the lowest." +
                "\n\n While held, this item will give you:" +
                "\n - 2x Damage," +
                "\n - 1.20x Movement Speed and Rate of Fire," +
                "\n - A poison projectile fired at a random angle on a cooldown, with the cooldown decreasing based on time remaining, and" +
                "\n - +10% Movement speed and Rate of Fire when the timer reaches 75%, 50%, and 25% duration remaining.";

            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, 2, StatModifier.ModifyMethod.MULTIPLICATIVE);
            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            item.quality = PickupObject.ItemQuality.B;
            ID = item.PickupObjectId;
        }
        [SerializeField]
        public float timeLeft;
        [SerializeField]
        private float initialTime;

        private static Hook FloorExitHook = new Hook(
                typeof(ElevatorDepartureController).GetMethod("DoDeparture", BindingFlags.Public | BindingFlags.Instance),
                typeof(PlutoniumPlatato).GetMethod("OnFloorExit", BindingFlags.Public | BindingFlags.Static)
            );

        [SerializeField]
        private float statBonus = 1.2f;

        [SerializeField]
        private bool doInitialLock = true;

        private bool did25boost = false;
        private bool did50boost = false;
        private bool did75boost = false;

        public override void Update()
        {
            if(pauseTimer == false)
            {
                timeLeft -= Time.deltaTime;
                DoTimeBoostCheck();
                if(timeLeft <= 30 && !isMeltdownCountdownRun)
                {
                    GameManager.Instance.StartCoroutine(BeginMeltdown());
                    this.CanBeDropped = true;
                    this.CanBeSold = true;
                }
                if(timeLeft <= 0)
                {
                    BigBoom();
                }
                if (spawnNextProjectile)
                {
                    GameManager.Instance.StartCoroutine(HandleSpawnProjectile());
                }
                
            }
            base.Update();
        }

        private float projectileCooldown = .25f;
        private bool spawnNextProjectile = true;
        private IEnumerator HandleSpawnProjectile()
        {
            float cooldown = projectileCooldown * GetTimePercentageLeft();
            spawnNextProjectile = false;
            float angle = Random.Range(0, 360);
            Projectile projectile = ((Gun)ETGMod.Databases.Items[15]).DefaultModule.projectiles[0];
            GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, Owner.transform.position, Quaternion.Euler(0f, 0f, (base.Owner.CurrentGun == null) ? 0f : 0f + (angle)), true);
            Projectile component = gameObject.GetComponent<Projectile>();
            if (component != null)
            {
                component.Owner = base.Owner;
                component.Shooter = base.Owner.specRigidbody;
                component.baseData.damage /= 2;
                projectile.statusEffectsToApply.Add((PickupObjectDatabase.GetById(204) as BulletStatusEffectItem).HealthModifierEffect);
                projectile.AppliesPoison = true;
                projectile.AdjustPlayerProjectileTint((PickupObjectDatabase.GetById(204) as BulletStatusEffectItem).TintColor, (PickupObjectDatabase.GetById(204) as BulletStatusEffectItem).TintPriority);
            }
            do
            {
                cooldown -= Time.deltaTime;
            } while (cooldown > 0);
            spawnNextProjectile = true;
            yield break;
        }

        private void BigBoom()
        {
            RoomHandler currentRoom = Owner.CurrentRoom;
            foreach(AIActor a in currentRoom.activeEnemies)
            {
                if (!a.healthHaver.IsBoss)
                {
                    a.healthHaver.ApplyDamage(a.healthHaver.GetMaxHealth() + 1, Vector2.zero, "demon_core", CoreDamageTypes.None, DamageCategory.Unstoppable, true, null, true);
                }
                else
                {
                    if(a.healthHaver.GetCurrentHealthPercentage() > .2f)
                    {
                        a.healthHaver.ForceSetCurrentHealth(a.healthHaver.GetMaxHealth() * .2f);
                    }
                    else
                    {
                        a.healthHaver.ApplyDamage(a.healthHaver.GetMaxHealth() + 1, Vector2.zero, "demon_core", CoreDamageTypes.None, DamageCategory.Unstoppable, true, null, true);
                    }
                }
            }
            if(currentRoom.area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.BOSS)
            {
                currentRoom.ClearReinforcementLayers();
            }
            RemoveStat(PlayerStats.StatType.RateOfFire);
            RemoveStat(PlayerStats.StatType.MovementSpeed);
            bool flag = (PassiveItem.ActiveFlagItems.ContainsKey(Owner) && PassiveItem.ActiveFlagItems[Owner].ContainsKey(typeof(HelmetItem)));
            if (!flag)
            {
                if (Owner.healthHaver.GetDamageModifierForType(CoreDamageTypes.Poison) >= 1)
                {
                    if (Owner.healthHaver.GetCurrentHealth() > .5f)
                    {
                        float half_hp = (Owner.healthHaver.GetCurrentHealth() / 2);
                        half_hp = Mathf.Max(Mathf.Floor(half_hp), .5f);
                        Owner.healthHaver.ForceSetCurrentHealth(half_hp);
                    }
                }
                else
                {
                    Owner.healthHaver.ForceSetCurrentHealth(.5f);
                }
            }

            GoKaboomEffects();
            
            Owner.stats.RecalculateStats(Owner, true, false);
            Owner.DropPassiveItem(this);
        }

        private void GoKaboomEffects()
        {
            float FlashTime = 1f;
            float FlashFadetime = 2f;
            Pixelator.Instance.FadeToColor(FlashFadetime, Color.white, true, FlashTime);
            StickyFrictionManager.Instance.RegisterCustomStickyFriction(0.15f, 1f, false, false); FlashTime = 0.1f;
            GameObject epicwin = UnityEngine.Object.Instantiate<GameObject>(EnemyDatabase.GetOrLoadByGuid("b98b10fca77d469e80fb45f3c5badec5").GetComponent<BossFinalRogueDeathController>().DeathStarExplosionVFX);
            epicwin.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(Owner.sprite.WorldCenter, tk2dBaseSprite.Anchor.LowerCenter);
            epicwin.transform.position = Owner.sprite.WorldCenter.Quantize(0.0625f);
            epicwin.GetComponent<tk2dBaseSprite>().UpdateZDepth();
            for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
            {
                DeadlyDeadlyGoopManager deadlyDeadlyGoopManager = StaticReferenceManager.AllGoops[i];
                deadlyDeadlyGoopManager.RemoveGoopCircle(Owner.sprite.WorldCenter, 25);
            }
            StaticReferenceManager.DestroyAllEnemyProjectiles();
        }

        private bool isMeltdownCountdownRun = false;
        private IEnumerator BeginMeltdown()
        {
            float elapsed = 0;
            float alert_sfx_delay = 5;
            float glow_alpha_percent = 0;
            isMeltdownCountdownRun = true;
            Material m = SpriteOutlineManager.GetOutlineMaterial(Owner.sprite);
            do
            {
                glow_alpha_percent = Mathf.Lerp(glow_alpha_percent, 1, .025f);
                m.SetColor("_OverrideColor", new Color(3 / 100, 110 / 100, 210 / 100, (glow_alpha_percent * 255) / 100));
                if(alert_sfx_delay >= 5)
                {
                    AkSoundEngine.PostEvent("Play_OBJ_time_bell_01", base.gameObject);
                    alert_sfx_delay = 0;
                }
                elapsed += Time.deltaTime;
                alert_sfx_delay += Time.deltaTime;
            } while (elapsed < 30);
            yield break;
        }


        private void DoTimeBoostCheck()
        {
            if (!did75boost)
            {
                if(GetTimePercentageLeft() <= .75f)
                {
                    did75boost = true;
                    RemoveStat(PlayerStats.StatType.RateOfFire);
                    RemoveStat(PlayerStats.StatType.MovementSpeed);
                    Owner.stats.RecalculateStats(Owner, true, false);

                    statBonus += .1f;

                    AddStat(PlayerStats.StatType.RateOfFire, statBonus, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    AddStat(PlayerStats.StatType.MovementSpeed, statBonus, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStats(Owner, true, false);
                }
            }
            if (!did50boost)
            {
                if (GetTimePercentageLeft() <= .5f)
                {
                    did50boost = true;
                    RemoveStat(PlayerStats.StatType.RateOfFire);
                    RemoveStat(PlayerStats.StatType.MovementSpeed);
                    Owner.stats.RecalculateStats(Owner, true, false);

                    statBonus += .1f;

                    AddStat(PlayerStats.StatType.RateOfFire, statBonus, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    AddStat(PlayerStats.StatType.MovementSpeed, statBonus, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStats(Owner, true, false);
                }
            }
            if (!did25boost)
            {
                if (GetTimePercentageLeft() <= .25f)
                {
                    did25boost = true;
                    RemoveStat(PlayerStats.StatType.RateOfFire);
                    RemoveStat(PlayerStats.StatType.MovementSpeed);
                    Owner.stats.RecalculateStats(Owner, true, false);

                    statBonus += .1f;

                    AddStat(PlayerStats.StatType.RateOfFire, statBonus, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    AddStat(PlayerStats.StatType.MovementSpeed, statBonus, StatModifier.ModifyMethod.MULTIPLICATIVE);
                    Owner.stats.RecalculateStats(Owner, true, false);
                }
            }
        }

        private float GetTimePercentageLeft()
        {
            float f = (timeLeft / initialTime);

            return f;
        }

        private void OnCombatEnter()
        {
            //On entering a combat room while the timer is paused, unpause it. 
            if (pauseTimer)
            {
                pauseTimer = false;
            }
            if (doInitialLock)
            {
                this.CanBeDropped = false;
                this.CanBeSold = false;
                doInitialLock = false;
            }
        }

        public static void OnFloorExit(Action<ElevatorDepartureController> orig, ElevatorDepartureController self)
        {
            // if either the primary player has this item or if the secondary player exists and has this passive. ensures the item's code only runs when a player has this passive
            bool flag1 = GameManager.Instance.PrimaryPlayer.HasPassiveItem(ID);
            bool flag2 = (GameManager.Instance.SecondaryPlayer != null && GameManager.Instance.SecondaryPlayer.HasPassiveItem(ID));
            if (flag1 || flag2)
            {
                //pauses the timer of the item to ensure it doesnt run during a level transition.
                if (flag1)
                {
                    foreach(PassiveItem p in GameManager.Instance.PrimaryPlayer.passiveItems)
                    {
                        if (p.PickupObjectId == ID)
                        {
                            (p as PlutoniumPlatato).pauseTimer = true;
                        }
                    }
                }
                if (flag2)
                {
                    foreach (PassiveItem p in GameManager.Instance.SecondaryPlayer.passiveItems)
                    {
                        if (p.PickupObjectId == ID)
                        {
                            (p as PlutoniumPlatato).pauseTimer = true;
                        }
                    }
                }
            }
            orig(self);
        }

        public bool pauseTimer = true;

        private void HandleInitialPickup()
        {
            RadialSlowInterface Rad = new RadialSlowInterface
            {
                RadialSlowHoldTime = 5f,
                RadialSlowOutTime = 2f,
                RadialSlowTimeModifier = 0.2f,
                DoesSepia = false,
                UpdatesForNewEnemies = true,
                audioEvent = "Play_OBJ_time_bell_01",
            };
            pauseTimer = true;
            Rad.DoRadialSlow(Owner.transform.position, Owner.CurrentRoom);
            TextBoxManager.ShowThoughtBubble(Owner.transform.position, Owner.transform, 10, "I should check this item's ammonomicon entry before I leave the room.", true);
            timeLeft = Random.Range((20f * 60f), (30f * 60f));
            float current_level_time_reduc_m = 1;
            switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
            {
                case GlobalDungeonData.ValidTilesets.CASTLEGEON:
                    current_level_time_reduc_m = 1;
                    break;
                case GlobalDungeonData.ValidTilesets.SEWERGEON:
                    current_level_time_reduc_m = .95f;
                    break;
                case GlobalDungeonData.ValidTilesets.GUNGEON:
                    current_level_time_reduc_m = .9f;
                    break;
                case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
                    current_level_time_reduc_m = .85f;
                    break;
                case GlobalDungeonData.ValidTilesets.MINEGEON:
                    current_level_time_reduc_m = .8f;
                    break;
                case GlobalDungeonData.ValidTilesets.RATGEON:
                    current_level_time_reduc_m = .75f;
                    break;
                case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
                    current_level_time_reduc_m = .7f;
                    break;
                case GlobalDungeonData.ValidTilesets.OFFICEGEON:
                    current_level_time_reduc_m = .7f;
                    break;
                case GlobalDungeonData.ValidTilesets.FORGEGEON:
                    current_level_time_reduc_m = .6f;
                    break;
                case GlobalDungeonData.ValidTilesets.HELLGEON:
                    current_level_time_reduc_m = .5f;
                    break;
                case GlobalDungeonData.ValidTilesets.JUNGLEGEON:
                    current_level_time_reduc_m = .95f;
                    break;
                case GlobalDungeonData.ValidTilesets.BELLYGEON:
                    current_level_time_reduc_m = .85f;
                    break;
                case GlobalDungeonData.ValidTilesets.WESTGEON:
                    current_level_time_reduc_m = .7f;
                    break;
                default:
                    current_level_time_reduc_m = 1;
                    break;       
            }
            timeLeft *= current_level_time_reduc_m;
            initialTime = timeLeft;
            doInitialLock = true;
        }

        private void NewLevelLoaded()
        {
            pauseTimer = false;
        }

        public override void Pickup(PlayerController player)
        {
            if (!m_pickedUpThisRun)
            {
                HandleInitialPickup();
            }
            AddStat(PlayerStats.StatType.RateOfFire, statBonus, StatModifier.ModifyMethod.MULTIPLICATIVE);
            AddStat(PlayerStats.StatType.MovementSpeed, statBonus, StatModifier.ModifyMethod.MULTIPLICATIVE);
            player.stats.RecalculateStats(player, true, false);
            player.OnEnteredCombat += OnCombatEnter;
            GameManager.Instance.OnNewLevelFullyLoaded += NewLevelLoaded;
            base.Pickup(player);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            player.OnEnteredCombat -= OnCombatEnter;
            GameManager.Instance.OnNewLevelFullyLoaded -= NewLevelLoaded;
            RemoveStat(PlayerStats.StatType.RateOfFire);
            RemoveStat(PlayerStats.StatType.MovementSpeed);
            player.stats.RecalculateStats(player, true, false);
            pauseTimer = true;
            if (isMeltdownCountdownRun)
            {
                GameManager.Instance.StopCoroutine(BeginMeltdown());
                Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(player.sprite);
                outlineMaterial.SetColor("_OverrideColor", new Color(0, 0, 0, 0));
            }
            if(timeLeft <= 0)
            {
                Destroy(this, 1);
            }
            return base.Drop(player);
        }

        private void AddStat(PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            StatModifier modifier = new StatModifier
            {
                amount = amount,
                statToBoost = statType,
                modifyType = method
            };
            if (this.passiveStatModifiers == null)
                this.passiveStatModifiers = new StatModifier[] { modifier };
            else
                this.passiveStatModifiers = this.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
        }

        private void RemoveStat(PlayerStats.StatType statType)
        {
            var newModifiers = new List<StatModifier>();
            for (int i = 0; i < passiveStatModifiers.Length; i++)
            {
                if (passiveStatModifiers[i].statToBoost != statType)
                    newModifiers.Add(passiveStatModifiers[i]);
            }
            this.passiveStatModifiers = newModifiers.ToArray();
        }
    }

    class DebugScrewdriver : PlayerItem
    {
        public static int ID;
        public static void Init()
        {
            string itemName = "Debug Screwdriver";

            string resourceName = "SilverJacket/Resources/Actives/debug_screwdriver";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<DebugScrewdriver>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Uh oh.";
            string longDesc = "Causes the Plutonium Platato to go kaboomie. Only for debug purposes.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 10f);

            item.consumable = false;
            item.quality = ItemQuality.EXCLUDED;
            item.sprite.IsPerpendicular = true;
            ID = item.PickupObjectId;
        }


        public override void DoEffect(PlayerController user)
        {
            foreach(PassiveItem p in user.passiveItems)
            {
                if(p.PickupObjectId == PlutoniumPlatato.ID)
                {
                    (p as PlutoniumPlatato).timeLeft = 10;
                }
            }
        }
    }
}
