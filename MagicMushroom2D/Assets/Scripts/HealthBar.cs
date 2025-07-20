using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public float Health;
    public float MaxHealth;
    public float Width;
    public float Height;

    [SerializeField]
    private RectTransform healthBar;


    public void SetMaxHealth(float maxHealth)
    {
        MaxHealth = maxHealth; 
    }

    public void SetHealth(float health)
    {
        Health = health;
        float newWidth = (Health / MaxHealth) * Width;

        healthBar.sizeDelta = new Vector2(newWidth, Height);
    }
}