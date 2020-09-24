using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{

    public Transform attackPoint;
    public LayerMask enemyLayers;
    public PlayerMana playerMana;

    [SerializeField] const float attackRange = 1.5f;
    [SerializeField] const float attackWidth = 0.5f;

    public Vector2 hitBoxSize = new Vector2(attackRange, attackWidth);
    public int attackDamage = 40;
    public int manaRecov = 20;
    public float attackRate = 2f;
    float nextAttackTime = 0f;

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetButtonDown("Attack"))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void Attack()
    {
        //Play an Attack Animation
        //animator.SetTrigger("Attack");

        //Detect Enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, hitBoxSize, 0f, enemyLayers);
        //Damage Them
        foreach(Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>().TakeDamage(attackDamage);
            playerMana.AddMana(manaRecov);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireCube(attackPoint.position, hitBoxSize);
    }
}
