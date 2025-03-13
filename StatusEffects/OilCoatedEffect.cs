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
        public static string ID = Module.MOD_PREFIX + "_oilcoated";
        
        private void Start()
        {
            this.AppliesTint = true;
            this.TintColor = new Color(26, 1, 14);
            this.effectIdentifier = ID;
            this.duration = 15;
            //this.OverheadVFX = SpriteBuilder.SpriteFromResource("SilverJacket/Resources/StatusEffects/oil_coated_effect");
            this.PlaysVFXOnActor = true;
        }
        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {
            
            base.OnEffectApplied(actor, effectData, partialAmount);
        }
        public override void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
        {
            base.EffectTick(actor, effectData);
        }
    }

    public class OilCoatedTempImmunity : GameActorEffect
    {
        public static string ID = Module.MOD_PREFIX + "_oilcoated_immunity";
        private void Start()
        {
            this.AppliesTint = false;
            this.effectIdentifier = ID;
            this.duration = 30;
        }
        public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1)
        {
            actor.RemoveEffect(OilCoatedEffect.ID);
            base.OnEffectApplied(actor, effectData, partialAmount);
        }
    }
}
