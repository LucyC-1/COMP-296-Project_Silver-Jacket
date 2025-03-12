using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace SilverJacket
{
    class MVChemicalReactor : PassiveItem
    {
        public static int ID;
        public static void Init()
        {
            string itemName = "MV Chemical Reactor";

            string resourceName = "Mod/Resources/Passives/mv_chemical_reactor";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<MVChemicalReactor>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Greg-ified!";
            string longDesc = "Adds reactions between various status effects:\n\n" +
                "Enemies standing in water or damaged by water attacks will give them the wet effect.\n" +
                "Enemies standing in oil will get the oil coated effect.\n\n" +
                "- Oil Coated + Wet = Oilslick; Oilslick increases knockback taken and causes the enemy to leave a trail of oil. Gives a temporary immunity to Wet.\n" +
                "- Wet + Freeze = Crystal Nucleation; Crystal Nucleation causes non-boss enemies to become instantly frozen. Gives a temporary immunity to Wet.\n" +
                "- Wet + Fire = Steam Cloud; Steam Cloud slows nearby enemies and extinguishes the enemy. Gives a temporary immunity to Wet.\n" +
                "- Freeze + Fire = Steam Explosion; Ceates an explosion. Removes extinguishes and thaws the enemy.\n" +
                "- Fire + Poison = Toxic Fumes; Toxic Fumes poisons all enemies nearby.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);
            

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
            ID = item.PickupObjectId;
        }

        private void OnActorStart(AIActor actor)
        {
            actor.gameObject.AddComponent<HandleGoopsAndEffects>();
        }
        

        public override void Pickup(PlayerController player)
        {
            ETGMod.AIActor.OnPreStart += OnActorStart;
            base.Pickup(player);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            ETGMod.AIActor.OnPreStart -= OnActorStart;
            return base.Drop(player);
        }

        class HandleGoopsAndEffects : MonoBehaviour
        {
            private void Start()
            {
                if (gameObject.GetComponent<AIActor>())
                {
                    actor = gameObject.GetComponent<AIActor>();
                }
                actor.healthHaver.OnDamaged += OnDamaged;
            }
            private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
            {
                if ((damageTypes & CoreDamageTypes.Water) == CoreDamageTypes.Water && actor.GetEffect(Module.MOD_PREFIX + "_wet_immunity") == null) 
                {
                    actor.ApplyEffect(new WetEffect { });
                }
            }
            private void Update()
            {
                elapsed += Time.deltaTime;
                CheckStatuses();
                if (elapsed >= .1f) // this causes the game to only run the check for goops every .1 seconds, reducing lag
                {
                    elapsed = 0;
                    for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
                    {
                        // if the enemy's current position is in a goop
                        if (StaticReferenceManager.AllGoops[i].IsPositionInGoop((Vector2)actor.transform.position) == true)
                        {
                            // check if the current goop at the position is either water or oil
                            DeadlyDeadlyGoopManager goopM = StaticReferenceManager.AllGoops[i];
                            if(goopM.goopDefinition == Library.WaterGoop)
                            {
                                //check for immunity
                                if (actor.GetEffect(Module.MOD_PREFIX + "_wet_immunity") == null)
                                {
                                    
                                    actor.ApplyEffect(new WetEffect { });
                                }
                                
                            }
                            else if (goopM.goopDefinition == Library.OilDef)
                            {
                                //check for immunity
                                if (actor.GetEffect(Module.MOD_PREFIX + "_oilcoated_immunity") == null)
                                {
                                    actor.ApplyEffect(new OilCoatedEffect { });
                                }
                    
                            }

                        }
                    }
                    
                }
            }
            private void CheckStatuses()
            {
                if(actor.m_activeEffects.Count == 0)
                {
                    return;
                }
                if(actor.GetEffect("fire") != null)
                {
                    if(actor.GetEffect("poison") != null)
                    {
                        if (actor.GetEffect(Module.MOD_PREFIX + "_poison_fume_immunity") == null)
                        {
                            actor.ApplyEffect(new PoisonFumeTempImmunity { });
                        }
                    }
                    if(actor.GetEffect("freeze") != null)
                    {
                        ExplosionData explosionData = new ExplosionData { };
                        explosionData.CopyFrom(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData);
                        explosionData.damageToPlayer = 0;
                        actor.RemoveEffect("fire");
                        actor.RemoveEffect("freeze");
                    }
                }
            }
            private float elapsed = 0;
            private AIActor actor;
        }
    }
}
