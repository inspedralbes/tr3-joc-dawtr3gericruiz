using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Red / Multijugador")]
    public bool esJugadorLocal = false; 

    [Header("Estado Inicial")]
    public bool bloqueadoPorIntro = false;

    [Header("Interpolación Red")]
    private Vector3 redPosicionObjetivo;
    private bool recibioPrimeraPosicion = false;
    private float movSendTimer = 0f;
    private const float MOV_SEND_INTERVAL = 0.033f;
    private float redVelY;
    private bool redEnSuelo;

    [Header("Estado del Jugador (Vidas y UI)")]
    public PlayerUI miInterfaz;
    public int vidasMaximas = 3;
    public int vidasActuales;
    public Transform puntoDeRespawn;
    public Sprite miFotoDePerfil;

    [Header("Movimiento y Salto")]
    public float velocidadCorrer = 16f;
    public float fuerzaSalto = 22f;
    public int saltosMaximos = 2;
    private int saltosRestantes;
    
    [Header("Input State")]
    public float inputX;
    public bool inputJumpDown;
    public bool inputJumpHeld;
    public bool inputCaidaRapida;
    public bool inputJabDown;
    public bool inputJabHeld;
    public bool inputSmashDown;
    public bool inputSmashUp;
    public bool inputProyectilDown;
    public bool inputRecovery;

    [Header("Caída Rápida")]
    public float multiplicadorCaidaRapida = 3f;
    public float impulsoInstantaneo = 5f;
    private bool esCaidaRapida = false;
    private float gravedadBase;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public float radioSuelo = 0.2f;
    public LayerMask capaSuelo;
    public bool enSuelo;

    private Rigidbody2D rb;
    private Animator anim;

    private bool estaCargandoSmash = false;
    private bool estaHaciendoAtaque = false;
    private bool estaHaciendoRecovery = false;

    [Header("Combate General")]
    public Hitbox hitboxAtaque;
    public AttackData[] arsenalDeAtaques;
    private float multiplicadorCalculado = 1f;

    [Header("Proyectil (Ataque a Distancia)")]
    public GameObject proyectilPrefab;
    public Transform puntoDeDisparo;

    [Header("Combo Jab (J)")]
    public float comboWindowTime = 0.6f;
    [SerializeField] private float currentComboTimer = 0f;
    private int comboStateJab = 0;
    private bool estaEnComboGap = false;

    [Header("Smash Attack (K)")]
    public float tiempoMaximoCarga = 2f;
    public float multiplicadorMaximo = 2f;
    public float tiempoCargaActual = 0f;

    [Header("Cooldowns")]
    public float cooldownSmash = 1.2f;
    public float cooldownProyectil = 1.0f;
    private float timerCooldownSmash = 0f;
    private float timerCooldownProyectil = 0f;

    [Header("Recovery (Salto + L)")]
    public float velocidadDashRecovery = 20f;
    public float duracionDashRecovery = 0.2f;
    private float timerDashRecovery = 0f;
    private bool yaUsoRecovery = false;

    [Header("Sistema de Daño (Smash)")]
    public float porcentajeDaño = 0f;
    public float peso = 100f;
    public bool estaEnHitstun = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gravedadBase = rb.gravityScale;
        saltosRestantes = saltosMaximos;
        vidasActuales = vidasMaximas;
        if (miInterfaz != null)
        {
            miInterfaz.ActualizarVidas(vidasActuales);
        }
    }

    void Update()
    {
        if (bloqueadoPorIntro)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (!esJugadorLocal && recibioPrimeraPosicion)
        {
            transform.position = Vector3.Lerp(transform.position, redPosicionObjetivo, Time.deltaTime * 25f);
        }

        if (estaEnHitstun) return;
        
        RecogerInput();
        
        if (esJugadorLocal)
        {
            enSuelo = Physics2D.OverlapCircle(groundCheck.position, radioSuelo, capaSuelo);
            anim.SetBool("isGrounded", enSuelo);
            anim.SetFloat("yVelocity", rb.linearVelocity.y);
        }
        else
        {
            enSuelo = redEnSuelo;
            anim.SetBool("isGrounded", enSuelo);
            anim.SetFloat("yVelocity", redVelY);
        }

        anim.SetBool("isAttacking", estaHaciendoAtaque || estaCargandoSmash);

        if (enSuelo)
        {
            saltosRestantes = saltosMaximos;
            yaUsoRecovery = false;

            if (rb.linearVelocity.y <= 0.1f)
            {
                estaHaciendoRecovery = false;
                anim.SetBool("isRecovering", false);
            }

            if (esCaidaRapida)
            {
                esCaidaRapida = false;
                rb.gravityScale = gravedadBase;
            }
            else if (rb.linearVelocity.y < 0f)
            {
                anim.SetBool("isRecovering", false);
            }
        }

        if (timerCooldownSmash > 0f)     timerCooldownSmash     -= Time.deltaTime;
        if (timerCooldownProyectil > 0f) timerCooldownProyectil -= Time.deltaTime;

        if (estaCargandoSmash)
        {
            ManejarEstadoCargaSmash();
        }
        else if (estaHaciendoAtaque)
        {
            HandleJabComboState();
        }
        else if (estaHaciendoRecovery)
        {
            saltosRestantes = 0;
            if (timerDashRecovery > 0)
            {
                timerDashRecovery -= Time.deltaTime;
            }
        }
        else
        {
            ManejarEstadoLibre();
        }

        if (!estaEnComboGap && !anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && hitboxAtaque != null && hitboxAtaque.gameObject.activeSelf)
        {
            DesactivarHitbox();
            estaHaciendoAtaque = false;
            estaHaciendoRecovery = false;
            anim.SetBool("isRecovering", false);
        }
    }

    private void ManejarEstadoCargaSmash()
    {
        if (tiempoCargaActual < tiempoMaximoCarga)
        {
            tiempoCargaActual += Time.deltaTime;
        }

        if (esJugadorLocal || (MatchManager.Instance != null && MatchManager.Instance.isVsCpu && !esJugadorLocal))
        {
            if (inputSmashUp || tiempoCargaActual >= tiempoMaximoCarga)
            {
                EjecutarLanzamientoSmash();
                if (NetworkManager.Instancia != null) NetworkManager.Instancia.EnviarAccion("smash_release");
            }
        }
    }

    public void EjecutarLanzamientoSmash()
    {
        float porcentajeCarga = tiempoCargaActual / tiempoMaximoCarga;
        multiplicadorCalculado = Mathf.Lerp(1f, multiplicadorMaximo, porcentajeCarga);
        anim.SetTrigger("LanzarSmash");

        estaCargandoSmash = false;
        estaHaciendoAtaque = true;
        anim.SetBool("CargandoSmash", false);
        timerCooldownSmash = cooldownSmash;
    }

    public void ContinuarComboJab()
    {
        estaEnComboGap = false;
        currentComboTimer = 0f;
        DesactivarHitbox();
        anim.SetBool("HacerComboJab", true);
        comboStateJab = 3;
    }

    private void HandleJabComboState()
    {
        ActualizarOrientacion();

        if (comboStateJab == 2)
        {
            currentComboTimer += Time.deltaTime;

            if (inputJabDown || inputJabHeld)
            {
                ContinuarComboJab();
                if (NetworkManager.Instancia != null) NetworkManager.Instancia.EnviarAccion("combo_jab");
            }
            else if (currentComboTimer >= comboWindowTime)
            {
                estaEnComboGap = false;
                estaHaciendoAtaque = false;
                comboStateJab = 0;
            }
        }
    }

    private void ManejarEstadoLibre()
    {
        anim.SetBool("isRunning", inputX != 0);
        ActualizarOrientacion();

        ManejarMovimientoVerticalLocal();

        if (inputRecovery && !yaUsoRecovery)
        {
            EjecutarRecovery();
            if (NetworkManager.Instancia != null) NetworkManager.Instancia.EnviarAccion("recovery");
        }
        else if (inputJabDown)
        {
            EjecutarJab();
            if (NetworkManager.Instancia != null) NetworkManager.Instancia.EnviarAccion("jab");
        }
        else if (inputSmashDown && enSuelo && timerCooldownSmash <= 0f)
        {
            IniciarCargaSmash();
            if (NetworkManager.Instancia != null) NetworkManager.Instancia.EnviarAccion("smash_start");
        }
        else if (inputProyectilDown && timerCooldownProyectil <= 0f)
        {
            EjecutarProyectil();
            if (NetworkManager.Instancia != null) NetworkManager.Instancia.EnviarAccion("proyectil");
        }
    }

    private void ActualizarOrientacion()
    {
        if (inputX > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (inputX < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    void ManejarMovimientoVerticalLocal()
    {
        if (inputJumpDown)
        {
            if (enSuelo) EjecutarSalto();
            else if (saltosRestantes > 1) { EjecutarSalto(); saltosRestantes--; }
        }

        if (!enSuelo && rb.linearVelocity.y <= 0.1f && !esCaidaRapida)
        {
            if (inputCaidaRapida)
            {
                EjecutarCaidaRapida();
            }
        }
    }

    private void RecogerInput()
    {
        // Si este personaje está siendo controlado por la IA, ignoramos el teclado
        FighterAgent agent = GetComponent<FighterAgent>();
        if (agent != null && agent.enabled) return;

        if (esJugadorLocal)
        {
            inputX = Input.GetAxisRaw("Horizontal");
            inputJumpDown = Input.GetButtonDown("Jump");
            inputJumpHeld = Input.GetButton("Jump");
            inputCaidaRapida = Input.GetAxisRaw("Vertical") < -0.5f;
            inputJabDown = Input.GetKeyDown(KeyCode.J);
            inputJabHeld = Input.GetKey(KeyCode.J);
            inputSmashDown = Input.GetKeyDown(KeyCode.K);
            inputSmashUp = Input.GetKeyUp(KeyCode.K);
            inputProyectilDown = Input.GetKeyDown(KeyCode.L);
            inputRecovery = Input.GetButton("Jump") && Input.GetKeyDown(KeyCode.L);
        }
    }

    public void EjecutarSalto()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.linearVelocity += Vector2.up * fuerzaSalto;
        if (esCaidaRapida) { esCaidaRapida = false; rb.gravityScale = gravedadBase; }
    }

    public void EjecutarCaidaRapida()
    {
        esCaidaRapida = true;
        rb.gravityScale = gravedadBase * multiplicadorCaidaRapida;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - impulsoInstantaneo);
    }

    public void EjecutarRecovery()
    {
        yaUsoRecovery = true;
        estaHaciendoRecovery = true;
        timerDashRecovery = duracionDashRecovery;
        anim.SetBool("isRecovering", true);
        anim.SetTrigger("HacerRecovery");
    }

    public void EjecutarJab()
    {
        multiplicadorCalculado = 1f;
        estaHaciendoAtaque = true;
        comboStateJab = 1;
        anim.SetTrigger("JabStart");
    }

    public void IniciarCargaSmash()
    {
        estaCargandoSmash = true;
        tiempoCargaActual = 0f;
        anim.SetBool("CargandoSmash", true);
    }

    public void EjecutarProyectil()
    {
        if (proyectilPrefab != null && puntoDeDisparo != null)
        {
            estaHaciendoAtaque = true;
            timerCooldownProyectil = cooldownProyectil;
            anim.SetTrigger("LanzarProyectil");
        }
    }

    void FixedUpdate()
    {
        if (bloqueadoPorIntro) return;
        if (estaEnHitstun) return;
        
        if (estaHaciendoRecovery && timerDashRecovery > 0)
        {
            rb.linearVelocity = new Vector2(inputX * velocidadCorrer, velocidadDashRecovery);
        }
        else
        {
            rb.linearVelocity = new Vector2(inputX * velocidadCorrer, rb.linearVelocity.y);
        }

        if (esJugadorLocal && NetworkManager.Instancia != null)
        {
            movSendTimer += Time.fixedDeltaTime;
            if (movSendTimer >= MOV_SEND_INTERVAL)
            {
                movSendTimer = 0f;
                NetworkManager.Instancia.EnviarMovimiento(transform.position.x, transform.position.y, transform.localScale.x, inputX, rb.linearVelocity.y, enSuelo);
            }
        }
    }

    public void ActualizarPosicionRed(float x, float y, float velY, bool suelo)
    {
        redPosicionObjetivo = new Vector3(x, y, transform.position.z);
        redVelY = velY;
        redEnSuelo = suelo;

        if (!recibioPrimeraPosicion)
        {
            transform.position = redPosicionObjetivo;
            recibioPrimeraPosicion = true;
        }
    }

    public void RecibirDaño(float dañoRecibido, Vector2 direccion, float empujeBase, float escalado)
    {
        porcentajeDaño += dañoRecibido;
        if (miInterfaz != null) miInterfaz.ActualizarPorcentaje(porcentajeDaño);

        float knockbackTotal = empujeBase + ((porcentajeDaño * dañoRecibido * escalado) / (peso * 0.1f));
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direccion * knockbackTotal, ForceMode2D.Impulse);

        float tiempoAturdimiento = knockbackTotal * 0.025f;
        StartCoroutine(RutinaHitstun(tiempoAturdimiento));
    }

    public void PerderVida()
    {
        vidasActuales--;
        FindObjectOfType<GameManager>().ComprobarVictoria();
        if (miInterfaz != null) miInterfaz.ActualizarVidas(vidasActuales);

        if (esJugadorLocal && NetworkManager.Instancia != null)
        {
            NetworkManager.Instancia.EnviarMuerte();
        }

        if (vidasActuales > 0)
        {
            transform.position = puntoDeRespawn.position;
            rb.linearVelocity = Vector2.zero;
            porcentajeDaño = 0f;
            if (miInterfaz != null) miInterfaz.ActualizarPorcentaje(porcentajeDaño);
            estaEnHitstun = false;
            estaHaciendoAtaque = false;
            estaCargandoSmash = false;
            estaHaciendoRecovery = false;
            anim.SetBool("isRecovering", false);
            anim.SetBool("CargandoSmash", false);
            DesactivarHitbox();
        }
        else
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm == null || !gm.modoEntrenamiento)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void PerderVidaRed()
    {
        vidasActuales--;
        FindObjectOfType<GameManager>().ComprobarVictoria();
        if (miInterfaz != null) miInterfaz.ActualizarVidas(vidasActuales);

        if (vidasActuales > 0)
        {
            rb.linearVelocity = Vector2.zero;
            porcentajeDaño = 0f;
            if (miInterfaz != null) miInterfaz.ActualizarPorcentaje(porcentajeDaño);
            estaEnHitstun = false;
            estaHaciendoAtaque = false;
            estaCargandoSmash = false;
            estaHaciendoRecovery = false;
            recibioPrimeraPosicion = false; 
            anim.SetBool("isRecovering", false);
            anim.SetBool("CargandoSmash", false);
            DesactivarHitbox();
        }
        else
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm == null || !gm.modoEntrenamiento)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void AplicarDañoVisual(float dañoRecibido)
    {
        porcentajeDaño += dañoRecibido;
        if (miInterfaz != null) miInterfaz.ActualizarPorcentaje(porcentajeDaño);
    }

    private System.Collections.IEnumerator RutinaHitstun(float tiempo)
    {
        estaEnHitstun = true;
        DesactivarHitbox();
        estaHaciendoAtaque = false;
        estaCargandoSmash = false;
        estaHaciendoRecovery = false;
        anim.SetBool("isRecovering", false);
        anim.SetBool("CargandoSmash", false);

        yield return new WaitForSeconds(tiempo);
        estaEnHitstun = false;
    }

    public void TerminarRecovery()
    {
        estaHaciendoRecovery = false;
        DesactivarHitbox();
    }

    public void ActivarHitboxJab(int indiceArsenal)
    {
        hitboxAtaque.ataqueActual = arsenalDeAtaques[indiceArsenal];
        hitboxAtaque.multiplicadorActual = multiplicadorCalculado;
        hitboxAtaque.gameObject.SetActive(true);
    }

    public void DesactivarHitbox()
    {
        if (hitboxAtaque != null)
        {
            hitboxAtaque.gameObject.SetActive(false);
            hitboxAtaque.ataqueActual = null;
            hitboxAtaque.multiplicadorActual = 1f;
        }
    }

    public void EntrarEnComboGap()
    {
        comboStateJab = 2;
        currentComboTimer = 0f;
        estaEnComboGap = true;
    }

    public void TerminarSecuenciaJab()
    {
        estaHaciendoAtaque = false;
        comboStateJab = 0;
        anim.SetBool("HacerComboJab", false);
        DesactivarHitbox();
    }

    public void Disparar()
    {
        if (proyectilPrefab != null && puntoDeDisparo != null)
        {
            Quaternion rotacionProyectil = puntoDeDisparo.rotation;
            if (transform.localScale.x < 0)
            {
                rotacionProyectil = Quaternion.Euler(0, 180, 0);
            }
            GameObject proy = Instantiate(proyectilPrefab, puntoDeDisparo.position, rotacionProyectil);
            Proyectil scriptProy = proy.GetComponent<Proyectil>();
            if (scriptProy != null)
            {
                scriptProy.jugadorQueDispara = this;
            }
        }
    }

    public void FinalizarAtaque()
    {
        estaHaciendoAtaque = false;
    }

    public void IniciarSecuenciaIntro(float tiempo)
    {
        StartCoroutine(RutinaIntro(tiempo));
    }

    private System.Collections.IEnumerator RutinaIntro(float tiempo)
    {
        bloqueadoPorIntro = true;
        yield return null; 
        if (anim != null) 
        {
            anim.SetTrigger("Intro");
        }

        yield return new WaitForSeconds(tiempo);
        bloqueadoPorIntro = false;
    }
}