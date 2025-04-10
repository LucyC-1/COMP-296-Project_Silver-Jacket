using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Dungeonator;

namespace SilverJacket
{
    class IcebergShavings : PlayerItem
    {
        public static int encounterTimes;
        public static int ID;

        public static ItemStats stats = new ItemStats();

        public static void Init()
        {
            string itemName = "Iceberg Shavings";

            string resourceName = "SilverJacket/Resources/Actives/iceberg_shavings";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<IcebergShavings>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Hie Hie no Mi";
            string longDesc = "Freezes all nearby enemies and creates a pool of ice that slows enemies.\n\nShards of a super-chilled iceberg said to have been made by an 'Ice Man'.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 600);

            item.consumable = false;
            item.quality = ItemQuality.C;
            item.sprite.IsPerpendicular = true;
            ID = item.PickupObjectId;
            stats.name = item.EncounterNameOrDisplayName;
        }

        GoopDefinition iceygoop = new GoopDefinition
        {
            ambientGoopFX = Library.WaterGoop.ambientGoopFX,
            ambientGoopFXChance = Library.WaterGoop.ambientGoopFXChance,
            AppliesSpeedModifier = Library.PlayerFriendlyWebGoop.AppliesSpeedModifier,
            AppliesSpeedModifierContinuously = Library.PlayerFriendlyWebGoop.AppliesSpeedModifierContinuously,
            baseColor32 = Library.WaterGoop.baseColor32,
            CanBeElectrified = Library.WaterGoop.CanBeElectrified,
            CanBeFrozen = true,
            AppliesDamageOverTime = false,
            AppliesCharm = false,
            AppliesCheese = false,
            DrainsAmmo = false,
            CanBeIgnited = false,
            damagePerSecondtoEnemies = 0,
            damagesEnemies = false,
            damagesPlayers = false,
            electrifiedDamagePerSecondToEnemies = Library.WaterGoop.electrifiedDamagePerSecondToEnemies,
            electrifiedDamageToPlayer = Library.WaterGoop.electrifiedDamageToPlayer,
            electrifiedTime = Library.WaterGoop.electrifiedTime,
            damageTypes = CoreDamageTypes.Water,
            eternal = false,
            fadeColor32 = Library.WaterGoop.fadeColor32,
            fadePeriod = Library.WaterGoop.fadePeriod,
            freezeLifespan = Library.WaterGoop.freezeLifespan,
            freezeSpreadTime = Library.WaterGoop.freezeSpreadTime,
            frozenColor32 = Library.WaterGoop.frozenColor32,
            goopTexture = Library.WaterGoop.goopTexture,
            lifespan = Library.WaterGoop.lifespan * 2,
            lifespanRadialReduction = Library.WaterGoop.lifespanRadialReduction,
            overrideOpaqueness = Library.WaterGoop.overrideOpaqueness,
            playerStepsChangeLifetime = Library.WaterGoop.playerStepsChangeLifetime,
            playerStepsLifetime = Library.WaterGoop.playerStepsLifetime,
            prefreezeColor32 = Library.WaterGoop.prefreezeColor32,
            SpeedModifierEffect = Library.PlayerFriendlyWebGoop.SpeedModifierEffect,
            usesLifespan = Library.WaterGoop.usesLifespan,
            usesWaterVfx = Library.WaterGoop.usesWaterVfx,
            usesAmbientGoopFX = Library.WaterGoop.usesAmbientGoopFX,
            usesOverrideOpaqueness = Library.WaterGoop.usesOverrideOpaqueness,
            usesWorldTextureByDefault = Library.WaterGoop.usesWorldTextureByDefault,
            worldTexture = Library.WaterGoop.worldTexture,
        };

        public override void Pickup(PlayerController player)
        {
            if (!m_pickedUpThisRun)
            {
                stats.encounterAmount++;
                Module.UpdateStatList();
            }
            base.Pickup(player);
        }

        public override void DoEffect(PlayerController user)
        {
            RoomHandler room = user.CurrentRoom;
            List<AIActor> targets = new List<AIActor> { };
            Action<AIActor, float> InitialTargetting = delegate (AIActor actor, float dist)
            {
                targets.Add(actor);
            };
            room.ApplyActionToNearbyEnemies(user.sprite.WorldCenter, 4, InitialTargetting);
            foreach(AIActor a in targets)
            {
                a.ApplyEffect((PickupObjectDatabase.GetById(278) as BulletStatusEffectItem).FreezeModifierEffect);
                if (a.healthHaver.IsBoss)
                {
                    a.FreezeAmount = 75;
                }
                else
                {
                    a.FreezeAmount = 101;
                }
            }
            DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(iceygoop).TimedAddGoopCircle(user.sprite.WorldCenter, 5, 6);
            DeadlyDeadlyGoopManager.FreezeGoopsCircle(user.sprite.WorldCenter, 2);
            
        }
    }
}
