using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Dungeonator;

namespace SilverJacket
{
    public class OilCoatedEffect : GameActorEffect
    {
        private void Start()
        {
            this.AppliesTint = true;
            this.TintColor = new Color(26, 1, 14);
            this.effectIdentifier = Module.MOD_PREFIX + "_oilcoated";
            this.duration = 10;
            this.OverheadVFX = SpriteBuilder.SpriteFromResource("Mod/Resources/Status Effects/oil_coated_effect");
            this.PlaysVFXOnActor = true;
        }
        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {
            ApplyTint(actor);
            base.OnEffectApplied(actor, effectData, partialAmount);
        }
        public override void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
        {
            base.EffectTick(actor, effectData);
        }
    }

    public class OilCoatedTempImmunity : GameActorEffect
    {
        private void Start()
        {
            this.AppliesTint = false;
            this.effectIdentifier = Module.MOD_PREFIX + "_oilcoated_immunity";
            this.duration = 30;
        }
        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {
            actor.RemoveEffect(Module.MOD_PREFIX + "_oilcoated");
            base.OnEffectApplied(actor, effectData, partialAmount);
        }
    }
}
