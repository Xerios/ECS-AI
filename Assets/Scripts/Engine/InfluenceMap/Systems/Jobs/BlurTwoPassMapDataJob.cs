#pragma warning disable 0649
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

public partial class InfluenceMapSystem
{
    [BurstCompile]
    struct BlurHorizontalMapData : IJob
    {
        public NativeArray<float> resultValues;

        public NativeArray<float> values;

        public int WorldSize;
        public int WorldSizePowerOf2;

        public void Execute ()
        {
            for (int x = 1; x < WorldSizePowerOf2 - 1; x++) {
                float sum = 0;

                sum += values[x - 1];
                sum += values[x];
                sum += values[x + 1];

                sum /= 3;
                resultValues[x] = sum;
            }
        }
    }

    [BurstCompile]
    struct BlurVerticalMapData : IJob
    {
        [DeallocateOnJobCompletion] public NativeArray<float> resultValues;

        public NativeArray<float> values;

        public int WorldSize;
        public int WorldSizePowerOf2;

        public void Execute ()
        {
            for (int x = WorldSize; x < WorldSizePowerOf2 - 1 - WorldSize; x++) {
                float sum = 0;

                sum += resultValues[x - WorldSize];
                sum += resultValues[x];
                sum += resultValues[x + WorldSize];

                sum /= 3;
                resultValues[x] = sum;
            }

            resultValues.CopyTo(values);
        }
    }
}