using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public partial class InfluenceMapSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(InfluenceMapData), typeof(InfluenceMapToAddData))]
    struct ApplyBufferedInfluenceJob : IJobForEachWithEntity_EC<InfluenceMapUpdate>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapData> bufferFromEnt;
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapToAddData> addvalues;

        public int WorldSize;
        public int WorldSizePowerOf2;

        public void Execute (Entity entity, int index, [ReadOnly] ref InfluenceMapUpdate _)
        {
            var buffer = addvalues[entity];
            var values = bufferFromEnt[entity];

            for (int i = 0; i < buffer.Length; i++) {
                byte size = buffer[i].size;
                float rSquared = size * size;

                var pos = buffer[i].pos;
                var minX = math.max(0, pos.x - size);
                var minY = math.max(0, pos.y - size);
                var maxX = math.min(WorldSize, pos.x + size);
                var maxY = math.min(WorldSize, pos.y + size);

                for (int y = minY; y < maxY; y++) {
                    for (int x = minX; x < maxX; x++) {
                        if (math.distancesq(pos, new float2(x, y)) < rSquared) {
                            var trueIndex = y * WorldSize + x;
                            values[trueIndex] += buffer[i].weight;
                        }
                    }
                }
            }
        }
    }
}