using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;

namespace UtilityAI
{
    public struct Context
    {
        public Entity SelfMind;
        public StateMachine machine;
        public BlackboardDictionary data;

        public DecisionHistory decisionHistory;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetComponent<T> (Entity entity) where T : struct, IComponentData => AIManager.Instance.mgr.GetComponentData<T>(entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T> (Entity entity, T data) where T : struct, IComponentData => AIManager.Instance.mgr.SetComponentData<T>(entity, data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity GetTarget () => decisionHistory.GetSignalId();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetStateTrackHash () => math.hash(new int2(machine.Current.CurrentState.meta.Id.GetHashCode(), machine.Current.CurrentState.CurrentTrackId));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Repeat () => machine.Current.Repeat();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Suspend () => machine.Current.CurrentState.Suspend();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait (float time = 0) => machine.Current.CurrentState.Wait(time);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop () => machine.Current.CurrentState.Stop();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset () => machine.Current.Begin();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Go (string name) => machine.Current.Go(name);
    }
}