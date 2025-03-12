using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace SilverJacket
{
    public class OilSlickEffect : GameActorEffect
    {
        private void Start()
        {
            this.AppliesTint = false;
            this.effectIdentifier = Module.MOD_PREFIX + "_oilslick";
            this.duration = 5;
            this.OverheadVFX = SpriteBuilder.SpriteFromResource("Mod/Resources/Status Effects/oilslick_effect");
            
        }
        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {
            actor.knockbackDoer.knockbackMultiplier += 3;
            actor.ApplyEffect(new WetEffectTempImmunity { });
            base.OnEffectApplied(actor, effectData, partialAmount);
        }
        public override void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
        {
            DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(Library.OilDef).TimedAddGoopCircle(actor.specRigidbody.UnitBottomCenter, .5f);
            base.EffectTick(actor, effectData);
        }
        public override void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
        {
            actor.knockbackDoer.knockbackMultiplier -= 3;
            base.OnEffectRemoved(actor, effectData);
        }
        
    }
}
