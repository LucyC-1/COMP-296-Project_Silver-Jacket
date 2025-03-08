using System.Collections.Generic;
using Alexandria.ItemAPI;
using UnityEngine;
using System.Linq;
using System;
using System.Reflection;
namespace SilverJacket
{
    class ActiveTemplate : PlayerItem
    {
        public static int ID;
        public static void Init()
        {
            string itemName = " ";

            string resourceName = "[Resource file path]";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<ActiveTemplate>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = " ";
            string longDesc = " ";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Timed, 10f);

            item.consumable = false;
            item.quality = ItemQuality.C;
            item.sprite.IsPerpendicular = true;
            ID = item.PickupObjectId;
        }

        
        public override void DoEffect(PlayerController user)
        {

        }

    }
}

