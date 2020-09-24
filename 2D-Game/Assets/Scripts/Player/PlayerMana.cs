using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{

    public int maxMana = 100;
    public int currentMana;

    public ManaBar manaBar;

    // Start is called before the first frame update
    void Start()
    {
        currentMana = maxMana;

        manaBar.SetMaxMana(maxMana);
        manaBar.SetMana(currentMana);
    }

    public void UseMana(int mana)
    {
        currentMana -= mana;

        if (currentMana < 0)
            currentMana = 0;

        manaBar.SetMana(currentMana);
    }

    public void AddMana(int mana)
    {
        currentMana += mana;

        if (currentMana > maxMana)
            currentMana = maxMana;

        manaBar.SetMana(currentMana);
    }
}
