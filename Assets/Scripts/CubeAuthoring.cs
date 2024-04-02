using Unity.Entities;
using UnityEngine;

public struct Cube : IComponentData
{
}

[DisallowMultipleComponent]
public class CubeAuthoring : MonoBehaviour
{
    private class CubeAuthoringBaker : Baker<CubeAuthoring>
    {
        public override void Bake(CubeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Cube>(entity);
        }
    }
}
