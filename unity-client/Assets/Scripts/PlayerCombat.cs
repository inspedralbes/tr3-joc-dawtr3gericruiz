using UnityEngine;
using TMPro;

public class PlayerCombat : MonoBehaviour
{
    [Header("Estat (Dany % Acumulat)")]
    [HideInInspector]
    public float damagePercentage = 0f;

    [Header("Interfície (UI)")]
    public TextMeshProUGUI damageText;

    [Header("Configuració de Tecles")]
    public KeyCode keyJab = KeyCode.J;
    public KeyCode keyPatada = KeyCode.K;
    public KeyCode keySmash = KeyCode.L;

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Punts d'Atac (Assignar a l'Inspector)")]
    public Transform AttackPoint_Jab; 
    public Transform attackPointPeu;
    public Transform attackPointPuny;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(keyJab))
        {
            if (anim != null) anim.SetTrigger("jab");
        }
        else if (Input.GetKeyDown(keyPatada))
        {
            if (anim != null) anim.SetTrigger("patada");
        }
        else if (Input.GetKeyDown(keySmash))
        {
            if (anim != null) anim.SetTrigger("smash_endavant");
        }
    }

    public void ActivarHitbox(string tipusAtac)
    {
        float danyActual = 0f;
        Transform puntActual = null;

        switch (tipusAtac)
        {
            case "jab":
                danyActual = 4f;
                puntActual = AttackPoint_Jab;
                break;
            case "patada":
                danyActual = 8f;
                puntActual = attackPointPeu;
                break;
            case "smash_endavant":
                danyActual = 15f;
                puntActual = attackPointPuny;
                break;
            default:
                Debug.LogWarning("Atac no reconegut: " + tipusAtac);
                return;
        }

        if (puntActual == null) return; 

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(puntActual.position, attackRange, enemyLayers);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.gameObject == this.gameObject) continue;

            PlayerCombat enemyScript = enemyCollider.GetComponent<PlayerCombat>();
            if (enemyScript != null)
            {
                Vector2 direction = (enemyCollider.transform.position - transform.position).normalized;
                direction.y += 0.5f; 
                direction = direction.normalized;
                
                enemyScript.TakeDamage(danyActual, direction);
            }
        }
    }

    public void TakeDamage(float amount, Vector2 direction)
    {
        damagePercentage += amount;
        UpdateUI();
        
        if (anim != null) anim.SetTrigger("hit");
        
        float knockbackForce = 5f + (damagePercentage * 0.2f);
        
        rb.linearVelocity = Vector2.zero; 
        
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        
        Debug.Log($"{gameObject.name} ha rebut dany. Dany actual: {damagePercentage}%");
    }

    void UpdateUI()
    {
        if (damageText != null) {
            damageText.text = Mathf.Round(damagePercentage) + "%"; 
        }
    }
}