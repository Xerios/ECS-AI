using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public partial class InfluenceMapSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(InfluenceMapUpdate))]
    struct NormalizeMapDataJob : IJobForEachWithEntity_EC<InfluenceMap_NormalizeData>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapData> bufferFromEnt;

        public void Execute (Entity entity, int index, [ReadOnly] ref InfluenceMap_NormalizeData c0)
        {
            var b0 = bufferFromEnt[entity];

            for (int i = 0; i < b0.Length; i++) {
                b0[i] = math.saturate(b0[i].Value);
            }
        }
    }
}