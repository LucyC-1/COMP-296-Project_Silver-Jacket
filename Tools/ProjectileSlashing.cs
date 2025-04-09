using Alexandria.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections.ObjectModel;
using Alexandria.ItemAPI;
using Dungeonator;

namespace SilverJacket
{
    public class ProjectileSlashingBehaviourForked : MonoBehaviour
    {
        public ProjectileSlashingBehaviourForked()
        {
            DestroyBaseAfterFirstSlash = false;
            timeBetweenSlashes = 1;
            initialDelay = 0f;
            slashParameters = ScriptableObject.CreateInstance<SlashDataForked>();
            SlashDamageUsesBaseProjectileDamage = true;
            timeBetweenCustomSequenceSlashes = 0.15f;
            customSequence = null;
            angleVariance = 0;
        }
        private void Start()
        {
            this.m_projectile = base.GetComponent<Projectile>();
            timer = initialDelay;
            if (this.m_projectile.Owner && this.m_projectile.Owner is PlayerController) this.owner = this.m_projectile.Owner as PlayerController;
        }
        private void Update()
        {
            if (this.m_projectile)
            {
                if (timer > 0f) { timer -= BraveTime.DeltaTime; }
                else { m_projectile.StartCoroutine(DoAttackSequence()); }
            }
        }
        private IEnumerator DoAttackSequence()
        {
            if (customSequence != null && customSequence.Count > 0)
            {
                foreach (float angle in customSequence)
                {
                    DoSlash(angle);
                    yield return new WaitForSeconds(timeBetweenCustomSequenceSlashes);
                }
            }
            else { DoSlash(0); }

            timer = timeBetweenSlashes;
            if (DestroyBaseAfterFirstSlash) StartCoroutine(Suicide());
            yield break;
        }
        private void DoSlash(float angle)
        {
            Projectile proj = this.m_projectile;
            List<GameActorEffect> effects = new List<GameActorEffect>();
            effects.AddRange(proj.GetFullListOfStatusEffects(true));

            SlashDataForked instSlash = SlashDataForked.CloneSlashData(slashParameters);

            if (SlashDamageUsesBaseProjectileDamage)
            {
                instSlash.damage = this.m_projectile.baseData.damage;
                instSlash.bossDamageMult = this.m_projectile.BossDamageMultiplier;
                instSlash.jammedDamageMult = this.m_projectile.BlackPhantomDamageMultiplier;
                instSlash.enemyKnockbackForce = this.m_projectile.baseData.force;
            }
            instSlash.OnHitTarget += SlashHitTarget;
            instSlash.OnHitBullet += SlashHitBullet;
            instSlash.OnHitMajorBreakable += SlashHitMajorBreakable;
            instSlash.OnHitMinorBreakable += SlashHitMinorBreakable;
            instSlash.OnHitTargetDamageContext += SlashHitTargetDamageContext;

            angle += UnityEngine.Random.Range(angleVariance, -angleVariance);

            SlashDoerForked.DoSwordSlash(this.m_projectile.specRigidbody.UnitCenter, (this.m_projectile.Direction.ToAngle() + angle), owner, instSlash);
        }
        private IEnumerator Suicide()
        {
            yield return null;
            if (DestroysOnlyComponentAfterFirstSlash == true) { Destroy(this); }
            else
            {
                UnityEngine.Object.Destroy(this.m_projectile.gameObject);
            }
            yield break;
        }
        /// <summary>
        /// Called when the slash hits a GameActor. Can be overridden for custom effects.
        /// </summary>
        /// <param name="target">The game actor that has been hit by the slash.</param>
        /// <param name="fatal">Whether or not the slash killed the actor it hit.</param>
        public virtual void SlashHitTarget(GameActor target, bool fatal) { }
        /// <summary>
        /// Called when the slash hits a projectile.
        /// </summary>
        /// <param name="target">The projectile that has been hit by the slash.</param>
        public virtual void SlashHitBullet(Projectile target) { }
        /// <summary>
        /// Called when the slash hits a Minor breakable object
        /// </summary>
        /// <param name="target">The object that has been hit by the slash.</param>
        public virtual void SlashHitMinorBreakable(MinorBreakable target) { }
        /// <summary>
        /// Called when the slash hits a Major breakable object
        /// </summary>
        /// <param name="target">The object that has been hit by the slash.</param>
        public virtual void SlashHitMajorBreakable(MajorBreakable target) { }
        /// <summary>
        /// Called when the slash hits a GameActor. Also includes the damage done.
        /// </summary>
        /// <param name="target">The game actor that has been hit by the slash.</param>
        /// <param name="damageDealt">The damage dealt.</param>
        /// <param name="fatal">Whether or not the slash killed the actor it hit.</param>
        public virtual void SlashHitTargetDamageContext(GameActor target, float damageDealt, bool fatal) { }

        /// <summary>
        /// How long should the projectile wait after spawning before doing it's first slash. Zero by default, meaning it occurs instantly.
        /// </summary>
        public float initialDelay;
        private float timer;
        /// <summary>
        /// How long the projectile will wait between performing subsequent slashes after the first.
        /// </summary>
        public float timeBetweenSlashes;
        /// <summary>
        /// If true, the slash's damage, boss damage multiplier, jammed damage multiplier, and knockback stats will be equal to the base projectile's stats.
        /// </summary>
        public bool SlashDamageUsesBaseProjectileDamage;
        /// <summary>
        /// If true, the base projectile will be erased after performing it's first slash/sequence of slashes.
        /// </summary>
        public bool DestroyBaseAfterFirstSlash;
        /// <summary>
        /// If true, the base projectile will only erase the slashing COMPONENT after performing it's first slash/sequence of slashes.
        /// </summary>
        public bool DestroysOnlyComponentAfterFirstSlash = false;
        /// <summary>
        /// The data which defines the exact nature of the slash created.
        /// </summary>
        public SlashDataForked slashParameters;
        private Projectile m_projectile;
        private PlayerController owner;

        /// <summary>
        /// The time between slashes in a custom sequence. Only works if customSequence is set.
        /// </summary>
        public float timeBetweenCustomSequenceSlashes;
        /// <summary>
        /// A list of angles (0-360) where 0 is the projectile's direction of travel. If set, when performing a slash the projectile will instead perform a sequence of slashes corresponding to the angles in the sequence.
        /// </summary>
        public List<float> customSequence;
        /// <summary>
        /// If set, the precise direction of the slash relative to the base projectile's direction will be able to vary by up to that number of degrees in either direction.
        /// </summary>
        public float angleVariance;
    }

    public class SlashDataForked : ScriptableObject
    {
        public static SlashDataForked CloneSlashData(SlashDataForked original)
        {
            SlashDataForked newData = ScriptableObject.CreateInstance<SlashDataForked>();
            newData.doVFX = original.doVFX;
            newData.VFX = original.VFX;
            newData.doHitVFX = original.doHitVFX;
            newData.hitVFX = original.hitVFX;
            newData.projInteractMode = original.projInteractMode;
            newData.playerKnockbackForce = original.playerKnockbackForce;
            newData.enemyKnockbackForce = original.enemyKnockbackForce;
            newData.statusEffects = original.statusEffects;
            newData.jammedDamageMult = original.jammedDamageMult;
            newData.bossDamageMult = original.bossDamageMult;
            newData.doOnSlash = original.doOnSlash;
            newData.doPostProcessSlash = original.doPostProcessSlash;
            newData.slashRange = original.slashRange;
            newData.slashDegrees = original.slashDegrees;
            newData.damage = original.damage;
            newData.damagesBreakables = original.damagesBreakables;
            newData.soundEvent = original.soundEvent;
            newData.OnHitTarget = original.OnHitTarget;
            newData.OnHitBullet = original.OnHitBullet;
            newData.OnHitMinorBreakable = original.OnHitMinorBreakable;
            newData.OnHitMajorBreakable = original.OnHitMajorBreakable;
            newData.OnHitTargetDamageContext = original.OnHitTargetDamageContext;
            return newData;
        }

        public bool doVFX = true;
        public VFXPool VFX = (PickupObjectDatabase.GetById(417) as Gun).muzzleFlashEffects;
        public bool doHitVFX = true;
        public VFXPool hitVFX = (PickupObjectDatabase.GetById(417) as Gun).DefaultModule.projectiles[0].hitEffects.enemy;
        public SlashDoerForked.ProjInteractMode projInteractMode = SlashDoerForked.ProjInteractMode.IGNORE;
        public float playerKnockbackForce = 5;
        public float enemyKnockbackForce = 10;
        public List<GameActorEffect> statusEffects = new List<GameActorEffect>();
        public float jammedDamageMult = 1;
        public float bossDamageMult = 1;
        public bool doOnSlash = true;
        public bool doPostProcessSlash = true;
        public float slashRange = 2.5f;
        public float slashDegrees = 90f;
        public float damage = 5f;
        public bool damagesBreakables = true;
        public string soundEvent = "Play_WPN_blasphemy_shot_01";
        public Action<GameActor, bool> OnHitTarget = null;
        public Action<Projectile> OnHitBullet = null;
        public Action<MinorBreakable> OnHitMinorBreakable = null;
        public Action<MajorBreakable> OnHitMajorBreakable = null;
        public Action<GameActor, float, bool> OnHitTargetDamageContext = null;

    }
    public class SlashDoerForked
    {
        public static void DoSwordSlash(
            Vector2 position,
            float angle,
            GameActor owner,
            SlashDataForked slashParameters,
            Transform parentTransform = null)
        {
           
            if (slashParameters.doVFX && slashParameters.VFX != null) slashParameters.VFX.SpawnAtPosition(position, angle, parentTransform, null, null, -0.05f);
            if (!string.IsNullOrEmpty(slashParameters.soundEvent) && owner != null && owner.gameObject != null) AkSoundEngine.PostEvent(slashParameters.soundEvent, owner.gameObject);
            GameManager.Instance.StartCoroutine(HandleSlash(position, angle, owner, slashParameters));
        }
        private static IEnumerator HandleSlash(Vector2 position, float angle, GameActor owner, SlashDataForked slashParameters)
        {
            int slashId = Time.frameCount;
            List<SpeculativeRigidbody> alreadyHit = new List<SpeculativeRigidbody>();
            if (slashParameters.playerKnockbackForce != 0f && owner != null) owner.knockbackDoer.ApplyKnockback(BraveMathCollege.DegreesToVector(angle, 1f), slashParameters.playerKnockbackForce, 0.25f, false);
            float ela = 0f;
            while (ela < 0.2f)
            {
                ela += BraveTime.DeltaTime;
                HandleHeroSwordSlash(alreadyHit, position, angle, slashId, owner, slashParameters);
                yield return null;
            }
            
            yield break;
        }



        public enum ProjInteractMode
        {
            IGNORE,
            DESTROY,
            REFLECT,
            REFLECTANDPOSTPROCESS,
        }
        private static bool SlasherIsPlayerOrFriendly(GameActor slasher)
        {
            if (slasher is PlayerController) return true;
            if (slasher is AIActor)
            {
                if (slasher.GetComponent<CompanionController>()) return true;
                if (!slasher.aiActor.CanTargetPlayers && slasher.aiActor.CanTargetEnemies) return true;
            }
            return false;
        }
        private static bool ProjectileIsValid(Projectile proj, GameActor slashOwner)
        {
            if (proj)
            {
                if (slashOwner == null)
                {
                    return false;
                }
                if (SlasherIsPlayerOrFriendly(slashOwner))
                {
                    if ((proj.Owner && !(proj.Owner is PlayerController)) || proj.ForcePlayerBlankable) return true;
                }
                else if (slashOwner is AIActor)
                {
                    if (proj.Owner && proj.Owner is PlayerController) return true;
                }
                else
                {
                    if (proj.Owner) return true;
                }
            }

            return false;
        }
        private static bool ObjectWasHitBySlash(Vector2 ObjectPosition, Vector2 SlashPosition, float slashAngle, float SlashRange, float SlashDimensions)
        {
            if (Vector2.Distance(ObjectPosition, SlashPosition) < SlashRange)
            {
                float num7 = BraveMathCollege.Atan2Degrees(ObjectPosition - SlashPosition);
                float minRawAngle = Math.Min(SlashDimensions, -SlashDimensions);
                float maxRawAngle = Math.Max(SlashDimensions, -SlashDimensions);
                bool isInRange = false;
                float actualMaxAngle = slashAngle + maxRawAngle;
                float actualMinAngle = slashAngle + minRawAngle;

                if (num7.IsBetweenRange(actualMinAngle, actualMaxAngle)) isInRange = true;
                if (actualMaxAngle > 180)
                {
                    float Overflow = actualMaxAngle - 180;
                    if (num7.IsBetweenRange(-180, (-180 + Overflow))) isInRange = true;
                }
                if (actualMinAngle < -180)
                {
                    float Underflow = actualMinAngle + 180;
                    if (num7.IsBetweenRange((180 + Underflow), 180)) isInRange = true;
                }
                return isInRange;
            }
            return false;
        }
        private static void HandleHeroSwordSlash(List<SpeculativeRigidbody> alreadyHit, Vector2 arcOrigin, float slashAngle, int slashId, GameActor owner, SlashDataForked slashParameters)
        {
            float degreesOfSlash = slashParameters.slashDegrees;
            float slashRange = slashParameters.slashRange;



            ReadOnlyCollection<Projectile> allProjectiles2 = StaticReferenceManager.AllProjectiles;
            for (int j = allProjectiles2.Count - 1; j >= 0; j--)
            {
                Projectile projectile2 = allProjectiles2[j];
                if (ProjectileIsValid(projectile2, owner))
                {
                    Vector2 projectileCenter = projectile2.sprite.WorldCenter;
                    if (ObjectWasHitBySlash(projectileCenter, arcOrigin, slashAngle, slashRange, degreesOfSlash))
                    {
                        if (slashParameters.OnHitBullet != null) slashParameters.OnHitBullet(projectile2);
                        if (slashParameters.projInteractMode != ProjInteractMode.IGNORE || projectile2.collidesWithProjectiles)
                        {
                            if (slashParameters.projInteractMode == ProjInteractMode.DESTROY || slashParameters.projInteractMode == ProjInteractMode.IGNORE) projectile2.DieInAir(false, true, true, true);
                            else if (slashParameters.projInteractMode == ProjInteractMode.REFLECT || slashParameters.projInteractMode == ProjInteractMode.REFLECTANDPOSTPROCESS)
                            {
                                if (projectile2.Owner != null && projectile2.LastReflectedSlashId != slashId)
                                {
                                    projectile2.ReflectBullet(true, owner, 5, (slashParameters.projInteractMode == ProjInteractMode.REFLECTANDPOSTPROCESS), 1, 5, 0, null);
                                    projectile2.LastReflectedSlashId = slashId;
                                }
                            }
                        }
                    }
                }
            }
            DealDamageToEnemiesInArc(owner, arcOrigin, slashAngle, slashRange, slashParameters, alreadyHit);

            if (slashParameters.damagesBreakables)
            {
                List<MinorBreakable> allMinorBreakables = StaticReferenceManager.AllMinorBreakables;
                for (int k = allMinorBreakables.Count - 1; k >= 0; k--)
                {
                    MinorBreakable minorBreakable = allMinorBreakables[k];
                    if (minorBreakable && minorBreakable.specRigidbody)
                    {
                        if (!minorBreakable.IsBroken && minorBreakable.sprite)
                        {
                            if (ObjectWasHitBySlash(minorBreakable.sprite.WorldCenter, arcOrigin, slashAngle, slashRange, degreesOfSlash))
                            {
                                if (slashParameters.OnHitMinorBreakable != null) slashParameters.OnHitMinorBreakable(minorBreakable);
                                minorBreakable.Break();
                            }
                        }
                    }
                }
                List<MajorBreakable> allMajorBreakables = StaticReferenceManager.AllMajorBreakables;
                for (int l = allMajorBreakables.Count - 1; l >= 0; l--)
                {
                    MajorBreakable majorBreakable = allMajorBreakables[l];
                    if (majorBreakable && majorBreakable.specRigidbody)
                    {
                        if (!alreadyHit.Contains(majorBreakable.specRigidbody))
                        {
                            if (!majorBreakable.IsSecretDoor && !majorBreakable.IsDestroyed)
                            {
                                if (ObjectWasHitBySlash(majorBreakable.specRigidbody.UnitCenter, arcOrigin, slashAngle, slashRange, degreesOfSlash))
                                {
                                    float num9 = slashParameters.damage;
                                    if (majorBreakable.healthHaver)
                                    {
                                        num9 *= 0.2f;
                                    }
                                    if (slashParameters.OnHitMajorBreakable != null) slashParameters.OnHitMajorBreakable(majorBreakable);
                                    majorBreakable.ApplyDamage(num9, majorBreakable.specRigidbody.UnitCenter - arcOrigin, false, false, false);
                                    alreadyHit.Add(majorBreakable.specRigidbody);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void DealDamageToEnemiesInArc(GameActor owner, Vector2 arcOrigin, float arcAngle, float arcRadius, SlashDataForked slashParameters, List<SpeculativeRigidbody> alreadyHit = null)
        {
            RoomHandler roomHandler = arcOrigin.GetAbsoluteRoom();
            if (roomHandler == null) return;
            if (SlasherIsPlayerOrFriendly(owner))
            {
                List<AIActor> activeEnemies = roomHandler.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
                if (activeEnemies == null) return;

                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    AIActor aiactor = activeEnemies[i];
                    if (aiactor && aiactor.specRigidbody && aiactor.IsNormalEnemy && !aiactor.IsGone && aiactor.healthHaver)
                    {
                        if (alreadyHit == null || !alreadyHit.Contains(aiactor.specRigidbody))
                        {
                            for (int j = 0; j < aiactor.healthHaver.NumBodyRigidbodies; j++)
                            {
                                SpeculativeRigidbody bodyRigidbody = aiactor.healthHaver.GetBodyRigidbody(j);
                                PixelCollider hitboxPixelCollider = bodyRigidbody.HitboxPixelCollider;
                                if (hitboxPixelCollider != null)
                                {
                                    Vector2 vector = BraveMathCollege.ClosestPointOnRectangle(arcOrigin, hitboxPixelCollider.UnitBottomLeft, hitboxPixelCollider.UnitDimensions);
                                    if (ObjectWasHitBySlash(vector, arcOrigin, arcAngle, arcRadius, 90))
                                    {
                                        bool attackIsNotBlocked = true;
                                        int rayMask = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, CollisionLayer.BulletBreakable);
                                        RaycastResult raycastResult;
                                        if (PhysicsEngine.Instance.Raycast(arcOrigin, vector - arcOrigin, Vector2.Distance(vector, arcOrigin), out raycastResult, true, true, rayMask, null, false, null, null) && raycastResult.SpeculativeRigidbody != bodyRigidbody)
                                        {
                                            attackIsNotBlocked = false;
                                        }
                                        RaycastResult.Pool.Free(ref raycastResult);
                                        if (attackIsNotBlocked)
                                        {
                                            float damage = DealSwordDamageToEnemy(owner, aiactor, arcOrigin, vector, arcAngle, slashParameters);
                                            if (alreadyHit != null)
                                            {
                                                if (alreadyHit.Count == 0) StickyFrictionManager.Instance.RegisterSwordDamageStickyFriction(damage);
                                                alreadyHit.Add(aiactor.specRigidbody);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                List<PlayerController> AllPlayers = new List<PlayerController>();
                if (GameManager.Instance.PrimaryPlayer) AllPlayers.Add(GameManager.Instance.PrimaryPlayer);
                if (GameManager.Instance.SecondaryPlayer) AllPlayers.Add(GameManager.Instance.SecondaryPlayer);
                for (int i = 0; i < AllPlayers.Count; i++)
                {
                    PlayerController player = AllPlayers[i];
                    if (player && player.specRigidbody && player.healthHaver && !player.IsGhost)
                    {
                        if (alreadyHit == null || !alreadyHit.Contains(player.specRigidbody))
                        {
                            SpeculativeRigidbody bodyRigidbody = player.specRigidbody;
                            PixelCollider hitboxPixelCollider = bodyRigidbody.HitboxPixelCollider;
                            if (hitboxPixelCollider != null)
                            {
                                Vector2 vector = BraveMathCollege.ClosestPointOnRectangle(arcOrigin, hitboxPixelCollider.UnitBottomLeft, hitboxPixelCollider.UnitDimensions);
                                if (ObjectWasHitBySlash(vector, arcOrigin, arcAngle, arcRadius, 90))
                                {
                                    bool attackIsNotBlocked = true;
                                    int rayMask = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, CollisionLayer.BulletBreakable);
                                    RaycastResult raycastResult;
                                    if (PhysicsEngine.Instance.Raycast(arcOrigin, vector - arcOrigin, Vector2.Distance(vector, arcOrigin), out raycastResult, true, true, rayMask, null, false, null, null) && raycastResult.SpeculativeRigidbody != bodyRigidbody)
                                    {
                                        attackIsNotBlocked = false;
                                    }
                                    RaycastResult.Pool.Free(ref raycastResult);
                                    if (attackIsNotBlocked)
                                    {
                                        float damage = DealSwordDamageToEnemy(owner, player, arcOrigin, vector, arcAngle, slashParameters);
                                        if (alreadyHit != null)
                                        {
                                            if (alreadyHit.Count == 0) StickyFrictionManager.Instance.RegisterSwordDamageStickyFriction(damage);
                                            alreadyHit.Add(player.specRigidbody);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }
        private static float DealSwordDamageToEnemy(GameActor owner, GameActor targetEnemy, Vector2 arcOrigin, Vector2 contact, float angle, SlashDataForked slashParameters)
        {
            if (targetEnemy.healthHaver)
            {
                float damageToDeal = slashParameters.damage;
                if (targetEnemy.healthHaver && targetEnemy.healthHaver.IsBoss) damageToDeal *= slashParameters.bossDamageMult;
                if ((targetEnemy is AIActor) && (targetEnemy as AIActor).IsBlackPhantom) damageToDeal *= slashParameters.jammedDamageMult;
                DamageCategory category = DamageCategory.Normal;
                if ((owner is AIActor) && (owner as AIActor).IsBlackPhantom) category = DamageCategory.BlackBullet;

                //VFX
                if (slashParameters.doHitVFX && slashParameters.hitVFX != null)
                {
                    bool fatal = false;

                    slashParameters.hitVFX.SpawnAtPosition(new Vector3(contact.x, contact.y), 0, targetEnemy.transform);


                    bool wasAlivePreviously = targetEnemy.healthHaver.IsAlive;

                    if(targetEnemy.healthHaver.currentHealth <= damageToDeal)
                    {
                        if (targetEnemy.healthHaver.CanCurrentlyBeKilled || category == DamageCategory.Unstoppable)
                        {
                            fatal = true;
                        }
                    }

                    if (slashParameters.OnHitTarget != null) slashParameters.OnHitTarget(targetEnemy, fatal);

                    if (slashParameters.OnHitTargetDamageContext != null) slashParameters.OnHitTargetDamageContext(targetEnemy, damageToDeal, fatal);

                    targetEnemy.healthHaver.ApplyDamage(damageToDeal, contact - arcOrigin, owner.ActorName, CoreDamageTypes.None, category, false, null, false);

                }
                if (targetEnemy.knockbackDoer)
                {
                    targetEnemy.knockbackDoer.ApplyKnockback(contact - arcOrigin, slashParameters.enemyKnockbackForce, false);
                }
                if (slashParameters.statusEffects != null && slashParameters.statusEffects.Count > 0)
                {
                    foreach (GameActorEffect effect in slashParameters.statusEffects)
                    {
                        targetEnemy.ApplyEffect(effect);
                    }
                }
            }
            return slashParameters.damage;
        }
    }

}
