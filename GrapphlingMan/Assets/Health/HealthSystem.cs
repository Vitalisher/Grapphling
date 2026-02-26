using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable
{
    public float MaxHealth;
    [SerializeField] private float currentHealth;

    public void Die()
    {
        Debug.Log(gameObject.name + "Помер");
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth > 0)
        {
            currentHealth -= damage;
            Debug.Log(gameObject.name + "получил урон " + damage);
        }
        else
        {
            Die();
        }
    }

    void Start()
    {
        currentHealth = MaxHealth;
    }

    void Update()
    {
        
    }
}
