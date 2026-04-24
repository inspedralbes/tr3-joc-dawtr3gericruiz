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
        if (objetivo != null && objetivo != miJugador && ataqueActual != null && miJugador.esJugadorLocal)
        {
            float dañoFinal = ataqueActual.daño * multiplicadorActual;
            
            Vector2 direccionEmpuje = (objetivo.transform.position - miJugador.transform.position).normalized;
            direccionEmpuje.y += 0.5f; 
            direccionEmpuje = direccionEmpuje.normalized;

            objetivo.AplicarDañoVisual(dañoFinal);

            if (NetworkManager.Instancia != null)
            {
                NetworkManager.Instancia.EnviarGolpe(dañoFinal, direccionEmpuje.x, direccionEmpuje.y, ataqueActual.fuerzaEmpujeBase, ataqueActual.escaladoEmpuje);
            }
            
            Debug.Log($"Impacte registrat! Dany: {dañoFinal}.");
        }
    }
}