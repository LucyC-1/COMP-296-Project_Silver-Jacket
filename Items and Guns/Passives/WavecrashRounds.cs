using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Random = UnityEngine.Random;

namespace SilverJacket
{
    class WavecrashRounds : PassiveItem
    {
        public static int ID;
        //Call this method from the Start() method of your ETGModule extension
        public static void Init()
        {

            //The name of the item
            string itemName = "Wavecrash Rounds";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "SilverJacket/Resources/Passives/wavecrash_rounds";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<WavecrashRounds>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "High Tide";
            string longDesc = "Hitting an enemy has a chance to knockback and damage nearby enemies.\n\n" +
                "It is said these bullets were made with the raging fury of Gunymede's oceans.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
            
            ID = item.PickupObjectId;
        }

        


        public override void Pickup(PlayerController player)
        {
            player.PostProcessProjectile += PostProcessProjectile;
            base.Pickup(player);
        }

        private void PostProcessProjectile(Projectile projectile, float eff)
        {
            if(Random.value < (.3f * eff))
            {
                projectile.OnHitEnemy += ProjHitEnemy;
            }
        }

        private void ProjHitEnemy(Projectile projectile, SpeculativeRigidbody enemy, bool fatal)
        {
            ExplosionData explosionData = new ExplosionData { };
            explosionData.CopyFrom(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData);
            explosionData.damageToPlayer = 0;
            explosionData.doDestroyProjectiles = false;
            explosionData.playDefaultSFX = false;
            explosionData.doScreenShake = false;
            explosionData.damage = 8;
            explosionData.effect = Instantiate<GameObject>(WaveCrashVFX.wavecrashPrefab);
            explosionData.ignoreList.Add(enemy);
            enemy.healthHaver.ApplyDamage(8, Vector2.zero, Owner.name, CoreDamageTypes.Water);
            Exploder.Explode(enemy.sprite.WorldCenter, explosionData, Vector2.zero);

            if (Owner.HasPassiveItem(MVChemicalReactor.ID))
            {
                if (enemy.aiActor.GetEffect(WetEffectTempImmunity.ID) == null)
                {
                    WetEffect wet = new WetEffect
                    {
                        AppliesTint = true,
                        TintColor = new Color(3 / 100, 110 / 100, 210 / 100),
                        effectIdentifier = WetEffect.ID,
                        duration = 15,
                        OverheadVFX = SpriteBuilder.SpriteFromResource("SilverJacket/Resources/StatusEffects/wet_effect"),

                    };
                    enemy.aiActor.ApplyEffect(wet);
                }
            }

            AkSoundEngine.PostEvent("Play_ENV_water_splash_01", gameObject);
        }
        
        public override DebrisObject Drop(PlayerController player)
        {
            player.PostProcessProjectile -= PostProcessProjectile;
            return base.Drop(player);
        }

    }

    public class WaveCrashVFX : BraveBehaviour
    {
        public static GameObject wavecrashPrefab;

        public static void InitVFX()
        {
            List<string> spritePaths = new List<string>
                {
                    "SilverJacket/Resources/VFX/Wavecrash/wavecrash_start_001",
                    "SilverJacket/Resources/VFX/Wavecrash/wavecrash_start_002",
                    "SilverJacket/Resources/VFX/Wavecrash/wavecrash_start_003",
                    "SilverJacket/Resources/VFX/Wavecrash/wavecrash_start_004",
                };
            GameObject obj = BasicVFXCreator.MakeBasicVFX("wavecrash", spritePaths, 10, new IntVector2(50, 19), tk2dBaseSprite.Anchor.MiddleCenter);
            obj.AddComponent<WaveCrashVFX>();
            wavecrashPrefab = obj;

        }

        private void Start()
        {
            this.gameObject.SetActive(true);
            this.gameObject.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("start");
        }
    }
}
