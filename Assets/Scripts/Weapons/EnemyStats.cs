using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    //Health of the enemy
    public float health = 50f;

    void Update()
    {
        //Out of health
        if (health <= 0f)
        {
            Die();
        }
    }

    //What happens when the enemy is shot at
    public void TakeDamage(float damage)
    {
        //Decreasing health
        health -= damage;

        Debug.Log(health);

        //Out of health
        if (health <= 0f)
        {
            Die();
        }
    }

    //Once they have taken enough damage to die
    void Die()
    {
        Destroy(gameObject, 0.5f);
    }

    //For 3D RB add 2D for other rb option
    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(health);

        health -= 10f;
    }
}
