using UnityEngine;

public class BlastZone : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.name + " ha sigut eliminat!");

            PlayerController jugador = other.GetComponent<PlayerController>();

            if (jugador != null)
            {
                jugador.PerderVida();
            }
        }
    }
}