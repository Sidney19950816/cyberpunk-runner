using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Player : CharacterController, IScoreable
    {
        [SerializeField] private Transform _weaponHolder;
        [SerializeField] private Transform spine;
        [SerializeField] private Transform rightHand;

        public PlayerWeapon Weapon { get; private set; }

        public System.Action<long> OnEarnedRokens;

        private long earnedRokens;

        public long EarnedRokens { get { return earnedRokens; } }

        protected override void Start()
        {
            base.Start();

            //EconomyManager.Instance.DeleteInventoryItemAsync(EconomyManager.Instance.PlayersInventoryItems.Find(i => i.InventoryItemId == Util.GetSelectedItemId("WEAPON")).PlayersInventoryItemId);
            if (!EconomyManager.Instance.PlayersInventoryItems.Exists(i => i.InventoryItemId.Contains("WEAPON")))
                EconomyManager.Instance.OnPurchaseClicked(EconomyManager.Instance.InventoryItemDefinitions.Find(i => i.Id == "WEAPON_0"), SetDefaultWeapon);
            else
                InstantiateWeapon();

            OnEarnedRokens += AddScore;
        }

        private void InstantiateWeapon()
        {
            var playersInventoryItem = EconomyManager.Instance.PlayersInventoryItems.Find(i => i.InventoryItemId == Util.GetSelectedItemId("WEAPON"));
            string jsonData = playersInventoryItem?.InstanceData.GetAsString();
            WeaponData weaponData = JsonUtility.FromJson<WeaponData>(jsonData);

            GameObject weaponObject = (GameObject)Object.Instantiate(Resources.Load($"Game/Weapon/{Util.GetSelectedItemId("WEAPON")}"), _weaponHolder);

            Weapon = weaponObject.GetComponent<PlayerWeapon>();
            Weapon.Initialize(weaponData);
        }

        private void SetDefaultWeapon()
        {
            Util.SetSelectedItemId("WEAPON", "WEAPON_0");
            InstantiateWeapon();
        }

        public void CollectEarnedRokens(int multiplier = 1)
        {
            EconomyManager.Instance.IncrementCurrencyBalance(CurrencyType.ROKEN.ToString(), (int)earnedRokens * multiplier);
            earnedRokens = 0;
        }

        public override void SetAimTarget(Transform target)
        {
            base.SetAimTarget(target);

            Animator.SetInteger(AIM_FORWARD, 1);
            StartCoroutine(SetIKUpperBodyWeightCoroutine(1, 0, 0.25f));
            StartCoroutine(WaitForTarget(target, 0.75f));
        }

        private IEnumerator WaitForTarget(Transform target, float duration)
        {
            float elapsedTime = 0f;
            float randomDuration = UnityEngine.Random.Range(0, 0.5f);

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime / Time.timeScale;
                yield return null;
            }

            _weaponHolder.transform.parent = rightHand.transform;
            _weaponHolder.transform.localPosition = new Vector3(-0.04f, 0.1f, 0.02f);
            _weaponHolder.transform.localRotation = Quaternion.Euler(250, 160, -90);

            if (AimController != null)
                AimController.target = target;
        }

        private IEnumerator SetIKUpperBodyWeightCoroutine(float startValue, float targetValue, float duration)
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

        private void SetIKLowerBodyWeight(float value)
        {
            IK.solver.bodyEffector.positionWeight = value;

            IK.solver.rightFootEffector.positionWeight = value;
            IK.solver.rightFootEffector.rotationWeight = value;

            IK.solver.leftFootEffector.positionWeight = value;
            IK.solver.leftFootEffector.rotationWeight = value;
        }

        private void SetIKUpperBodyWeight(float value)
        {
            IK.solver.rightHandEffector.positionWeight = value;
            IK.solver.rightArmMapping.weight = value;
            IK.solver.rightArmChain.pull = value;

            IK.solver.bodyEffector.positionWeight = value;
        }

        public void ResetAimTarget()
        {
            StartCoroutine(SetIKUpperBodyWeightCoroutine(0, 1, 2));
            Animator.SetInteger(AIM_FORWARD, 0);
            AimIK.enabled = false;
            Animator.updateMode = AnimatorUpdateMode.Normal;
            AimController.enabled = false;

            _weaponHolder.transform.parent = spine.transform;
            _weaponHolder.transform.localPosition = new Vector3(-0.06f, 0.06f, -0.12f);
            _weaponHolder.transform.localRotation = Quaternion.Euler(70, 210, 90);
        }

        public void AddScore(long score)
        {
            earnedRokens += score;
        }

        public void ResetScore()
        {
            earnedRokens = 0;
        }
    }
}
