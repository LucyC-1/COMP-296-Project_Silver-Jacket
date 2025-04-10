using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;
using Dungeonator;
using System.Collections;


namespace SilverJacket
{
    class TacticalArtillery : PlayerItem
    {
        public static int encounterTimes;
        public static int ID;

        public static ItemStats stats = new ItemStats();

        public static void Init()
        {
            string itemName = "Tactical Artillery";

            string resourceName = "SilverJacket/Resources/Actives/tactical_missile";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<TacticalArtillery>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Heavy Air Support";
            string longDesc = "Summons a series of air strikes at the largest cluster of enemies in the room.\n";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 500f);

            item.consumable = false;
            item.quality = ItemQuality.B;

            item.sprite.IsPerpendicular = true;

            ID = item.PickupObjectId;
            stats.name = item.EncounterNameOrDisplayName;
        }

       

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
            GameManager.Instance.StartCoroutine(DoAirStrikes(user));
        }

        private IEnumerator DoAirStrikes(PlayerController user)
        {
            int i = 0;
            do
            {
                ExplosionData explosionData = new ExplosionData { };
                explosionData.CopyFrom(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData);

                explosionData.damageToPlayer = 0;
                AIActor Target = TargetEnemies(user, 150, explosionData.damageRadius);
                if(Target != null)
                {
                    Exploder.Explode(Target.specRigidbody.UnitCenter, explosionData, Vector2.zero);
                }              
                i++;
                yield return new WaitForSeconds(.15f);
            } while (i < 4);
            yield break;
        }

        private List<AIActor> PossibleLargestGrouping = new List<AIActor> { };

        private AIActor TargetEnemies(PlayerController user, float initialRange, float secondaryRange)
        {
            if (user.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
            {
                if (PossibleLargestGrouping.Any())
                {
                    PossibleLargestGrouping.Clear();
                }
                RoomHandler room = user.CurrentRoom;
                List<AIActor> PossibleTargets = new List<AIActor> { };
                AIActor TrueTarget = null;
                Action<AIActor, float> InitialTargetting = delegate (AIActor actor, float dist)
                {
                    PossibleTargets.Add(actor);
                };
                room.ApplyActionToNearbyEnemies(user.specRigidbody.UnitCenter, initialRange, InitialTargetting);
                foreach (AIActor a in PossibleTargets)
                {
                    List<AIActor> nearbyEnemies = new List<AIActor> { };
                    Action<AIActor, float> FindGroups = delegate (AIActor actor, float dist)
                    {
                        nearbyEnemies.Add(actor);
                    };
                    room.ApplyActionToNearbyEnemies(a.specRigidbody.UnitCenter, secondaryRange, FindGroups);
                    if (PossibleLargestGrouping.Count() < nearbyEnemies.Count())
                    {
                        PossibleLargestGrouping = nearbyEnemies;
                        TrueTarget = a;
                    }
                }
                return TrueTarget;
            }
            else
            {
                return null;
            }
        }

    }
}
