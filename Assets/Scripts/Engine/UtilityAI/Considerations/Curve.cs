using System;
using Unity.Mathematics;

[Serializable]
public struct Curve
{
    public float range, offset;

    public float value0;
    public float value1;
    public float tangent0;
    public float tangent1;

    public Curve(float range, float offset, float value0, float value1, float tangent0, float tangent1)
    {
        this.range = range;
        this.offset = offset;
        this.value0 = value0;
        this.value1 = value1;
        this.tangent0 = tangent0;
        this.tangent1 = tangent1;
    }

    public float Evaluate (float t)
    {
        float t0 = offset + t;

        float m0 = tangent0 * range;
        float m1 = tangent1 * range;

        float t2 = t0 * t0;
        float t3 = t2 * t0;

        float a0 = 2f * t3 - 3f * t2 + 1f;
        float b0 = t3 - 2f * t2 + t0;
        float c0 = t3 - t2;
        float d0 = -2f * t3 + 3f * t2;

        return (a0 * value0 + b0 * m0 + c0 * m1 + d0 * value1);
    }
}