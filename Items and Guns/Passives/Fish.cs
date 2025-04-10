using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Collections;

namespace SilverJacket
{
    class Fish : PassiveItem
    {
        public static int encounterTimes;
        public static int ID;

        public static ItemStats stats = new ItemStats();

        public static void Init()
        {

            string itemName = "Fish";

            string resourceName = "SilverJacket/Resources/Passives/fish";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<Fish>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Roll Included.";
            string longDesc = "Dive underwater while rolling, creating a trail of water. Creates a splash that damages and knocks back enemies at the end of a dodge roll.\n\n" +
                "A mutated fish from a far off an irradiated planet. Once sought the Throne That Can Kill The Past, only to become depressed upon learning that no such artifact existed.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            item.quality = PickupObject.ItemQuality.D;
            ID = item.PickupObjectId;
            stats.name = item.EncounterNameOrDisplayName;
        }

        private void OnRoll(PlayerController player)
        {
            GameManager.Instance.StartCoroutine(RollTimer(player.rollStats.GetModifiedTime(player), player));
            player.IsVisible = false;
            this.CanBeDropped = false;
        }

        private void Rolling(PlayerController player)
        {
            DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(Library.WaterGoop).TimedAddGoopCircle(player.sprite.WorldBottomCenter, 1.3f, .25f);
        }

        private IEnumerator RollTimer(float rollTime, PlayerController player)
        {
            yield return new WaitForSeconds(rollTime);
            // do end of roll effect
            player.IsVisible = true;
            List<AIActor> targets = new List<AIActor>();
            Action<AIActor, float> InitialTargetting = delegate (AIActor actor, float dist)
            {
                targets.Add(actor);
            };
            player.CurrentRoom.ApplyActionToNearbyEnemies(player.sprite.WorldCenter, 3f, InitialTargetting);
            if (targets.Any())
            {
                foreach (AIActor a in targets)
                {
                    a.healthHaver.ApplyDamage(5f, Vector2.zero, "splash", CoreDamageTypes.Water);
                    a.knockbackDoer.ApplyKnockback((a.sprite.WorldCenter - player.sprite.WorldCenter), 8);
                }
            }
            GameObject obj = Instantiate<GameObject>(WaveCrashVFX.wavecrashPrefab);
            obj.transform.position = player.sprite.WorldBottomCenter;
            obj.SetActive(true);
            AkSoundEngine.PostEvent("Play_WPN_eyeballgun_impact_01", player.gameObject);
            //player.sprite.renderer.enabled = true;
            
            this.CanBeDropped = true;
            yield break;
        }

        public override void Pickup(PlayerController player)
        {
            player.OnIsRolling += Rolling;
            player.OnPreDodgeRoll += OnRoll;
            if (!m_pickedUpThisRun)
            {
                stats.encounterAmount++;
                Module.UpdateStatList();
            }
            base.Pickup(player);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            player.OnIsRolling -= Rolling;
            player.OnPreDodgeRoll -= OnRoll;
            return base.Drop(player);
        }
    }
}
