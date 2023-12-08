using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public float movementSpeed = 7f;
    public float jumpSpeed = 15f;
    public bool canJump = false;

    public NetworkVariable<Color> playerColorNetVar = new NetworkVariable<Color>(Color.white);
    private GameObject playerSprite;

    public NetworkVariable<int> scoreNetVar = new NetworkVariable<int>(0);

    public PlayerScoreObject playerScoreObject;

    public override void OnNetworkSpawn()
    {
        NetworkInit();
        base.OnNetworkSpawn();
    }

    void NetworkInit()
    {
        playerSprite = transform.Find("Sprite").gameObject;
        ApplyColor();
        playerColorNetVar.OnValueChanged += OnPlayerColorChanged;
        scoreNetVar.OnValueChanged += ClientOnScoreValueChanged;
    }

    void Update()
    {
        if (IsOwner)
        {
            OwnerHandleInput();
        }
    }

    private void OwnerHandleInput()
    {
        float verticalVelocity = 0;
        if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) && canJump)
        {
            verticalVelocity = jumpSpeed;
        }
        else
        {
            verticalVelocity = GetComponent<Rigidbody2D>().velocity.y;
        }

        float moveVelocity = 0;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            moveVelocity = -movementSpeed;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            moveVelocity = movementSpeed;
        }

        if(verticalVelocity != 0 || moveVelocity != 0)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(moveVelocity, verticalVelocity);
            MoveServerRpc(moveVelocity, verticalVelocity);
        }
    }

    [ServerRpc]
    private void MoveServerRpc(float horizontalVelocity, float verticalVelocity)
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer)
        {
            if (other.gameObject.CompareTag("Collectable"))
            {
                other.GetComponent<BaseCollectable>().ServerPickUp(this);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = false;
        }
    }

    private void ClientOnScoreValueChanged(int old, int current)
    {
        if(playerScoreObject != null) playerScoreObject.ChangeScore(current);
    }

    public void OnPlayerColorChanged(Color previous, Color current)
    {
        ApplyColor();
        if(playerScoreObject != null) playerScoreObject.ChangeTextColor(current);
    }

    private void ApplyColor()
    {
        playerSprite.GetComponent<SpriteRenderer>().color = playerColorNetVar.Value;
    }
}
