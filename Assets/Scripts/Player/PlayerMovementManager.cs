using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementManager : MonoBehaviour
{
    private PlayerManager playerManager;
    private Vector3 movement;

    private float speed;
    
    private void Start()
    {
        playerManager=GetComponent<PlayerManager>();
        speed = playerManager.playerData.speed;
    }
    
    private void Update()
    {
        HandleMovement();
    }

    public void HandleMovementInput(InputAction.CallbackContext context)
    {
        movement=new Vector3(context.ReadValue<Vector2>().x, 0, context.ReadValue<Vector2>().y).normalized;
    }

    private void HandleMovement()
    {
        transform.Translate(movement * (speed * Time.deltaTime));
    }
}