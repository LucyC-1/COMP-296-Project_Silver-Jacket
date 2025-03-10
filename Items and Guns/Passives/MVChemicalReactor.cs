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
            string longDesc = "Adds reactions between various status effects;\n" +
                "";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            //Adds the actual passive effect to the item
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, 1, StatModifier.ModifyMethod.ADDITIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Coolness, 1);

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
            private float elapsed = 0;
            private AIActor actor;
        }
    }
}
