using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct GoInGameRequest : IRpcCommand
{
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CubeSpawner>();

        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess()
                     .WithNone<NetworkStreamInGame>())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(entity);

            var req = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<GoInGameRequest>(req);
            commandBuffer.AddComponent(req, new SendRpcCommandRequest { TargetConnection = entity });
        }

        commandBuffer.Playback(state.EntityManager);
    }
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    private ComponentLookup<NetworkId> _networkIdLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CubeSpawner>();

        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRequest>()
            .WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(builder));

        _networkIdLookup = state.GetComponentLookup<NetworkId>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var prefab = SystemAPI.GetSingleton<CubeSpawner>().Prefab;
        state.EntityManager.GetName(prefab, out var prefabName);

        var worldName = state.WorldUnmanaged.Name;

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        _networkIdLookup.Update(ref state);

        foreach (var (req, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequest>().WithEntityAccess())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(req.ValueRO.SourceConnection);

            var networkId = _networkIdLookup[req.ValueRO.SourceConnection];
            Debug.Log($"'{worldName}' settings connection '{networkId.Value}' to in game, spawning a Ghost '{prefabName}'");
            var player = commandBuffer.Instantiate(prefab);
            commandBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

            // add player to the linked entity group so it gets destroyed when the connection is destroyed
            commandBuffer.AppendToBuffer(req.ValueRO.SourceConnection, new LinkedEntityGroup { Value = player });

            commandBuffer.DestroyEntity(entity);
        }

        commandBuffer.Playback(state.EntityManager);
    }
}
