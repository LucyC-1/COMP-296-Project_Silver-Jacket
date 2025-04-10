using Alexandria.ItemAPI;
using UnityEngine;
using System.Collections;

namespace SilverJacket
{
    class CascadingBullets : PassiveItem
    {
        public static int encounterTimes;
        public static int ID;

        public static ItemStats stats = new ItemStats();
        public static void Init()
        {

            string itemName = "Cascading Bullets";

            string resourceName = "SilverJacket/Resources/Passives/cascading_bullets";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<CascadingBullets>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "An Extra Drop of Bullets";
            string longDesc = "Hitting an enemy has a chance to apply the Cascading effect, making enemies shoot friendly bullets on hit for a short time.\n\n" +
                "A special coating of liquid bullet applied to your bullets, practically doubling your bullet-per-bullet value!";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, Module.MOD_PREFIX);

            item.quality = PickupObject.ItemQuality.A;
            ID = item.PickupObjectId;
            stats.name = item.EncounterNameOrDisplayName;
        }

        private bool bulletsOnCooldown = false;

        public override void Pickup(PlayerController player)
        {
            player.OnDealtDamageContext += OnDidDamage;
            player.PostProcessProjectile += PostProcessProjectile;
            if (!m_pickedUpThisRun)
            {
                stats.encounterAmount++;
                Module.UpdateStatList();
            }
            base.Pickup(player);
        }

        private void PostProcessProjectile(Projectile projectile, float eff)
        {
            if(UnityEngine.Random.value < .25f * eff)
            {
                CascadingEffect effect = new CascadingEffect
                {
                    DamagePerSecondToEnemies = 0,
                    duration = 5,
                    AppliesTint = false,
                    AffectsEnemies = true,
                    OverheadVFX = SpriteBuilder.SpriteFromResource("SilverJacket/Resources/StatusEffects/cascading_effect"),
                    effectIdentifier = Module.MOD_PREFIX + "_cascading",
                };
                projectile.statusEffectsToApply.Add(effect);
                projectile.AppliesPoison = true;
            }
        }

        private void OnDidDamage(PlayerController player, float damage, bool fatal, HealthHaver target)
        {
            if (player)
            {
                if(target.gameActor.GetEffect(Module.MOD_PREFIX + "_cascading") != null)
                {
                    if (!bulletsOnCooldown)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            float angle = 0;
                            switch (i)
                            {
                                case 0:
                                    angle = 0;
                                    break;
                                case 1:
                                    angle = 90;
                                    break;
                                case 2:
                                    angle = 180;
                                    break;
                                case 3:
                                    angle = 270;
                                    break;
                            }
                            Projectile projectile = ((Gun)ETGMod.Databases.Items[404]).DefaultModule.projectiles[0];
                            GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, target.sprite.WorldCenter, Quaternion.Euler(0f, 0f, (angle)), true);
                            Projectile component = gameObject.GetComponent<Projectile>();
                            if (component != null)
                            {
                                component.Owner = base.Owner;
                                component.Shooter = base.Owner.specRigidbody;
                                component.baseData.damage = 3;
                                component.specRigidbody.RegisterSpecificCollisionException(target.specRigidbody);
                                component.UpdateCollisionMask();
                                component.baseData.range *= .5f;
                                
                            }
                        }
                        bulletsOnCooldown = true;
                        GameManager.Instance.StartCoroutine(DoCooldown());
                    }
                }
            }
        }

        private IEnumerator DoCooldown()
        {
            yield return new WaitForSeconds(.2f);
            bulletsOnCooldown = false;
            yield break;
        }

        public override DebrisObject Drop(PlayerController player)
        {
            player.OnDealtDamageContext -= OnDidDamage;
            player.PostProcessProjectile -= PostProcessProjectile;
            if (bulletsOnCooldown)
            {
                GameManager.Instance.StopCoroutine(DoCooldown());
            }
            return base.Drop(player);
        }

        class CascadingEffect : GameActorHealthEffect
        {

        }

    }
}
