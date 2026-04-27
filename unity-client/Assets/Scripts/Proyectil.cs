using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Estadísticas de Vuelo")]
    public float velocidad = 10f;
    public float tiempoDeVida = 3f;

    [Header("Datos del Ataque")]
    public AttackData datosAtaque; 

    [HideInInspector] public PlayerController jugadorQueDispara;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * velocidad;
        Destroy(gameObject, tiempoDeVida);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        PlayerController objetivo = hitInfo.GetComponent<PlayerController>();

        if (objetivo != null)
        {
            if (objetivo == jugadorQueDispara) return;

            if (datosAtaque != null)
            {
                Vector2 direccionEmpuje = (objetivo.transform.position - transform.position).normalized;
                direccionEmpuje.y += 0.5f;
                direccionEmpuje = direccionEmpuje.normalized;

                objetivo.RecibirDaño(datosAtaque.daño, direccionEmpuje, datosAtaque.fuerzaEmpujeBase, datosAtaque.escaladoEmpuje);
                Debug.Log($"¡Proyectil de {datosAtaque.nombreAtaque} ha conectado!");
            }
        }
        Destroy(gameObject);
    }
}