using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatManager : MonoBehaviour
{
    private bool isInvincible;
    private SpriteRenderer spriteRenderer;
    private PlayerManager playerManager;
    public int health { get; private set; }
    
    public LayerMask enemyLayer;

    public Weapon currentWeapon;
    
    [SerializeField] private Weapon[] weaponsOnPlayer;
    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        spriteRenderer=GetComponent<SpriteRenderer>();
        health = playerManager.playerData.maxHealth;
    }

    public void TakeDamage(float damageToTake)
    {
        health -= (int)damageToTake;

        if (health > 0)
        {
            StartCoroutine(IFrame());
        }
        else
        {
            health = 0;
            //Play Death Animation
            //Game Over Pop Up
        }
        
        //Update Health Slider
    }
    
    private IEnumerator IFrame()
    {
        isInvincible = true;
        
        spriteRenderer.enabled=false;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.enabled=true;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.enabled=false;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.enabled=true;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.enabled=false;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.enabled=true;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.enabled=false;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.enabled=true;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.enabled=false;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.enabled=true;
        
        isInvincible = false;
    }
    
    public void HandleFirstWeaponChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!weaponsOnPlayer[0].gameObject.activeSelf)
            {
                weaponsOnPlayer[0].gameObject.SetActive(true);
                weaponsOnPlayer[1].gameObject.SetActive(false);
                currentWeapon=weaponsOnPlayer[0];
            }
        }
    }

    public void HandleSecondWeaponChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!weaponsOnPlayer[1].gameObject.activeSelf)
            {
                weaponsOnPlayer[1].gameObject.SetActive(true);
                weaponsOnPlayer[0].gameObject.SetActive(false);
                currentWeapon=weaponsOnPlayer[1];
            }
        }
    }
}