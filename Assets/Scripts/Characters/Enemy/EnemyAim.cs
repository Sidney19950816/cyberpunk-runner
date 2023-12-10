using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;

public class EnemyAim : CharacterAim
{
    [Space]
    [SerializeField] private GameObject _weaponLaser;

    private IEnumerator SetIKLowerBodyWeightCoroutine(float startValue, float targetValue, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            SetIKLowerBodyWeight(Mathf.Lerp(startValue, targetValue, elapsedTime / duration));

            elapsedTime += Time.deltaTime / Time.timeScale;
            yield return null;
        }

        SetIKLowerBodyWeight(targetValue);
    }

    private void SetIKLowerBodyWeight(float value)
    {
        IK.solver.bodyEffector.positionWeight = value;

        IK.solver.rightFootEffector.positionWeight = value;
        IK.solver.rightFootEffector.rotationWeight = value;

        IK.solver.leftFootEffector.positionWeight = value;
        IK.solver.leftFootEffector.rotationWeight = value;
    }

    protected override IEnumerator WaitForTarget(Transform target, float duration)
    {
        float elapsedTime = 0f;
        float randomDuration = UnityEngine.Random.Range(0, 0.5f);

        while (elapsedTime < duration)
        {
            transform.LookAt(target);

            if (elapsedTime > randomDuration)
                Animator.SetFloat(AIM_FORWARD, 1);

            elapsedTime += Time.deltaTime / Time.timeScale;
            yield return null;
        }

        if (AimController != null)
            AimController.target = target;
    }

    protected override void SetIKUpperBodyWeight(float value)
    {
        IK.solver.rightShoulderEffector.positionWeight = value;
        IK.solver.rightHandEffector.positionWeight = value;
        IK.solver.rightHandEffector.rotationWeight = value;
        IK.solver.rightArmMapping.weight = value;
        IK.solver.rightArmChain.pull = value;

        IK.solver.leftShoulderEffector.positionWeight = value;
        IK.solver.leftHandEffector.positionWeight = value;
        IK.solver.leftHandEffector.rotationWeight = value;
        IK.solver.leftArmMapping.weight = value;
        IK.solver.leftArmChain.pull = value;
    }

    public override void Set(Transform target)
    {
        ToggleAim(true);

        StartCoroutine(SetIKLowerBodyWeightCoroutine(1, 0, 1));
        StartCoroutine(SetIKUpperBodyWeightCoroutine(1, 0, 3));
        StartCoroutine(WaitForTarget(target, 1));

        _weaponLaser?.SetActive(true);
    }
}
