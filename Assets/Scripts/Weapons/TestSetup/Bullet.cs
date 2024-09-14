using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 100.0f;

    public float maxLifetime = 10.0f;

    //If the bullet hits anything with a RB it will be destroyed
    void OnCollisionEnter(Collision collision)//for 3D RB add 2D for other rb option
    {
        if(collision.gameObject.tag == "Enemy")
        {
            Destroy(gameObject);
        }
    }
}
