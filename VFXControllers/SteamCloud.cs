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
        public static void Init()
        {
            GameObject obj = SpriteBuilder.SpriteFromResource("Mod/Resources/VFX/SteamRing/steam_ring_001");
            obj.GetComponent<tk2dSprite>().HeightOffGround = .1f;
            obj.AddAnimation("start", "Mod/Resources/VFX/SteamRing", 10, CompanionBuilder.AnimationType.Idle);
            obj.SetActive(false);
            obj.AddComponent<SteamCloud>();
            FakePrefab.MarkAsFakePrefab(obj);
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
