using UnityEngine;
using System.Collections;
using System.Linq;

public class EnemyBase : MonoBehaviour, IStunnable
{
    public int damage;

    public float speed;

    private bool isStunned = false;
    private MonoBehaviour[] movementScripts; // Todos los scripts que controlan movimiento
    
    [Header("Efecto Visual (Opcional)")]
    [SerializeField] private GameObject stunEffect;
    [SerializeField] private float stunAnimationSpeed = 0.3f;

    void Awake()
    {
        // Obtener todos los scripts de movimiento (excepto este)
        movementScripts = GetComponents<MonoBehaviour>()
                        .Where(script => script != this && script.enabled)
                        .ToArray();
    }

    public void Stun(float duration) 
    {
        if (isStunned) return; // Opcional: Si ya está aturdido, ignorar nuevo stun
        
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        // 1. Desactivar capacidad de movimiento
        isStunned = true;
        foreach (var script in movementScripts)
        {
            script.enabled = false;
        }

        // 2. Opcional: Efecto visual
        if (stunEffect != null) stunEffect.SetActive(true);
        if (TryGetComponent<Animator>(out var animator))
        {
            animator.speed = stunAnimationSpeed;
        }

        // 3. Esperar duración
        yield return new WaitForSeconds(duration);

        // 4. Restaurar movimiento
        foreach (var script in movementScripts)
        {
            script.enabled = true;
        }
        
        if (stunEffect != null) stunEffect.SetActive(false);
        if (animator != null) animator.speed = 1f;
        
        isStunned = false;
    }
}