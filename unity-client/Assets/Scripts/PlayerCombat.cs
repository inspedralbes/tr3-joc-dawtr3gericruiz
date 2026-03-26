using UnityEngine;
using TMPro;

public class PlayerCombat : MonoBehaviour
{
    [Header("Estat (Dany %)")]
    public float damagePercentage = 0f;

    [Header("Interfície (UI)")]
    public TextMeshProUGUI damageText;

    [Header("Configuració General")]
    public Transform attackPoint;      
    public float attackRange = 0.5f;   
    public LayerMask enemyLayers;      
    public KeyCode attackKey = KeyCode.J; 

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
        {
            Attack();
        }
    }

    void Attack()
    {
        if (anim != null) anim.SetTrigger("attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            PlayerCombat enemyScript = enemyCollider.GetComponent<PlayerCombat>();

            if (enemyScript != null)
            {
                Vector2 direction = (enemyCollider.transform.position - transform.position).normalized;
                direction.y += 0.5f; 
                direction = direction.normalized;

                enemyScript.TakeDamage(10f, direction);
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

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}