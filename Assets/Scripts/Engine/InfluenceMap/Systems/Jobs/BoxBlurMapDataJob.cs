using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

public partial class InfluenceMapSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(InfluenceMapUpdate))]
    struct BoxBlurMapDataJob : IJobForEachWithEntity_EC<InfluenceMap_BlurData>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapData> bufferFromEnt;

        public int WorldSize;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetIndex (int x, int y) => y * WorldSize + x;

        public void Execute (Entity entity, int index, [ReadOnly] ref InfluenceMap_BlurData c0)
        {
            var values = bufferFromEnt[entity].Reinterpret<float>().AsNativeArray();

            var result = new NativeArray<float>(values.Length, Allocator.Temp);

            int WorldSizePower2 = (WorldSize * WorldSize) - 1;

            for (int y = 1; y < WorldSize - 1; y++) {
                for (int x = 1; x < WorldSize - 1; x++) {
                    float sum = 0;
                    // var initial = values[GetIndex(x, y)];

                    sum += values[GetIndex(x - 1, y)];
                    sum += values[GetIndex(x, y)];
                    sum += values[GetIndex(x + 1, y)];

                    // sum += values[GetIndex(x - 1, y - 1)];
                    sum += values[GetIndex(x, y - 1)];
                    // sum += values[GetIndex(x + 1, y - 1)];

                    // sum += values[GetIndex(x - 1, y + 1)];
                    sum += values[GetIndex(x, y + 1)];
                    // sum += values[GetIndex(x + 1, y + 1)];

                    sum /= 5;
                    result[GetIndex(x, y)] = sum;
                }
            }

            values.CopyFrom(result);
        }
    }
}