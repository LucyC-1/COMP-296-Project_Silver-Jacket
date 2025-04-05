using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace SilverJacket
{
    class BoneParade : PlayerItem
    {
        public static int ID;
        public static void Init()
        {
            string itemName = "Bone Parade";

            string resourceName = "SilverJacket/Resources/Actives/bone_parade";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<BoneParade>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Yo-ho-ho!";
            string longDesc = "Summons 10 slightly-homing bone projectiles that bounce off both enemies and walls.\n\nThe bones of a great pirate. When in flight, you can almost hear them singing.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 600f);

            item.consumable = false;
            item.quality = ItemQuality.A;
            item.sprite.IsPerpendicular = true;
            ID = item.PickupObjectId;

            boneProjectilePrefab = UnityEngine.Object.Instantiate<Projectile>(((Gun)ETGMod.Databases.Items[15]).DefaultModule.projectiles[0]);
            boneProjectilePrefab.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(boneProjectilePrefab.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(boneProjectilePrefab);
            List<string> frameNames = new List<string>
            {
                "bone_projectile_001",
                "bone_projectile_002",
                "bone_projectile_003",
                "bone_projectile_004",
            };
            boneProjectilePrefab.AddAnimationToProjectile(frameNames, 10, Library.ConstructListOfSameValues<IntVector2>(new IntVector2(19, 19), 4), Library.ConstructListOfSameValues<bool>(true, 4), Library.ConstructListOfSameValues<tk2dBaseSprite.Anchor>(tk2dBaseSprite.Anchor.MiddleCenter, 4), Library.ConstructListOfSameValues(false, 4), Library.ConstructListOfSameValues(false, 4), Library.ConstructListOfSameValues<Vector3?>(null, 4), Library.ConstructListOfSameValues<IntVector2?>(null, 4), Library.ConstructListOfSameValues<IntVector2?>(null, 4),Library.ConstructListOfSameValues<Projectile>(null, 4));
            boneProjectilePrefab.shouldRotate = false;
            boneProjectilePrefab.baseData.damage = 6;

            HomingModifier homingModifier = boneProjectilePrefab.gameObject.AddComponent<HomingModifier>();
            homingModifier.HomingRadius = 1000f;
            homingModifier.AngularVelocity = 75f;

            BounceProjModifier bounceProjModifier = boneProjectilePrefab.gameObject.AddComponent<BounceProjModifier>();
            bounceProjModifier.damageMultiplierOnBounce = 1;
            bounceProjModifier.bouncesTrackEnemies = false;
            bounceProjModifier.numberOfBounces = 5;

            boneProjectilePrefab.gameObject.AddComponent<ProjectileBounceRedirect>();

        }

        

        private static Projectile boneProjectilePrefab;

        public override void DoEffect(PlayerController user)
        {
            float angle = 0;
            for(int i = 0; i < 10; i++)
            {
                GameObject gameObject = SpawnManager.SpawnProjectile(boneProjectilePrefab.gameObject, user.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (angle)), true);
                Projectile component = gameObject.GetComponent<Projectile>();
                component.Owner = user;
                component.Shooter = user.specRigidbody;
                component.collidesWithPlayer = false;
                component.UpdateCollisionMask();
                angle += 36;
            }
        }


        class ProjectileBounceRedirect : MonoBehaviour
        {
            private void Start()
            {
                gameObject.GetComponent<Projectile>().OnHitEnemy += OnHitEnemy;
            }
            private void OnHitEnemy(Projectile p, SpeculativeRigidbody enemy, bool fatal)
            {
                BounceProjModifier bounce = gameObject.GetComponent<BounceProjModifier>();
                if (bounce.numberOfBounces > 1)
                {
                    bounce.numberOfBounces--;
                    PierceProjModifier orAddComponent = gameObject.GetOrAddComponent<PierceProjModifier>();
                    orAddComponent.penetratesBreakables = true;
                    orAddComponent.penetration++;
                    Vector2 dirVec = UnityEngine.Random.insideUnitCircle;
                    gameObject.GetComponent<Projectile>().projectile.SendInDirection(dirVec, false, true);
                }
            }
        }

    }
}
