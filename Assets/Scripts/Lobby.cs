using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class Lobby : NetworkBehaviour
{
    public LobbyUi lobbyUi;
    public NetworkedPlayers networkedPlayers;
    private ulong[] selfClientId = new ulong[1];

    void Start()
    {
        if (IsServer)
        {
            ServerPopulateCards();
            networkedPlayers.allNetPlayers.OnListChanged += ServerNetPlayersChanged;
            lobbyUi.ShowStart(true);
            lobbyUi.OnStartClicked += ServerStartClicked;
        }
        else
        {
            ClientPopulateCards();
            networkedPlayers.allNetPlayers.OnListChanged += ClientNetPlayersChanged;
            lobbyUi.ShowStart(false);
            lobbyUi.OnReadyToggled += ClientOnReadyToggled;
            NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnect;
        }

        lobbyUi.OnChangeNameClicked += OnChangeNameClicked;
    }

    private void OnChangeNameClicked(string newValue)
    {
        UpdatePlayerNameServerRpc(newValue);
    }

    private void PopulateMyInfo()
    {
        NetworkPlayerInfo myInfo = networkedPlayers.GetMyPlayerInfo();
        if (myInfo.clientId != ulong.MaxValue)
        {
            lobbyUi.SetPlayerName(myInfo.playerName.ToString());
        }
    }

    private void ServerPopulateCards()
    {
        lobbyUi.playerCards.Clear();
        foreach (NetworkPlayerInfo info in networkedPlayers.allNetPlayers)
        {
            PlayerCard pc = lobbyUi.playerCards.AddCard("Some Player");
            pc.clientId = info.clientId;
            pc.ready = info.ready;
            pc.color = info.color;
            pc.playerName = info.playerName.ToString();
            if (info.clientId == NetworkManager.LocalClientId)
            {
                pc.ShowKick(false);
            }
            else
            {
                pc.ShowKick(true);
            }
            pc.OnKickClicked += ServerOnKickClicked;
            pc.UpdateDisplay();
        }
    }

    private void ServerStartClicked()
    {
        NetworkManager.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void ClientPopulateCards()
    {
        lobbyUi.playerCards.Clear();
        foreach (NetworkPlayerInfo info in networkedPlayers.allNetPlayers)
        {
            PlayerCard pc = lobbyUi.playerCards.AddCard("Some Player");
            pc.clientId = info.clientId;
            pc.ready = info.ready;
            pc.color = info.color;
            pc.playerName = info.playerName.ToString();
            pc.ShowKick(false);
            pc.UpdateDisplay();
        }
    }

    private void ClientOnReadyToggled(bool newValue)
    {
        UpdateReadyServerRpc(newValue);
    }

    private void ServerNetPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent)
    {
        ServerPopulateCards();
        PopulateMyInfo();
        lobbyUi.EnableStart(networkedPlayers.AllPlayersReady());
    }

    private void ServerOnKickClicked(ulong clientId)
    {
        NetworkManager.DisconnectClient(clientId);
    }

    private void ClientNetPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent)
    {
        ClientPopulateCards();
        PopulateMyInfo();
    }

    private void ClientOnClientDisconnect(ulong clientId)
    {
        lobbyUi.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyServerRpc(bool newValue, ServerRpcParams rpcParams = default)
    {
        networkedPlayers.UpdateReady(rpcParams.Receive.SenderClientId, newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerNameServerRpc(string newValue, ServerRpcParams rpcParams = default)
    {
        if (VerifyName(newValue))
        {
            networkedPlayers.UpdatePlayerName(rpcParams.Receive.SenderClientId, newValue);
        }
        else
        {
            ClientRpcParams clientRpcParams = default;
            selfClientId[0] = rpcParams.Receive.SenderClientId;
            clientRpcParams.Send.TargetClientIds = selfClientId;
            ResetNameFieldToPreviousValueClientRpc(clientRpcParams);
        }
    }

    private static bool VerifyName(string name)
    {
        return name.Length <= 15;
    }

    [ClientRpc]
    private void ResetNameFieldToPreviousValueClientRpc(ClientRpcParams clientRpcParams = default)
    {
        lobbyUi.SetPlayerName(networkedPlayers.allNetPlayers[((int)NetworkManager.LocalClientId)].playerName.ToString());
    }
}
