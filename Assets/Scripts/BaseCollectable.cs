using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public abstract class BaseCollectable : NetworkBehaviour
{
    public void ServerPickUp(Player thePickerUpper)
    {
        if (IsServer)
        {
            if (ApplyToPlayer(thePickerUpper))
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    protected abstract bool ApplyToPlayer(Player thePickerUpper);
}
