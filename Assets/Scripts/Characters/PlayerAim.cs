using System.Collections;
using UnityEngine;
using RootMotion.FinalIK;

public class PlayerAim : CharacterAim
{
    [Space]
    [SerializeField] private Transform _spine;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private Transform _weaponHolder;

    protected override IEnumerator WaitForTarget(Transform target, float duration)
    {
        float elapsedTime = 0f;
        float randomDuration = UnityEngine.Random.Range(0, 0.5f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime / Time.timeScale;
            yield return null;
        }

        _weaponHolder.transform.parent = _rightHand.transform;
        _weaponHolder.transform.localPosition = new Vector3(-0.04f, 0.1f, 0.02f);
        _weaponHolder.transform.localRotation = Quaternion.Euler(250, 160, -90);

        if (AimController != null)
            AimController.target = target;
    }

    protected override void SetIKUpperBodyWeight(float value)
    {
        IK.solver.rightHandEffector.positionWeight = value;
        IK.solver.rightArmMapping.weight = value;
        IK.solver.rightArmChain.pull = value;

        IK.solver.bodyEffector.positionWeight = value;
    }

    public override void Set(Transform target)
    {
        ToggleAim(true);

        Animator.SetInteger(AIM_FORWARD, 1);
        StartCoroutine(SetIKUpperBodyWeightCoroutine(1, 0, 0.25f));
        StartCoroutine(WaitForTarget(target, 0.75f));
    }

    public void Reset()
    {
        StartCoroutine(SetIKUpperBodyWeightCoroutine(0, 1, 2));
        Animator.SetInteger(AIM_FORWARD, 0);

        ToggleAim(false);

        _weaponHolder.transform.parent = _spine.transform;
        _weaponHolder.transform.localPosition = new Vector3(-0.06f, 0.06f, -0.12f);
        _weaponHolder.transform.localRotation = Quaternion.Euler(70, 210, 90);
    }
}
