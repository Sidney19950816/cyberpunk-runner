using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;

public abstract class CharacterAim : MonoBehaviour
{
    protected const string AIM_FORWARD = "Aim Forward";

    private Animator _animator;
    private FullBodyBipedIK _ik;
    private AimController _aimController;

    protected Animator Animator => _animator;
    protected FullBodyBipedIK IK => _ik;
    protected AimIK AimIK => _aimController.ik;
    protected AimController AimController => _aimController;

    protected void ToggleAim(bool enable)
    {
        AimIK.enabled = enable;
        AimController.enabled = enable;
        Animator.updateMode = enable ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
    }

    protected IEnumerator SetIKUpperBodyWeightCoroutine(float startValue, float targetValue, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            SetIKUpperBodyWeight(Mathf.Lerp(startValue, targetValue, elapsedTime / duration));

            elapsedTime += Time.deltaTime / Time.timeScale;
            yield return null;
        }

        SetIKUpperBodyWeight(targetValue);
    }

    public void Initialize(Animator animator, FullBodyBipedIK ik, AimController aimController)
    {
        _animator = animator;
        _ik = ik;
        _aimController = aimController;
    }

    protected abstract void SetIKUpperBodyWeight(float value);
    protected abstract IEnumerator WaitForTarget(Transform target, float duration);
    public abstract void Set(Transform target);

}
