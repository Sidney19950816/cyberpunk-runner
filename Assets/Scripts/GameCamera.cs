using System;
using Assets.Scripts.Abstractions;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts
{
    public sealed class GameCamera : BaseBehaviour
    {
        [SerializeField] [UsedImplicitly] private Transform _parent;

        [UsedImplicitly]
        private void Awake()
        {
            if (_parent == null)
            {
                throw new Exception("GameCamera parent is null.");
            }
        }

        public void Set(Transform attach, Transform lookAt)
        {
            transform.SetParent(attach);
            ResetPosition();
        }

        public void Reset()
        {
            transform.SetParent(_parent);
            ResetPosition();
        }

        // TODO: Make attachment smooth
        private void ResetPosition()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}