using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CollectionGame : NetworkBehaviour
{
    public Player playerPrefab;

    private NetworkedPlayers networkedPlayers;

    private int positionIndex = 0;
    private Vector3[] spawnPositions = new Vector3[]
    {
        new Vector3(-7, -4),
        new Vector3(7, -4),
        new Vector3(8, 3.4f),
        new Vector3(-3, 3)
    };
    
    void Start()
    {
        networkedPlayers = GameObject.Find("NetworkedPlayers").GetComponent<NetworkedPlayers>();
        if (IsServer)
        {
            SpawnPlayers();
        }
    }

    private void SpawnPlayers()
    {
        foreach(NetworkPlayerInfo info in networkedPlayers.allNetPlayers)
        {
            Player playerSpawn = Instantiate(playerPrefab, NextPosition(), Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(info.clientId);
            playerSpawn.playerColorNetVar.Value = info.color;
        }
    }

    private Vector3 NextPosition()
    {
        Vector3 pos = spawnPositions[positionIndex];
        positionIndex += 1;
        if (positionIndex > spawnPositions.Length - 1)
        {
            positionIndex = 0;
        }
        return pos;
    }

}
