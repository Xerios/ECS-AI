using UnityEngine;
using UnityEngine.Experimental.Animations;

public struct LookAtJob : IAnimationJob
{
    public TransformStreamHandle joint;
    public TransformSceneHandle target;
    public Vector3 axis;
    public float minAngle;
    public float maxAngle;

    public void ProcessRootMotion (AnimationStream stream)
    {}

    public void ProcessAnimation (AnimationStream stream)
    {
        Solve(stream, joint, target, axis, minAngle, maxAngle);
    }

    private static void Solve (AnimationStream stream, TransformStreamHandle joint, TransformSceneHandle target, Vector3 jointAxis, float minAngle, float maxAngle)
    {
        var targetScale = target.GetLocalScale(stream);

        if (targetScale.x == 0f) return;

        var jointPosition = joint.GetPosition(stream);
        var jointRotation = joint.GetRotation(stream);

        var targetPosition = target.GetPosition(stream);

        var fromDir = jointRotation * jointAxis;
        var toDir = targetPosition - jointPosition;

        var axis = Vector3.Cross(fromDir, toDir).normalized;
        var angle = Vector3.Angle(fromDir, toDir);

        angle = Mathf.Clamp(angle, minAngle, maxAngle);
        var jointToTargetRotation = Quaternion.AngleAxis(angle, axis);

        jointRotation = Quaternion.Lerp(jointRotation, jointToTargetRotation * jointRotation, targetScale.x);

        joint.SetRotation(stream, jointRotation);
    }
}