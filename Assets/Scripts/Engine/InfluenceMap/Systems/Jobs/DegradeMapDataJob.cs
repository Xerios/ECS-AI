using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public partial class InfluenceMapSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(InfluenceMapUpdate))]
    struct DegradeMapDataJob : IJobForEachWithEntity_EC<InfluenceMap_DegradeData>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapData> bufferFromEnt;

        public void Execute (Entity entity, int index, [ReadOnly] ref InfluenceMap_DegradeData c0)
        {
            var b0 = bufferFromEnt[entity];

            for (int i = 0; i < b0.Length; i++) {
                b0[i] *= c0.Value;
            }
        }
    }
}