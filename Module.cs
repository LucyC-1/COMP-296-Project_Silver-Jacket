using BepInEx;
using Alexandria;
using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.IO;


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
        public static string filePath;

        public static List<ItemStats> itemStatList = new List<ItemStats> { };

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

            filePath = this.FolderPath();

            ETGModConsole.Commands.AddGroup(MOD_PREFIX, args =>
            {
            });

            //where items and guns get initialised
            //ExamplePassive.Init();

            // Passives -----

            MVChemicalReactor.Init();
            PlutoniumPlatato.Init();
            Fish.Init();
            CascadingBullets.Init();
            WavecrashRounds.Init();

            itemStatList.Add(MVChemicalReactor.stats);
            itemStatList.Add(PlutoniumPlatato.stats);
            itemStatList.Add(Fish.stats);
            itemStatList.Add(WavecrashRounds.stats);

            // Actives -----

            IcebergShavings.Init();
            BoneParade.Init();
            TacticalArtillery.Init();

            itemStatList.Add(IcebergShavings.stats);
            itemStatList.Add(BoneParade.stats);
            itemStatList.Add(TacticalArtillery.stats);

            // Guns -----

            AGGun.Add();
            BreachFist.Add();
            FerrymanOar.Add();
            HFBladeLightning.Add();
            HookAndGut.Add();
            DollArm.Add();
            Ripper.Add();
            TheRightAngle.Add();

            itemStatList.Add(AGGun.stats);
            itemStatList.Add(BreachFist.stats);
            itemStatList.Add(FerrymanOar.stats);
            itemStatList.Add(HFBladeLightning.stats);
            itemStatList.Add(HookAndGut.stats);
            itemStatList.Add(DollArm.stats);
            itemStatList.Add(Ripper.stats);
            itemStatList.Add(TheRightAngle.stats);

            GetStats();
            // Debug Items -----

            DebugScrewdriver.Init();
            ElectrolytePack.Init();

            ETGModConsole.Commands.GetGroup(MOD_PREFIX).AddUnit("read_stats", PrintStats);

            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);
        }


        public static JObject UpdateStatList()
        {
            string path = Path.Combine(ETGMod.ResourcesDirectory, "../SilverJacketData/");
            string file = Path.Combine(path, "ItemStats.json");
            JObject data = new JObject();
            foreach (ItemStats stat in itemStatList)
            {
                data.Add(stat.name, stat.encounterAmount);
            }
            File.WriteAllText(file, data.ToString());
            return data;
        }

        private void PrintStats(string[] args)
        {
            ETGModConsole.Log("Stats: ");
            ETGModConsole.Log("----------------");
            ETGModConsole.Log(ReadStats().ToString());         
            ETGModConsole.Log("----------------");
        }

        private static JObject ReadStats()
        {
            JObject data = new JObject();
            
            string path = Path.Combine(ETGMod.ResourcesDirectory, "../SilverJacketData/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string file = Path.Combine(path, "ItemStats.json");
            if (!File.Exists(file))
            {
                JObject initData = UpdateStatList();
            }
            data = JObject.Parse(File.ReadAllText(file));
            return data;
        }

        private void GetStats()
        {
            JObject data = ReadStats();
            foreach(ItemStats stat in itemStatList)
            {
                stat.encounterAmount = data.GetValue(stat.name).Value<int>();
            }
        }

        

        public static void Log(string text, string color="#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }

    }

    public class ItemStats
    {
        public ItemStats()
        {
            encounterAmount = 0;
            name = "";
        }
        public int encounterAmount = 0;
        public string name = "";
    }
}
