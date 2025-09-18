using UnityEngine;

[CreateAssetMenu(menuName = "Player")]
public class PlayerData : ScriptableObject
{
    public float speed=5;
    public int maxHealth=100;
    public float damage=5;
    public float range=5;
}
