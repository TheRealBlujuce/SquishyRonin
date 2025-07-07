using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeartUI : MonoBehaviour
{
    public GameObject heartPrefab; // A UI Image prefab with no sprite
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public int maxHealth = 5;
    public int currentHealth = 5;

    private List<Image> heartImages = new List<Image>();

    void Start()
    {
        InitializeHearts();
        UpdateHearts();
    }

    void InitializeHearts()
    {
        // Clear existing
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        heartImages.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, transform);
            Image heartImage = heart.GetComponent<Image>();
            heartImages.Add(heartImage);
        }
    }

    public void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHealth)
                heartImages[i].sprite = fullHeart;
            else
                heartImages[i].sprite = emptyHeart;
        }
    }

    // Call this when the player takes damage
    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        UpdateHearts();
    }

    // Call this to heal
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHearts();
    }

    // Call this when gaining a heart container
    public void AddHeartContainer(int amount = 1)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        InitializeHearts();
        UpdateHearts();
    }
}
