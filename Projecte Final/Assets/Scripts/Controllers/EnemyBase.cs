using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyBase : MonoBehaviour, IStunnable
{
    public int damage;
    public float speed;
    
    private bool isStunned = false;
    private List<Component> movementComponents; // Cambiado a Component
    
    [Header("Efecto Visual")]
    [SerializeField] private GameObject stunEffect;
    [SerializeField] private float stunAnimationSpeed = 0.3f;

    public void Stun(float duration) 
    {
        movementComponents = new List<Component>();
        
        // 1. Detectar NavMeshAgent (como Component)
        if (TryGetComponent(out NavMeshAgent navAgent))
            movementComponents.Add(navAgent);
        
        // 2. Detectar Rigidbody2D
        if (TryGetComponent(out Rigidbody2D rb))
            movementComponents.Add(rb);
        
        // 3. Detectar scripts de movimiento personalizados
        if (TryGetComponent(out MonsterPatrolController patrol))
            movementComponents.Add(patrol);

        if (isStunned) return;
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        
        // Desactivar componentes
        foreach (var component in movementComponents)
        {
            if (component is NavMeshAgent agent)
                agent.isStopped = true;
            else if (component is Rigidbody2D rb)
                rb.simulated = false;
            else if (component is MonoBehaviour script)
                script.enabled = false;
        }

        // Efectos visuales
        if (stunEffect != null) stunEffect.SetActive(true);
        if (TryGetComponent(out Animator animator))
            animator.speed = stunAnimationSpeed;

        yield return new WaitForSeconds(duration);

        // Reactivar
        foreach (var component in movementComponents)
        {
            if (component is NavMeshAgent agent)
                agent.isStopped = false;
            else if (component is Rigidbody2D rb)
                rb.simulated = true;
            else if (component is MonoBehaviour script)
                script.enabled = true;
        }
        
        if (stunEffect != null) stunEffect.SetActive(false);
        if (animator != null) animator.speed = 1f;
        
        isStunned = false;
    }
}