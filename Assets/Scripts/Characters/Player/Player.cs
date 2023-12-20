using System.Collections;
using UnityEngine;
using System;

namespace Assets.Scripts
{
    public class Player : Character
    {
        [SerializeField] private PlayerWeaponInitializer _weaponInitializer;
        
        private PlayerAim _playerAim;

        public PlayerWeapon Weapon { get; private set; }

        public PlayerAim Aim => _playerAim;

        public PlayerScore Score { get; } = new PlayerScore(0);

        public int Rokens => Score?.Score ?? 0;

        protected override void Start()
        {
            base.Start();

            Weapon = _weaponInitializer.Initialize();

            _playerAim = GetComponent<PlayerAim>()
                .With(p => p.Initialize(Animator, IK, AimController));
        }

        public void CollectRokens(int multiplier = 1)
        {
            EconomyManager.Instance.IncrementCurrencyBalance(CurrencyType.ROKEN.ToString(), Rokens * multiplier);
            Score.Reset();
        }
    }
}
