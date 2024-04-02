using Unity.Entities;
using UnityEngine;

public struct CubeSpawner : IComponentData
{
    public Entity Prefab;
}

[DisallowMultipleComponent]
public class CubeSpawnerAuthoring : MonoBehaviour
{
    public GameObject prefab;

    private class CubeSpawnerAuthoringBaker : Baker<CubeSpawnerAuthoring>
    {
        public override void Bake(CubeSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new CubeSpawner
            {
                Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}
