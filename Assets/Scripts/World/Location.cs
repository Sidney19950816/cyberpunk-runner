using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.World
{
    [CreateAssetMenu(fileName = "Location", menuName = "ScriptableObjects/Location")]
    public sealed class Location : ScriptableObject
    {
        [SerializeField] [UsedImplicitly] private Chunk[] _start;
        [SerializeField] [UsedImplicitly] private Chunk[] _body;
        [SerializeField] [UsedImplicitly] private Chunk[] _end;

        public IEnumerable<Chunk> Start => new List<Chunk>(_start);
        public IEnumerable<Chunk> Body => new List<Chunk>(_body);
        public IEnumerable<Chunk> End => new List<Chunk>(_end);
    }
}
