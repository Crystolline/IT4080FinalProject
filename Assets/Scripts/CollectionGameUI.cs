using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CollectionGameUI : MonoBehaviour
{
    private NetworkedPlayers networkedPlayers;
    public PlayerScoreObject playerScoreObjectPrefab;

    private int scorePositionIndex = 0;
    private Vector3[] scoreObjectPositions = new Vector3[]{
        new Vector3(-300, 190),
        new Vector3(-100, 190),
        new Vector3(100, 190),
        new Vector3(300, 190)
    };

    void Start()
    {
        networkedPlayers = GameObject.Find("NetworkedPlayers").GetComponent<NetworkedPlayers>();
        CreateScoreObjects();
    }

    private void CreateScoreObjects()
    {
        foreach (NetworkPlayerInfo info in networkedPlayers.allNetPlayers)
        {
            PlayerScoreObject playerScoreObject = Instantiate(playerScoreObjectPrefab, NextScorePosition(), Quaternion.identity);
            playerScoreObject.transform.SetParent(GameObject.Find("Canvas").transform, false);
            playerScoreObject.ChangeTextColor(info.color);
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in players)
            {
                if(player.GetComponent<Player>().playerColorNetVar.Value == info.color)
                {
                    player.GetComponent<Player>().playerScoreObject = playerScoreObject;
                }
            }
        }
    }

    private Vector3 NextScorePosition()
    {
        Vector3 pos = scoreObjectPositions[scorePositionIndex];
        scorePositionIndex += 1;
        if (scorePositionIndex > scoreObjectPositions.Length - 1)
        {
            scorePositionIndex = 0;
        }
        return pos;
    }
}
