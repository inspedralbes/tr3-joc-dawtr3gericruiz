using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(PlayerController))]
public class FighterAgent : Agent
{
    private PlayerController miControlador;
    private PlayerController enemigo;
    
    // Variables para el cálculo de recompensas
    private float dañoAnteriorMio;
    private float dañoAnteriorEnemigo;
    private int vidasAnterioresMias;
    private int vidasAnterioresEnemigo;

    public override void Initialize()
    {
        miControlador = GetComponent<PlayerController>();
    }

    public void EstablecerEnemigo(PlayerController rival)
    {
        enemigo = rival;
    }

    private void Update()
    {
        // En Update comprobamos constantemente si hemos hecho o recibido daño para dar recompensas
        if (miControlador == null || enemigo == null) return;

        // --- RECOMPENSAS POR DAÑO ---
        if (enemigo.porcentajeDaño > dañoAnteriorEnemigo)
        {
            float diferencia = enemigo.porcentajeDaño - dañoAnteriorEnemigo;
            AddReward(diferencia * 0.01f); // Recompensa positiva por golpear
            dañoAnteriorEnemigo = enemigo.porcentajeDaño;
        }
        else if (enemigo.porcentajeDaño < dañoAnteriorEnemigo) dañoAnteriorEnemigo = enemigo.porcentajeDaño; // Se reseteó su daño

        if (miControlador.porcentajeDaño > dañoAnteriorMio)
        {
            float diferencia = miControlador.porcentajeDaño - dañoAnteriorMio;
            AddReward(-diferencia * 0.01f); // Castigo por recibir golpes
            dañoAnteriorMio = miControlador.porcentajeDaño;
        }
        else if (miControlador.porcentajeDaño < dañoAnteriorMio) dañoAnteriorMio = miControlador.porcentajeDaño;

        // --- RECOMPENSAS POR VIDAS ---
        if (enemigo.vidasActuales < vidasAnterioresEnemigo)
        {
            AddReward(1.0f); // Mató al enemigo
            vidasAnterioresEnemigo = enemigo.vidasActuales;
        }

        if (miControlador.vidasActuales < vidasAnterioresMias)
        {
            AddReward(-1.0f); // Murió
            vidasAnterioresMias = miControlador.vidasActuales;
        }

        // Si alguien se queda a 0 vidas y estamos entrenando, terminamos el episodio para resetear rápido
        if (miControlador.vidasActuales <= 0 || enemigo.vidasActuales <= 0)
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null && gm.modoEntrenamiento)
            {
                EndEpisode();
                OnEpisodeBegin(); // Lo llamamos manualmente por si ML-Agents está inactivo esperando a Python
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        // ML-Agents llamará a esto al empezar a entrenar o cuando llamemos a EndEpisode()
        if (miControlador != null && enemigo != null)
        {
            // Forzamos el reset total para poder hacer miles de partidas seguidas sin pasar por menús
            miControlador.vidasActuales = miControlador.vidasMaximas;
            miControlador.porcentajeDaño = 0f;
            miControlador.transform.position = miControlador.puntoDeRespawn.position;
            miControlador.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            enemigo.vidasActuales = enemigo.vidasMaximas;
            enemigo.porcentajeDaño = 0f;
            enemigo.transform.position = enemigo.puntoDeRespawn.position;
            enemigo.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            dañoAnteriorMio = 0f;
            dañoAnteriorEnemigo = 0f;
            vidasAnterioresMias = miControlador.vidasMaximas;
            vidasAnterioresEnemigo = enemigo.vidasMaximas;
            
            if (miControlador.miInterfaz != null)
            {
                miControlador.miInterfaz.ActualizarVidas(miControlador.vidasActuales);
                miControlador.miInterfaz.ActualizarPorcentaje(0f);
            }
            if (enemigo.miInterfaz != null)
            {
                enemigo.miInterfaz.ActualizarVidas(enemigo.vidasActuales);
                enemigo.miInterfaz.ActualizarPorcentaje(0f);
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Si no hay enemigo (porque el GameManager aún no lo ha asignado), no podemos enviar observaciones útiles
        if (miControlador == null || enemigo == null) return;

        // 1. Nuestra Posición
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.y);

        // 2. Nuestra Velocidad
        Rigidbody2D miRb = GetComponent<Rigidbody2D>();
        sensor.AddObservation(miRb.linearVelocity.x);
        sensor.AddObservation(miRb.linearVelocity.y);

        // 3. Posición del Enemigo
        sensor.AddObservation(enemigo.transform.position.x);
        sensor.AddObservation(enemigo.transform.position.y);

        // 4. Velocidad del Enemigo
        Rigidbody2D rbEnemigo = enemigo.GetComponent<Rigidbody2D>();
        sensor.AddObservation(rbEnemigo.linearVelocity.x);
        sensor.AddObservation(rbEnemigo.linearVelocity.y);

        // 5. Porcentajes de daño
        sensor.AddObservation(miControlador.porcentajeDaño);
        sensor.AddObservation(enemigo.porcentajeDaño);

        // 6. Direcciones hacia las que miramos
        sensor.AddObservation(transform.localScale.x); // +1 derecha, -1 izquierda

        // 7. Estado del suelo
        sensor.AddObservation(miControlador.enSuelo ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (miControlador == null) return;

        // Limpiamos los inputs del frame anterior para que no se quede pulsando cosas permanentemente
        ResetInputs();

        // Recogemos las Acciones Discretas generadas por la IA
        int moveAction = actions.DiscreteActions[0];
        int verticalAction = actions.DiscreteActions[1];
        int attackAction = actions.DiscreteActions[2];

        // Mapeo Movimiento Horizontal
        if (moveAction == 1) miControlador.inputX = -1f;
        else if (moveAction == 2) miControlador.inputX = 1f;

        // Mapeo Salto y Caída
        if (verticalAction == 1) miControlador.inputJumpDown = true;
        if (verticalAction == 2) miControlador.inputCaidaRapida = true;

        // Mapeo Ataques
        if (attackAction == 1) 
        {
            miControlador.inputJabDown = true;
            miControlador.inputJabHeld = true;
        }
        else if (attackAction == 2) miControlador.inputSmashDown = true; // Empieza a cargar
        else if (attackAction == 3) miControlador.inputSmashUp = true;   // Suelta el smash
        else if (attackAction == 4) miControlador.inputProyectilDown = true;
        else if (attackAction == 5) miControlador.inputRecovery = true;
    }

    private void ResetInputs()
    {
        miControlador.inputX = 0f;
        miControlador.inputJumpDown = false;
        miControlador.inputJumpHeld = false;
        miControlador.inputCaidaRapida = false;
        miControlador.inputJabDown = false;
        miControlador.inputJabHeld = false;
        miControlador.inputSmashDown = false;
        miControlador.inputSmashUp = false;
        miControlador.inputProyectilDown = false;
        miControlador.inputRecovery = false;
    }

    // Heuristic mode permite que controles manualmente a tu propio Agente en vez de usar la red neuronal.
    // Esto es muy útil para testear que el Agente y el controlador están bien conectados antes de entrenar.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        
        discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.RightArrow)) discreteActions[0] = 2;

        discreteActions[1] = 0;
        if (Input.GetKeyDown(KeyCode.UpArrow)) discreteActions[1] = 1;
        if (Input.GetKeyDown(KeyCode.DownArrow)) discreteActions[1] = 2;

        discreteActions[2] = 0;
        if (Input.GetKeyDown(KeyCode.J)) discreteActions[2] = 1;
        if (Input.GetKeyDown(KeyCode.K)) discreteActions[2] = 2;
        if (Input.GetKeyUp(KeyCode.K)) discreteActions[2] = 3;
        if (Input.GetKeyDown(KeyCode.L)) discreteActions[2] = 4;
        if (Input.GetKeyDown(KeyCode.I)) discreteActions[2] = 5; 
    }
}
