using UnityEngine;

public class StartAnimationOnTrigger : MonoBehaviour
{
    public Animator animator;
    
    void OnTriggerEnter(Collider other)
    {
        if (animator != null)
        {
            animator.SetTrigger("Play");
        }
    }
}