using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Configuración de la Escena (Arrastrar de la jerarquía)")]
    public PlayerUI[] hudsJugadores;
    public Transform[] puntosDeSpawn;

    [Header("Personajes Disponibles (Arrastrar de la carpeta Prefabs)")]
    public GameObject prefabGojo;
    public GameObject prefabSukuna;

    void Start()
    {
        // ---------------------------------------------------------
        // MODO PRUEBA RÁPIDA:
        // Descomenta la siguiente línea para probar el juego directamente al darle a Play.
        // Asumimos que 0 = Gojo y 1 = Sukuna.
        // ---------------------------------------------------------
        
        IniciarPelea(0, 1); 
    }

    private void ConfigurarJugador(GameObject jugadorInstanciado, int indiceJugador)
    {
        PlayerController controlador = jugadorInstanciado.GetComponent<PlayerController>();

        if (controlador != null)
        {
            if (puntosDeSpawn.Length > indiceJugador)
            {
                controlador.puntoDeRespawn = puntosDeSpawn[indiceJugador];
            }

            if (hudsJugadores.Length > indiceJugador && hudsJugadores[indiceJugador] != null)
            {
                controlador.miInterfaz = hudsJugadores[indiceJugador];
                controlador.miInterfaz.EstablecerRetrato(controlador.miFotoDePerfil);
                controlador.miInterfaz.ActualizarVidas(controlador.vidasMaximas);
                controlador.miInterfaz.ActualizarPorcentaje(0f);
            }
            else
            {
                Debug.LogError($"Falta assignar el HUD pel {indiceJugador + 1} en el GameManager.");
            }
        }
    }

    public void IniciarPelea(int eleccionP1, int eleccionP2)
{
    GameObject prefabP1 = ObtenerPrefabPorID(eleccionP1);
    if (prefabP1 != null && puntosDeSpawn.Length > 0)
    {
        GameObject jugador1 = Instantiate(prefabP1, puntosDeSpawn[0].position, Quaternion.identity);
        ConfigurarJugador(jugador1, 0);
        jugador1.GetComponent<PlayerController>().IniciarSecuenciaIntro(3f);
    }

    GameObject prefabP2 = ObtenerPrefabPorID(eleccionP2);
    if (prefabP2 != null && puntosDeSpawn.Length > 1)
    {
        GameObject jugador2 = Instantiate(prefabP2, puntosDeSpawn[1].position, Quaternion.identity);
        ConfigurarJugador(jugador2, 1);
        jugador2.transform.localScale = new Vector3(-1, 1, 1);
        jugador2.GetComponent<PlayerController>().IniciarSecuenciaIntro(3f);
    }
}

    private GameObject ObtenerPrefabPorID(int idPersonaje)
    {
        switch (idPersonaje)
        {
            case 0: return prefabGojo;
            case 1: return prefabSukuna;
            default: 
                Debug.LogWarning("ID de personatge no vàlida. Per defecte s'assigna Gojo.");
                return prefabGojo;
        }
    }
}