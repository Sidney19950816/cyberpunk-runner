using System.Collections;
using UnityEngine;
using System;

namespace Assets.Scripts
{
    public class Player : CharacterController
    {
        [SerializeField] private Transform _weaponHolder;
        
        private PlayerAim _playerAim;

        private PlayerScore _playerScore;

        public PlayerWeapon Weapon { get; private set; }

        public PlayerAim Aim => _playerAim;

        public PlayerScore Score => _playerScore;

        public int Rokens => _playerScore?.Score ?? 0;

        protected override void Start()
        {
            base.Start();

            //EconomyManager.Instance.DeleteInventoryItemAsync(EconomyManager.Instance.PlayersInventoryItems.Find(i => i.InventoryItemId == Util.GetSelectedItemId("WEAPON")).PlayersInventoryItemId);
            if (!EconomyManager.Instance.PlayersInventoryItems.Exists(i => i.InventoryItemId.Contains("WEAPON")))
                EconomyManager.Instance.OnPurchaseClicked(EconomyManager.Instance.InventoryItemDefinitions.Find(i => i.Id == "WEAPON_0"), SetDefaultWeapon);
            else
                InstantiateWeapon();

            _playerScore = new PlayerScore(0);

            _playerAim = GetComponent<PlayerAim>()
                .With(p => p.Initialize(Animator, IK, AimController));
        }

        private void InstantiateWeapon()
        {
            var playersInventoryItem = EconomyManager.Instance.PlayersInventoryItems.Find(i => i.InventoryItemId == Util.GetSelectedItemId("WEAPON"));
            string jsonData = playersInventoryItem?.InstanceData.GetAsString();
            WeaponData weaponData = JsonUtility.FromJson<WeaponData>(jsonData);

            GameObject weaponObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load($"Game/Weapon/{Util.GetSelectedItemId("WEAPON")}"), _weaponHolder);

            Weapon = weaponObject.GetComponent<PlayerWeapon>();
            Weapon.Initialize(weaponData);
        }

        private void SetDefaultWeapon()
        {
            Util.SetSelectedItemId("WEAPON", "WEAPON_0");
            InstantiateWeapon();
        }

        public void CollectRokens(int multiplier = 1)
        {
            EconomyManager.Instance.IncrementCurrencyBalance(CurrencyType.ROKEN.ToString(), Rokens * multiplier);
            _playerScore.Reset();
        }
    }
}
