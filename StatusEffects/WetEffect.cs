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
        private void Start()
        {
            this.AppliesTint = true;
            this.TintColor = new Color(3, 110, 210);
            this.effectIdentifier = Module.MOD_PREFIX + "_wet";
            this.duration = 15;
            this.OverheadVFX = SpriteBuilder.SpriteFromResource("Mod/Resources/Status Effects/wet_effect");
            this.PlaysVFXOnActor = true;
        }
        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {
            ApplyTint(actor);
            base.OnEffectApplied(actor, effectData, partialAmount);
        }
        public override void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
        {
            // Check for Oiled Coated effect
            if(actor.GetEffect(Module.MOD_PREFIX + "_oilcoated") != null)
            {
                actor.ApplyEffect(new OilSlickEffect { });
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
                    actor.ApplyEffect(new WetEffectTempImmunity { });
                }
            }
            if(actor.GetEffect("fire") != null)
            {
                if(!(actor.GetEffect("fire") as GameActorFireEffect).IsGreenFire)
                {
                    actor.RemoveEffect("fire");
                    actor.ApplyEffect(new WetEffectTempImmunity { });
                    SteamCloud.CreateSteamCloud(actor.transform.position);
                    List<AIActor> targets = new List<AIActor>();
                    Action<AIActor, float> InitialTargetting = delegate (AIActor actor, float dist)
                    {
                        targets.Add(actor);
                    };
                    GameManager.Instance.PrimaryPlayer.CurrentRoom.ApplyActionToNearbyEnemies(actor.transform.position, 120, InitialTargetting);
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
                            PlaysVFXOnActor = true,
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
        private void Start()
        {
            this.AppliesTint = false;
            this.effectIdentifier = Module.MOD_PREFIX + "_wet_immunity";
            this.duration = 30;
        }
        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {
            actor.RemoveEffect(Module.MOD_PREFIX + "_wet");
            base.OnEffectApplied(actor, effectData, partialAmount);
        }
    }
}
