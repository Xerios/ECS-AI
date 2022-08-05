using Engine;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

namespace UtilityAI
{
    public interface IAgent
    {
        void Init ();
        void InitSuccess (Entity ent, EntityCommandBuffer cmds);

        Vector3 GetPosition ();

        int GetFaction ();

        Mind GetMind ();

        #if UNITY_EDITOR
        ActionManager GetActionManager ();
        #endif
    }
}