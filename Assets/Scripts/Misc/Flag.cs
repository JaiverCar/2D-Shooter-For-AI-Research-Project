using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Flag : MonoBehaviour
{
    private PlayerLogic pHolder;
    private EnemyLogic eHolder;

    [SerializeField]
    public bool isEnemyFlag;

    private static readonly Vector3 spawnPosition = new Vector3(-21f, 21f, 0f);

    public void Respawn()
    {
        pHolder = null;
        eHolder = null;
        transform.position = spawnPosition;
        GetComponent<CircleCollider2D>().enabled = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerLogic.OnPlayerDied += PlayerDropFlag;
        EnemyLogic.OnEnemyDied += EnemyDropFlag;
    }

    private void EnemyDropFlag(EnemyLogic enemy)
    {
        // if the enemy holding the flag dies
        if (eHolder == enemy)
        {
            // set our position to the enemy's last position
            transform.position = eHolder.transform.position;

            enemy.hasFlag = false;

            eHolder = null;

            // enable our collider component
            GetComponent<CircleCollider2D>().enabled = true;
        }
    }

    private void PlayerDropFlag()
    {
        // if the player died and was holding the flag
        if (pHolder != null)
        {
            // set our position to the player's last position
            transform.position = pHolder.transform.position;

            pHolder = null;

            // enable our collider component
            GetComponent<CircleCollider2D>().enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //GameManager.Instance.ourFlag = this.transform;

        // if the player is holding us
        if (pHolder != null)
        {
            // follow them around at a slight offset
            transform.position = new Vector3(pHolder.transform.position.x + 0.5f, pHolder.transform.position.y + 0.5f);

            if (Input.GetKey(KeyCode.F))
            {
                // set our position to the player's last position
                transform.position = pHolder.transform.position;

                pHolder = null;

                // enable our collider component
                GetComponent<CircleCollider2D>().enabled = true;
            }
        }

        // if an enemy is holding us
        if (eHolder != null)
        {
            // follow them around at a slight offset
            transform.position = new Vector3(eHolder.transform.position.x + 0.5f, eHolder.transform.position.y + 0.5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Check for collision against the player
        var player = collision.GetComponent<PlayerLogic>();

        // if we collided with the player
        if (player != null)
        {
            // save a ref the player as our holder
            pHolder = player;

            // disable our collider component
            GetComponent<CircleCollider2D>().enabled = false;
            
            return;
        }

        //Check for collision against an enemy
        var enemy = collision.GetComponent<EnemyLogic>();

        // if we collided with an enemy
        if (enemy != null && enemy.thisBrain.squad == UtilityAI.HiveMind.squads.s_FlagAttackers)
        {
            // save a ref the enemy as our holder
            eHolder = enemy;

            enemy.hasFlag = true;
            
            // disable our collider component
            GetComponent<CircleCollider2D>().enabled = false;
            
            return;
        }
    }
}
