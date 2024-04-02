using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct CubeInput : IInputComponentData
{
    public int Horizontal;
    public int Vertical;
}

[DisallowMultipleComponent]
public class CubeInputAuthoring : MonoBehaviour
{
    private class CubeInputAuthoringBaker : Baker<CubeInputAuthoring>
    {
        public override void Bake(CubeInputAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<CubeInput>(entity);
        }
    }
}

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct SampleCubeInput : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var left = Input.GetKey("left");
        var right = Input.GetKey("right");
        var up = Input.GetKey("up");
        var down = Input.GetKey("down");

        foreach (var input in SystemAPI.Query<RefRW<CubeInput>>().WithAll<GhostOwnerIsLocal>())
        {
            input.ValueRW = default;

            if (left)
                input.ValueRW.Horizontal -= 1;
            if (right)
                input.ValueRW.Horizontal += 1;
            if (up)
                input.ValueRW.Vertical += 1;
            if (down)
                input.ValueRW.Vertical -= 1;
        }
    }
}
