using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    //Health of the enemy
    public float health = 50f;

    //What happens when the enemy is shot at
    public void TakeDamage(float damage)
    {
        //Decreasing health
        health -= damage; 

        //Out of health
        if(health <= 0f)
        {
            Die();
        }
    }

    //Once they have taken enough damage to die
    void Die()
    {
        Destroy(gameObject);
    }
}
