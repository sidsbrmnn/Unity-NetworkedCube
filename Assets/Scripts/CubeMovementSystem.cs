using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct CubeMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var speed = SystemAPI.Time.DeltaTime * 4;

        foreach (var (input, transform) in SystemAPI.Query<RefRO<CubeInput>, RefRW<LocalTransform>>()
                     .WithAll<Simulate>())
        {
            var movement = new float2(input.ValueRO.Horizontal, input.ValueRO.Vertical);
            movement = math.normalizesafe(movement) * speed;

            transform.ValueRW.Position = transform.ValueRO.Position + new float3(movement.x, 0, movement.y);
        }
    }
}
