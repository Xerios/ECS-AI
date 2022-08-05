using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

public class EnemyDetector : MonoBehaviour
{
    public SpriteRenderer sprite;
    public Sprite[] eyeFrames;
    public Sprite alarmed, open;
    public Sprite sleep;
    private bool isSleeping;

    public void Start ()
    {
        sprite.enabled = false;
    }

    public void Reset ()
    {
        sprite.enabled = false;
        isSleeping = false;
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0.1f);
    }

    // Start is called before the first frame update
    public void SetSleep ()
    {
        isSleeping = true;
    }

    // Start is called before the first frame update
    public void SetDetectionLevel (float timer = 0, byte state = 0)
    {
        if (timer == 0) {
            if (isSleeping) {
                sprite.enabled = true;
                sprite.sprite = sleep;
                sprite.color = Color.gray;
            }else{
                sprite.enabled = false;
            }
        }else if (timer >= SpatialDetectionTimer.AGGRO_LIMIT) {
            sprite.enabled = true;
            sprite.sprite = state == SpatialDetectionState.AGGRO ? open : alarmed;
            sprite.color = state == SpatialDetectionState.AGGRO ? Color.red : Color.white;
            // sprite.transform.localScale = new Vector3(3f, 3f, 1f);
        }else{
            sprite.enabled = true;
            sprite.sprite = eyeFrames[Mathf.FloorToInt(timer * eyeFrames.Length)];
            sprite.color = Color.Lerp(Color.gray * 0.5f, Color.gray, timer);
            // sprite.transform.localScale = new Vector3(3f, Mathf.Min(3f, 0.2f + timer * 2.8f), 1f);
        }
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0.1f);
    }
}