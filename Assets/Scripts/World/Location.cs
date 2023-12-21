using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Assets.Scripts.World
{
    [CreateAssetMenu(fileName = "Location", menuName = "ScriptableObjects/Location")]
    public sealed class Location : ScriptableObject
    {
        [SerializeField] [UsedImplicitly] private AssetReferenceGameObject[] _start;
        [SerializeField] [UsedImplicitly] private AssetReferenceGameObject[] _body;
        [SerializeField] [UsedImplicitly] private AssetReferenceGameObject[] _end;

        public IEnumerable<AssetReferenceGameObject> Start => new List<AssetReferenceGameObject>(_start);
        public IEnumerable<AssetReferenceGameObject> Body => new List<AssetReferenceGameObject>(_body);
        public IEnumerable<AssetReferenceGameObject> End => new List<AssetReferenceGameObject>(_end);
    }
}
