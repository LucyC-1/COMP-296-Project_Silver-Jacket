﻿using System;
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
            this.duration = 10;
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
                // Apply OilSlick effect
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
