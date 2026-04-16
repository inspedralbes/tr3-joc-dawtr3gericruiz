using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private PlayerController p1Controller;
    private PlayerController p2Controller;
    private bool partidaTerminada = false;

    [Header("UI de Partida")]
    public MatchUI uiPartida;

    [Header("Configuración de la Escena (Arrastrar de la jerarquía)")]
    public PlayerUI[] hudsJugadores;
    public Transform[] puntosDeSpawn;

    [Header("Personajes Disponibles (Arrastrar de la carpeta Prefabs)")]
    public GameObject prefabGojo;
    public GameObject prefabSukuna;

    void Start()
    {
        if (MatchManager.Instance != null && MatchManager.Instance.mapaElegido != null)
        {
            string nombreEscenaMapa = MatchManager.Instance.mapaElegido.sceneName;
            
            if (!string.IsNullOrEmpty(nombreEscenaMapa) && !SceneManager.GetSceneByName(nombreEscenaMapa).isLoaded)
            {
                SceneManager.LoadScene(nombreEscenaMapa, LoadSceneMode.Additive);
            }
        }
        // ----------------------------------------

        // ---------------------------------------------------------
        // MODO PRUEBA RÁPIDA:
        // Descomenta la siguiente línea para probar el juego directamente al darle a Play.
        // 0 = Gojo y 1 = Sukuna.
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
            
            p1Controller = jugador1.GetComponent<PlayerController>(); 
        }

        GameObject prefabP2 = ObtenerPrefabPorID(eleccionP2);
        if (prefabP2 != null && puntosDeSpawn.Length > 1)
        {
            GameObject jugador2 = Instantiate(prefabP2, puntosDeSpawn[1].position, Quaternion.identity);
            ConfigurarJugador(jugador2, 1);
            jugador2.transform.localScale = new Vector3(-1, 1, 1);
            jugador2.GetComponent<PlayerController>().IniciarSecuenciaIntro(3f);
            
            p2Controller = jugador2.GetComponent<PlayerController>();
        }
        StartCoroutine(RutinaCuentaAtras());
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

    private System.Collections.IEnumerator RutinaCuentaAtras()
    {
        if (uiPartida == null)
        {
            Debug.LogWarning("Falta assignar el MatchUI en el GameManager.");
            yield break;
        }

        uiPartida.ActualizarTexto("3");
        yield return new WaitForSeconds(1f);

        uiPartida.ActualizarTexto("2");
        yield return new WaitForSeconds(1f);

        uiPartida.ActualizarTexto("1");
        yield return new WaitForSeconds(1f);

        uiPartida.ActualizarTexto("JA!");
        yield return new WaitForSeconds(1f);

        uiPartida.ActualizarTexto("");
    }
    
    public void ComprobarVictoria()
    {
        if (partidaTerminada) return;

        if (p1Controller != null && p1Controller.vidasActuales <= 0)
        {
            TerminarPartida(2); 
        }
        else if (p2Controller != null && p2Controller.vidasActuales <= 0)
        {
            TerminarPartida(1); 
        }
    }

    private void TerminarPartida(int ganador)
    {
        partidaTerminada = true;
        
        if (uiPartida != null)
        {
            uiPartida.MostrarPantallaVictoria(ganador);
        }

        Time.timeScale = 0f;
    }
}