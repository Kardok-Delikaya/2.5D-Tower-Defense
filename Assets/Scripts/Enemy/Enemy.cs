using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int CheckPointIndex;
    public int MaxHealth=5;
    public int Health=5;
    public float Speed=5;
    public float Damage=1;
    public int ID;

    public void Init()
    {
        Health=MaxHealth;
        transform.position = EnemyManager.checkPointsPositions[0];
        CheckPointIndex = 0;
    }
}