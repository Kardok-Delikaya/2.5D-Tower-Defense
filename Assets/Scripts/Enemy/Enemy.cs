using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int NodeIndex;
    public int MaxHealth=5;
    public int Health=5;
    public float Speed=5;
    public float Damage=1;
    public int ID;
    
    //gold
    
    private int currentTarget = 0;
    private Vector3 targetCheckPoint;

    public void Init()
    {
        Health=MaxHealth;
        transform.position = GameManager.NodePositions[0];
        NodeIndex = 0;
    }
}