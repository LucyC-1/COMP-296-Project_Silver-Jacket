using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.Misc;
using Alexandria.ItemAPI;

namespace SilverJacket
{
    class SteamCloud : BraveBehaviour
    {
        public static List<string> spritePaths = new List<string>
        {
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_001",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_002",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_003",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_004",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_005",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_006",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_007",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_008",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_009",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_010",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_011",
            "SilverJacket/Resources/VFX/SteamRing/steam_ring_start_012",
        };
        public static void Init()
        {
            GameObject obj = BasicVFXCreator.MakeBasicVFX("steam_ring", spritePaths, 10, new IntVector2(53, 53), tk2dBaseSprite.Anchor.MiddleCenter);
            obj.AddComponent<SteamCloud>();
            steamCloudPrefab = obj;
        }
        public static void CreateSteamCloud(Vector2 pos)
        {
            GameObject obj = Instantiate<GameObject>(steamCloudPrefab);
            obj.transform.position = pos;
            obj.SetActive(true);
        }
        public static GameObject steamCloudPrefab;
        private void Start()
        {
            this.gameObject.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("start");    
        }
    }
}
