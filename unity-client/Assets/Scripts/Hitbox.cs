using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [HideInInspector] public AttackData ataqueActual; 
    [HideInInspector] public float multiplicadorActual = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && ataqueActual != null)
        {
            float dañoFinal = ataqueActual.daño * multiplicadorActual;
            
            Debug.Log($"{ataqueActual.nombreAtaque} fa {dañoFinal} de dany. (Carga: x{multiplicadorActual})");
        }
    }
}