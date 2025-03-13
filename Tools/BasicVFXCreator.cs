using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace SilverJacket
{
    public class BasicVFXCreator
    {
        public static GameObject MakeBasicVFX(string name, List<string> spritePaths, int fps, IntVector2 dimensions, tk2dBaseSprite.Anchor anchor)
        {
            GameObject obj = new GameObject();
            obj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(obj);
            UnityEngine.Object.DontDestroyOnLoad(obj);

            tk2dSpriteCollectionData tk2DSpriteCollectionData = SpriteBuilder.ConstructCollection(obj, name + "_collection");
            int spriteID = SpriteBuilder.AddSpriteToCollection(spritePaths[0], tk2DSpriteCollectionData);
            tk2dSprite sprite = obj.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(tk2DSpriteCollectionData, spriteID);
            tk2dSpriteDefinition defaultDef = sprite.GetCurrentSpriteDef();
            defaultDef.colliderVertices = new Vector3[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(dimensions.x, dimensions.y),
            };
            tk2dSpriteAnimator animator = obj.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = obj.GetOrAddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = "start", frames = new tk2dSpriteAnimationFrame[0], fps = fps };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spritePaths.Count; i++)
            {
                tk2dSpriteCollectionData collection = tk2DSpriteCollectionData;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(spritePaths[i], collection);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frameDef.ConstructOffsetsFromAnchor(anchor);
                frameDef.colliderVertices = defaultDef.colliderVertices;
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            clip.frames = frames.ToArray();
            clip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            SpriteAnimatorKiller kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
            kill.fadeTime = -1f;
            kill.animator = animator;
            kill.delayDestructionTime = -1f;
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("start");

            return obj;
        }
    }
}
