using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace SilverJacket
{
    public static class Library
    {
        public static void DefineGoops()
        {
            var assetBundle = ResourceManager.LoadAssetBundle("shared_auto_001");

            foreach (string text in Library.goops)
            {
                GoopDefinition goopDefinition;
                try
                {
                    GameObject gameObject2 = assetBundle.LoadAsset(text) as GameObject;
                    goopDefinition = gameObject2.GetComponent<GoopDefinition>();
                }
                catch
                {
                    goopDefinition = (assetBundle.LoadAsset(text) as GoopDefinition);
                }
                goopDefinition.name = text.Replace("assets/data/goops/", "").Replace(".asset", "");
                Library.goopDefs.Add(goopDefinition);

            }

            List<GoopDefinition> list = Library.goopDefs;

            FireDef = Library.goopDefs[0];
            OilDef = Library.goopDefs[1];
            PoisonDef = Library.goopDefs[2];
            BlobulonGoopDef = Library.goopDefs[3];
            WebGoop = Library.goopDefs[4];
            WaterGoop = Library.goopDefs[5];

            GoopDefinition midInitWeb = UnityEngine.Object.Instantiate<GoopDefinition>(WebGoop);
            midInitWeb.playerStepsChangeLifetime = false;
            midInitWeb.SpeedModifierEffect = FriendlyWebGoopSpeedMod;
            midInitWeb.CanBeIgnited = false;
            PlayerFriendlyWebGoop = midInitWeb;
            goopDefs.Add(CharmGoopDef);
            goopDefs.Add(GreenFireDef);
            goopDefs.Add(CheeseDef);

        }
        static Gun TripleCrossbow = ETGMod.Databases.Items["triple_crossbow"] as Gun;
        static GameActorSpeedEffect TripleCrossbowEffect = TripleCrossbow.DefaultModule.projectiles[0].speedEffect;
        public static GameActorSpeedEffect FriendlyWebGoopSpeedMod = new GameActorSpeedEffect
        {
            duration = 1,
            TintColor = TripleCrossbowEffect.TintColor,
            DeathTintColor = TripleCrossbowEffect.DeathTintColor,
            effectIdentifier = "FriendlyWebSlow",
            AppliesTint = false,
            AppliesDeathTint = false,
            resistanceType = EffectResistanceType.None,
            SpeedMultiplier = 0.40f,

            //Eh
            OverheadVFX = null,
            AffectsEnemies = true,
            AffectsPlayers = false,
            AppliesOutlineTint = false,
            OutlineTintColor = TripleCrossbowEffect.OutlineTintColor,
            PlaysVFXOnActor = false,
        };
        public static List<GoopDefinition> goopDefs = new List<GoopDefinition> { };
        private static string[] goops = new string[]
        {
            "assets/data/goops/napalmgoopthatworks.asset",
            "assets/data/goops/oil goop.asset",
            "assets/data/goops/poison goop.asset",
            "assets/data/goops/blobulongoop.asset",
            "assets/data/goops/phasewebgoop.asset",
            "assets/data/goops/water goop.asset",

        };

        public static GoopDefinition FireDef;
        public static GoopDefinition OilDef;
        public static GoopDefinition PoisonDef;
        public static GoopDefinition BlobulonGoopDef;
        public static GoopDefinition WebGoop;
        public static GoopDefinition PlayerFriendlyWebGoop;
        public static GoopDefinition WaterGoop;
        public static GoopDefinition CharmGoopDef = PickupObjectDatabase.GetById(310)?.GetComponent<WingsItem>()?.RollGoop;
        public static GoopDefinition GreenFireDef = (PickupObjectDatabase.GetById(698) as Gun).DefaultModule.projectiles[0].GetComponent<GoopModifier>().goopDefinition;
        public static GoopDefinition CheeseDef = (PickupObjectDatabase.GetById(808) as Gun).DefaultModule.projectiles[0].GetComponent<GoopModifier>().goopDefinition;
    }
}
