using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Dungeonator;

namespace SilverJacket
{
    public class WetEffect : GameActorEffect
    {
        public static string ID = Module.MOD_PREFIX + "_wet";
        private void Start()
        {
            this.AppliesTint = true;
            
            this.effectIdentifier = ID;
            this.duration = 15;
            //this.OverheadVFX = SpriteBuilder.SpriteFromResource("SilverJacket/Resources/StatusEffects/wet_effect");
            this.PlaysVFXOnActor = true;
        }
        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {            
            base.OnEffectApplied(actor, effectData, partialAmount);
        }
        public override void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
        {
            // Check for Oiled Coated effect
            if(actor.GetEffect(OilCoatedEffect.ID) != null)
            {
                actor.ApplyEffect(new OilSlickEffect { effectIdentifier = OilSlickEffect.ID, duration = 5, OverheadVFX = SpriteBuilder.SpriteFromResource("SilverJacket/Resources/StatusEffects/oilslick_effect") });
            }
            if(actor.GetEffect("freeze") != null)
            {
                if(actor.FreezeAmount > 0)
                {
                    if (actor.healthHaver.IsBoss)
                    {
                        actor.FreezeAmount = 75;
                    }
                    else
                    {
                        actor.FreezeAmount = 101;
                    }
                    actor.ApplyEffect(new WetEffectTempImmunity { effectIdentifier = WetEffectTempImmunity.ID, duration = 15 });
                }
            }
            if(actor.GetEffect("fire") != null)
            {
                if(!(actor.GetEffect("fire") as GameActorFireEffect).IsGreenFire)
                {
                    actor.RemoveEffect("fire");
                    actor.ApplyEffect(new WetEffectTempImmunity { effectIdentifier = WetEffectTempImmunity.ID, duration = 15 });
                    SteamCloud.CreateSteamCloud(actor.transform.position);
                    List<AIActor> targets = new List<AIActor>();
                    Action<AIActor, float> InitialTargetting = delegate (AIActor actor, float dist)
                    {
                        targets.Add(actor);
                    };
                    GameManager.Instance.PrimaryPlayer.CurrentRoom.ApplyActionToNearbyEnemies(actor.transform.position, 3, InitialTargetting);
                    foreach(AIActor a in targets)
                    {
                        GameActorSpeedEffect eff = new GameActorSpeedEffect
                        {
                            AffectsEnemies = Library.TripleCrossbowEffect.AffectsEnemies,
                            AffectsPlayers = false,
                            AppliesDeathTint = false,
                            AppliesOutlineTint = false,
                            AppliesTint = false,
                            effectIdentifier = Library.TripleCrossbowEffect.effectIdentifier,
                            resistanceType = Library.TripleCrossbowEffect.resistanceType,
                            SpeedMultiplier = Library.TripleCrossbowEffect.SpeedMultiplier,
                            OverheadVFX = Library.TripleCrossbowEffect.OverheadVFX,
                            
                            duration = 15,                            
                        };
                        a.ApplyEffect(eff);
                    }
                }
            }
            base.EffectTick(actor, effectData);
        }
    }

    public class WetEffectTempImmunity : GameActorEffect
    {
        public static string ID = Module.MOD_PREFIX + "_wet_immunity";
        private void Start()
        {
            this.AppliesTint = false;
            this.effectIdentifier = ID;
            this.duration = 15;
        }
        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {
            actor.RemoveEffect(WetEffect.ID);
            base.OnEffectApplied(actor, effectData, partialAmount);
        }
    }
}
