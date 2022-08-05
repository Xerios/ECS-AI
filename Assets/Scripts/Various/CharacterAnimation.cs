using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;
using UnityEngine.U2D;

public class CharacterAnimation : MonoBehaviour
{
    [Header("Setup")]
    public SpriteRenderer sprite;

    public AnimationFrames[] animations;

    // -----------------------
    [NonSerialized]
    public Vector3 Velocity;

    // -----------------------
    [Header("Bobbly movement")]
    public float swing = 20f;
    public float rotConst = 1.5f, posConst = 0.05f;
    private float randomTimeOffset;
    private Transform model;
    private new Transform camera;

    // -----------------------
    [NonSerialized]
    // public Vector3 HitVelocity;
    // private Vector3 hitVelocitySmoothed;
    private bool strobing;
    private Color originalColor = Color.black;

    private bool animating, animationStay, animationFaceDirection;
    private float animationFrame;
    private string animationName;
    private AnimationFrames animationSprites;
    private float animationSpeed;

    [Serializable]
    public struct AnimationFrames
    {
        public string name;
        public Sprite[] sprites;
    }

    void Awake ()
    {
        randomTimeOffset = UnityEngine.Random.value;

        model = this.transform;
        camera = Camera.main.transform;
    }

    void OnEnable ()
    {
        if (originalColor == Color.black) originalColor = sprite.color;

        sprite.color = originalColor;
    }

    public void Hit (Vector3 velocity, bool knockdown)
    {
        // HitVelocity = velocity;

        if (velocity.sqrMagnitude > 0) sprite.flipX = Vector3.Dot(velocity.normalized, camera.right) > 0;

        StrobeColor(sprite, 1, Color.red);
        if (knockdown) {
            Play("KnockedDown", stay: true, speed: 15f);
        }else if (!animating || !animationName.StartsWith("KnockDown")) {
            Play("Hurt");
        }
    }


    public void Die (Vector3 velocity)
    {
        Velocity = Vector3.zero;

        if (velocity.sqrMagnitude > 0) sprite.flipX = Vector3.Dot(velocity.normalized, camera.right) > 0;

        sprite.color = originalColor;
        model.localPosition = Vector3.zero;
        model.localRotation = Quaternion.identity;

        StopAllCoroutines();
        Play("Die", stay: true);
    }

    public void Play (string name, bool faceDir = false, bool stay = false, float speed = 10f)
    {
        if (!Has(name)) return;

        animating = true;
        animationFaceDirection = faceDir;
        animationStay = stay;
        animationFrame = 0f;
        animationName = name;
        animationSpeed = speed;
        animationSprites = animations.First(x => x.name == name);
    }

    public bool Has (string name)
    {
        return animations.Any(x => x.name == name);
    }

    void Update ()
    {
        var time = Time.time + randomTimeOffset;

        var vectorFlat = Velocity.X0Z();
        float speed = vectorFlat.magnitude;

        // ------------------
        var _rotation = Quaternion.LookRotation((model.position - camera.position).X0Z().normalized, Vector3.up).eulerAngles.y;

        model.localRotation = Quaternion.Euler(0, _rotation, 0);

        // ------------------
        // hitVelocitySmoothed = Vector3.Lerp(hitVelocitySmoothed, HitVelocity, 40f * Time.deltaTime);
        // HitVelocity = Vector3.Lerp(HitVelocity, Vector3.zero, 10f * Time.deltaTime);

        // var hitRotate = Quaternion.identity;
        // var hitRotate = Quaternion.Euler(0, hitVelocitySmoothed.DirectionXZ(),
        //     0) * Quaternion.Euler(hitVelocitySmoothed.magnitude * 45, 0, 0) * Quaternion.Inverse(Quaternion.Euler(0, hitVelocitySmoothed.DirectionXZ(), 0));

        // ------------------
        // if (speed > 0.01f) {
        //     float speedClamped = Mathf.Min(speed, 10f);
        //     Vector3 vel = vectorFlat / speed;

        //     float animTime = randomTimeOffset + time;
        //     var jumpUp = Mathf.Abs(Mathf.Cos(animTime * swing)) * speedClamped * posConst;
        //     var swingSides = Mathf.Sin(animTime * swing) * speedClamped * rotConst;

        //     var localModelPos = new Vector3(0, jumpUp, 0);
        //     var localModelRot = Quaternion.Euler(0, _rotation, swingSides);

        //     model.localPosition = localModelPos;
        //     model.localRotation = hitRotate * localModelRot;
        // }else{
        //     // _rotation = Mathf.LerpAngle(_rotation, lookAtYaw, Time.smoothDeltaTime * 5f);
        // var localModelRot = Quaternion.Euler(0, _rotation, 0);

        //     model.localPosition = Vector3.Lerp(model.localPosition, Vector3.zero, Time.smoothDeltaTime * 5f);
        //     model.localRotation = hitRotate * localModelRot;
        // }

        if (!animating) {
            if (speed >= 10f) {
                var idleAnims = animations.First(x => x.name == "Run");
                sprite.sprite = idleAnims.sprites[(int)((time * 10f) % idleAnims.sprites.Length)];
            }else if (speed >= 3f) {
                var idleAnims = animations.First(x => x.name == "Walk");
                sprite.sprite = idleAnims.sprites[(int)((time * 10f) % idleAnims.sprites.Length)];
            }else{
                var idleAnims = animations.First(x => x.name == "Idle");
                sprite.sprite = idleAnims.sprites[(int)((time * 2f) % idleAnims.sprites.Length)];
            }
            animationFaceDirection = true;
        }else{
            animationFrame += Time.deltaTime * animationSpeed;
            if (Mathf.CeilToInt(animationFrame) < animationSprites.sprites.Length) {
                sprite.sprite = animationSprites.sprites[Mathf.CeilToInt(animationFrame)];
            }else{
                if (!animationStay) animating = false;
            }
        }

        if (animationFaceDirection) {
            if (speed > 1f) sprite.flipX = Vector3.Dot(vectorFlat.normalized, camera.right) <= 0;
        }
    }

    void OnDestroy ()
    {}

    // ###########################################################################################################################

    void StrobeColor (SpriteRenderer mySprite, int _strobeCount, Color _toStrobe)
    {
        if (strobing || !this.isActiveAndEnabled) return;

        strobing = true;

        originalColor = mySprite.color;

        StartCoroutine(StrobeColorHelper(0, ((_strobeCount * 2) - 1), mySprite, _toStrobe));
    }

    public void StrobeAlpha (SpriteRenderer mySprite, int _strobeCount, float a) => StrobeColor(mySprite, _strobeCount, new Color(mySprite.color.r, mySprite.color.b, mySprite.color.g, a));

    private IEnumerator StrobeColorHelper (int _i, int _stopAt, SpriteRenderer _mySprite, Color _toStrobe)
    {
        if (_i <= _stopAt) {
            _mySprite.color = (_i % 2 == 0) ? _toStrobe : originalColor;

            yield return new WaitForSeconds(.2f);
            StartCoroutine(StrobeColorHelper((_i + 1), _stopAt, _mySprite, _toStrobe));
        }else{
            strobing = false;
        }
    }
}