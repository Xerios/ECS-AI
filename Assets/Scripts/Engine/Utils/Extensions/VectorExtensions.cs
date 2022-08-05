using UnityEngine;

public static class Vector3Extensions
{
    /// <summary>
    /// Returns a Vector2 comprised of the x and y components of the Vector3
    /// </summary>
    public static Vector2 XY (this Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }

    /// <summary>
    /// Returns a Vector2 comprised of the x and z components of the Vector3
    /// </summary>
    public static Vector2 XZ (this Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }

    /// <summary>
    /// Returns a Vector2 comprised of the y and z components of the Vector3
    /// </summary>
    public static Vector2 YZ (this Vector3 vector)
    {
        return new Vector2(vector.y, vector.z);
    }

    public static Vector3 X0Z (this Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }

    public static Vector3 _Y_ (this Vector3 vector)
    {
        return new Vector3(0, vector.y, 0);
    }

    public static float DirectionXZ (this Vector3 vector)
    {
        return Mathf.Atan2(vector.x, vector.z) * Mathf.Rad2Deg;
    }

    public static Quaternion QuaternionY (this Vector3 vector)
    {
        return Quaternion.Euler(0, vector.DirectionXZ(), 0);
    }

    public static bool Approximately (this Vector3 me, Vector3 other, float allowedDifference)
    {
        float num1 = (float)(me.x - other.x);

        if ((double)num1 < -(double)allowedDifference || (double)num1 > (double)allowedDifference) return false;
        float num2 = (float)(me.y - other.y);
        if ((double)num2 < -(double)allowedDifference || (double)num2 > (double)allowedDifference) return false;
        float num3 = (float)(me.z - other.z);
        if ((double)num3 >= -(double)allowedDifference) return (double)num3 <= (double)allowedDifference;
        return false;
    }

    public static Vector3 Around (this Vector3 vector, Vector3 target, float distance)
    {
        return vector + (target - vector).normalized * distance;
    }

    public static bool IsClose (this Vector3 vector, Vector3 target, float distance = 1f)
    {
        return (vector - target).sqrMagnitude < (distance * distance);
    }
}