using UnityEngine;

public class BlastZone : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController jugador = other.GetComponent<PlayerController>();

            if (jugador != null && jugador.esJugadorLocal)
            {
                Debug.Log(other.name + " ha sortit de la zona de combat!");
                jugador.PerderVida();
            }
        }
    }
}