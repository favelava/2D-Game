using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public HealthBar healthbar;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;

        healthbar.SetMaxHealth(maxHealth);

        healthbar.SetHealth(maxHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        healthbar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        healthbar.SetHealth(currentHealth);

    }

    private void Die()
    {
        Debug.Log("Game Over");
    }
}
