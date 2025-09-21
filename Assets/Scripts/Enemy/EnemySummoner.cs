using System.Collections.Generic;
using UnityEngine;

public class EnemySummoner : MonoBehaviour
{
    public static List<Enemy> EnemiesInGame;
    public static List<Transform> EnemiesInGameTransform;
    public static Dictionary<int, GameObject> EnemyPrefabs;
    public static Dictionary<int, Queue<Enemy>> EnemyObjectPools;

    private static bool IsInitilazed;

    public static void Init()
    {
        if (!IsInitilazed)
        {
            EnemiesInGame = new List<Enemy>();
            EnemyPrefabs=new Dictionary<int, GameObject>();
            EnemiesInGameTransform=new List<Transform>();
            EnemyObjectPools = new Dictionary<int, Queue<Enemy>>();

            EnemySummonData[] Enemies = Resources.LoadAll<EnemySummonData>("Enemies");

            foreach (EnemySummonData enemy in Enemies)
            {
                EnemyPrefabs.Add(enemy.EnemyID, enemy.EnemyPrefab);
                EnemyObjectPools.Add(enemy.EnemyID, new Queue<Enemy>());
            }

            IsInitilazed = true;
        }
    }

    public static Enemy SummonEnemy(int EnemyID)
    {
        Enemy SummonedEnemy = null;

        if (EnemyPrefabs.ContainsKey(EnemyID))
        {
            Queue<Enemy> ReferencedQueue = EnemyObjectPools[EnemyID];

            if (ReferencedQueue.Count > 0)
            {
                SummonedEnemy=ReferencedQueue.Dequeue();
                SummonedEnemy.Init();
                SummonedEnemy.gameObject.SetActive(true);
            }
            else
            {
                GameObject NewEnemy=Instantiate(EnemyPrefabs[EnemyID],EnemyManager.checkPointsPositions[0], Quaternion.identity);
                SummonedEnemy = NewEnemy.GetComponent<Enemy>();
                SummonedEnemy.Init();
            }
        }
        else
        {
            Debug.Log($"Enemy with id of {EnemyID} does not exist.");
            return null;
        }
        
        if(!EnemiesInGame.Contains(SummonedEnemy)) EnemiesInGame.Add(SummonedEnemy);
        if(!EnemiesInGameTransform.Contains(SummonedEnemy.transform)) EnemiesInGameTransform.Add(SummonedEnemy.transform);
        
        SummonedEnemy.ID = EnemyID;
        return SummonedEnemy;
    }

    public static void RemoveEnemy(Enemy EnemyToRemove)
    {
        EnemyObjectPools[EnemyToRemove.ID].Enqueue(EnemyToRemove);
        EnemyToRemove.gameObject.SetActive(false);
        EnemiesInGameTransform.Remove(EnemyToRemove.transform);
        EnemiesInGame.Remove(EnemyToRemove);
    }
}