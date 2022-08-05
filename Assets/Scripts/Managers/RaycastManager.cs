using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RaycastManager")]
public class RaycastManager : SingletonScritableObject<RaycastManager>
{
    [Header("Raycast settings")]
    public int raycastDistance = 500;

    [Header("Layers")]
    public LayerMask layerMaskGround, layerMaskUnits, layerMaskBuildings, layerMaskResource;

    private RaycastHit[] hitResults = new RaycastHit[8];
    private Collider[] hitColliders = new Collider[8];
    private Plane plane = new Plane(Vector3.up, 0);

    public bool RaycastPreciseLayer (Vector2 pos, LayerMask layer, out GameObject hit)
    {
        var ray = Camera.main.ScreenPointToRay(pos);

        int count = Physics.RaycastNonAlloc(ray, hitResults, raycastDistance, layer, QueryTriggerInteraction.Ignore);

        if (count != 0) {
            // Iterate over results and find the closest one to the camera
            Collider closestCollider = null;
            float closest = Mathf.Infinity;
            for (int i = 0; i < count; i++) {
                var dist = hitResults[i].distance;
                if (dist < closest) {
                    closestCollider = hitResults[i].collider;
                    closest = dist;
                }
            }
            hit = closestCollider.transform.root.gameObject;
            return true;
        }

        hit = null;
        return false;
    }

    public bool RaycastProximityLayer (Vector2 pos, LayerMask layer, out GameObject hit)
    {
        var ray = Camera.main.ScreenPointToRay(pos);

        int count = Physics.SphereCastNonAlloc(ray, 0.5f, hitResults, raycastDistance, layer, QueryTriggerInteraction.Ignore);

        // If we can't get any result with direct raycast, try fatter raycast
        if (count == 0) {
            RaycastGround(pos, out Vector3 hitpos);
            count = Physics.OverlapSphereNonAlloc(hitpos, 3f, hitColliders, layer, QueryTriggerInteraction.Ignore);

            // Iterate over results and find the closest one to the camera
            if (count != 0) {
                Collider closestCollider = null;
                float closest = Mathf.Infinity;
                for (int i = 0; i < count; i++) {
                    var dist = Vector3.SqrMagnitude(hitpos - hitColliders[i].transform.position);
                    if (dist < closest) {
                        closestCollider = hitColliders[i];
                        closest = dist;
                    }
                }
                hit = closestCollider.transform.root.gameObject;
                return true;
            }
        }else{
            // Iterate over results and find the closest one to the camera
            Collider closestCollider = null;
            float closest = Mathf.Infinity;
            for (int i = 0; i < count; i++) {
                var dist = hitResults[i].distance;
                if (dist < closest) {
                    closestCollider = hitResults[i].collider;
                    closest = dist;
                }
            }
            hit = closestCollider.transform.root.gameObject;
            return true;
        }

        hit = null;
        return false;
    }

    public bool RaycastProximityUnit (Vector2 pos, out GameObject hit) => RaycastProximityLayer(pos, layerMaskUnits, out hit);


    public bool RaycastPlane (Vector2 pos, float height, out Vector3 hitpos)
    {
        var ray = Camera.main.ScreenPointToRay(pos);

        // Raycast against an infinite plane, in case no colliders are present
        float dist;

        if (new Plane(Vector3.down, height).Raycast(ray, out dist)) {
            hitpos = ray.GetPoint(dist);
            return true;
        }

        hitpos = Vector3.zero;
        return false;
    }

    public bool RaycastGround (Vector2 pos, out Vector3 hitpos)
    {
        var ray = Camera.main.ScreenPointToRay(pos);

        // Raycast against ground layer
        if (Physics.RaycastNonAlloc(ray, hitResults, raycastDistance, layerMaskGround) != 0) {
            hitpos = hitResults[0].point;
            return true;
        }

        // Raycast against an infinite plane, in case no colliders are present
        float dist;
        if (plane.Raycast(ray, out dist)) {
            hitpos = ray.GetPoint(dist);
            return true;
        }

        hitpos = Vector3.zero;
        return false;
    }
}