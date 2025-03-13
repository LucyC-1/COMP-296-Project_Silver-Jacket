using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace SilverJacket
{
    class PoisonFumeTempImmunity : GameActorEffect
    {
        public static string ID = Module.MOD_PREFIX + "_poison_fume_immunity";
        private void Start()
        {
            this.AppliesTint = false;
            this.effectIdentifier = ID;
            this.duration = 15;
        }
    }
}
