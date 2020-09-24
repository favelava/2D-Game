using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float InvincibilityDuration = 1.5f;
    [SerializeField] private float InvincibilityDeltaTime = 0.1f;

    public int maxHealth = 100;
    public int currentHealth;

    public HealthBar healthbar;
    public SpriteRenderer spriteRenderer;
    public LevelLoader levelLoader;

    private bool isInvincible = false;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;

        healthbar.SetMaxHealth(maxHealth);

        healthbar.SetHealth(maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;

        currentHealth -= damage;

        healthbar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(PlayerTempInvincibility());
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

    private IEnumerator PlayerTempInvincibility()
    {
        Debug.Log("Player is Invincible!");
        isInvincible = true;

        for (float i = 0; i < InvincibilityDuration; i += InvincibilityDeltaTime)
        {
            if (spriteRenderer.enabled)
            {
                spriteRenderer.enabled = false;
            } else
            {
                spriteRenderer.enabled = true;
            }

            yield return new WaitForSeconds(InvincibilityDeltaTime);
        }

        spriteRenderer.enabled = true;

        isInvincible = false;
        Debug.Log("Player is no longer Invincible!");
    }

    private void Die()
    {
        levelLoader.LoadDeathScreen();
        Debug.Log("Game Over");
    }
}
