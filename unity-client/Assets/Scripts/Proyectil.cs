using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Estadísticas de Vuelo")]
    public float velocidad = 10f;
    public float tiempoDeVida = 3f;

    [Header("Datos del Ataque")]
    public AttackData datosAtaque; 

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        rb.linearVelocity = transform.right * velocidad;

        Destroy(gameObject, tiempoDeVida);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player")) return;
        Destroy(gameObject);
    }
}