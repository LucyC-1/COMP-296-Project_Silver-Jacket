using BepInEx;
using Alexandria;
using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace SilverJacket
{
    [BepInDependency(Alexandria.Alexandria.GUID)] // this mod depends on the Alexandria API: https://enter-the-gungeon.thunderstore.io/package/Alexandria/Alexandria/
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Module : BaseUnityPlugin
    {
        public const string GUID = "lucyc.etg.silverjacket";
        public const string NAME = "Silver Jacket";
        public const string VERSION = "1.0.0";
        public const string TEXT_COLOR = "#606d81";
        public const string MOD_PREFIX = "slvjckt";

        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {

            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            Library.DefineGoops();
            Library.InitVFX();
            //where items and guns get initialised
            //ExamplePassive.Init();

            // Passives -----

            MVChemicalReactor.Init();
            PlutoniumPlatato.Init();
            DebugScrewdriver.Init();
            Fish.Init();
            CascadingBullets.Init();
            WavecrashRounds.Init();

            // Actives -----

            IcebergShavings.Init();
            BoneParade.Init();
            TacticalArtillery.Init();

            // Guns -----

            AGGun.Add();
            BreachFist.Add();


            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);
        }

        public static void Log(string text, string color="#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
    }
}
