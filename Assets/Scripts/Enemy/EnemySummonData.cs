using UnityEngine;

[CreateAssetMenu(menuName = "EnemySummonData")]
public class EnemySummonData : ScriptableObject
{
    public GameObject EnemyPrefab;
    public int EnemyID;
}