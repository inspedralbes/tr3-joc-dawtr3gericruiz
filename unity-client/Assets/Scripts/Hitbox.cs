using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [HideInInspector] public AttackData ataqueActual; 
    [HideInInspector] public float multiplicadorActual = 1f;

    private PlayerController miJugador;

    private void Start()
    {
        miJugador = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController objetivo = other.GetComponent<PlayerController>();

        if (objetivo != null && objetivo != miJugador && ataqueActual != null)
        {
            float dañoFinal = ataqueActual.daño * multiplicadorActual;
            
            Vector2 direccionEmpuje = (objetivo.transform.position - miJugador.transform.position).normalized;
            direccionEmpuje.y += 0.5f; 
            direccionEmpuje = direccionEmpuje.normalized;

            objetivo.RecibirDaño(dañoFinal, direccionEmpuje, ataqueActual.fuerzaEmpujeBase, ataqueActual.escaladoEmpuje);
            
            Debug.Log($"¡{ataqueActual.nombreAtaque} ha connectat! {dañoFinal} de daño.");
        }
    }
}