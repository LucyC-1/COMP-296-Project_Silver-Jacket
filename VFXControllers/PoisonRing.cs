using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.Misc;
using Alexandria.ItemAPI;

namespace SilverJacket
{
    class PoisonRing : BraveBehaviour
    {
        public static List<string> spritePaths = new List<string>
        {
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_001",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_002",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_003",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_004",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_005",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_006",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_007",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_008",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_009",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_010",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_011",
            "SilverJacket/Resources/VFX/PoisonRing/poison_ring_start_012",
        };
        public static void Init()
        {
            //GameObject poisonExplosion = (ETGMod.Databases.Items["mailbox"] as Gun).DefaultModule.finalVolley.projectiles[0].projectiles[2].hitEffects.enemy.effects[0].effects[0].effect;

            GameObject obj = BasicVFXCreator.MakeBasicVFX("poison_ring", spritePaths, 10, new IntVector2(53, 53), tk2dBaseSprite.Anchor.MiddleCenter);
            obj.AddComponent<PoisonRing>();            
            poisonCloudPrefab = obj;
        }
        public static void CreatePoisonCloud(Vector2 pos)
        {
            GameObject obj = Instantiate<GameObject>(poisonCloudPrefab);
            obj.transform.position = pos;
            obj.SetActive(true);
        }
        public static GameObject poisonCloudPrefab;
        private void Start()
        {
            this.gameObject.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("start");
            
        }
    }
}
