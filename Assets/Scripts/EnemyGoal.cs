using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGoal : MonoBehaviour
{
    public bool flagDelivered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // The flag's collider is disabled while carried, so detect via the enemy carrying it instead
        EnemyLogic enemy = other.GetComponent<EnemyLogic>();
        if (enemy != null && enemy.hasFlag)
        {
            flagDelivered = true;
            Debug.Log("Flag delivered to enemy goal!");

            enemy.hasFlag = false;

            var flag = GameObject.Find("Flag").GetComponent<Flag>();
            if (flag != null)
            {
                flag.Respawn();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddEnemyScore();
            }
        }
    }
}
