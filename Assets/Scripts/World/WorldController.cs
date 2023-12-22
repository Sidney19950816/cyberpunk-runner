using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.World
{
    public sealed class WorldController:MonoBehaviour
    {
        private readonly Queue<Chunk> _queue = new();

        private readonly HashSet<Location> _currentLocations = new();
        private readonly List<LocationInstance> _locationChunks = new();

        [SerializeField][UsedImplicitly] private Chunk[] _tutorial;
        [SerializeField][UsedImplicitly] private Location[] _locations;

        [UsedImplicitly]
        private void Start()
        {
            InitializeLocations();
        }

        private Chunk _previous;
        private Chunk _current;
        private Chunk _next;
        public List<Chunk> chunks = new List<Chunk>();
        private int spawnedTimes;
        private int spawnedChanks = 3;

        private void InitializeLocations()
        {
            if(!PlayerPrefsUtil.GetTutorialCompleted()) {
                InitializeTutorial();
            }

            foreach (var location in _locations)
                InitializeLocation(location);

            UpdateChunks();
            StateManager.SetState(new GameState());
        }

        private void InitializeTutorial()
        {
            var chunks = _tutorial.Select(InitializeTutorialChunk);

            foreach (var chunk in chunks)
                _queue.Enqueue(chunk);
        }

        private void InitializeLocation(Location location)
        {
            var start = location.Start.Select(InitializeStartChunk).ToArray();
            var body = location.Body.Select(InitializeBodyChunk).ToArray();
            var end = location.End.Select(c => InitializeEndChunk(c,location)).ToArray();

            _locationChunks.Add(new LocationInstance(location,start,body,end));
        }

        private Chunk InitializeStartChunk(Chunk chunk)
        {
            return InitializeChunk(chunk,false);
        }

        private Chunk InitializeBodyChunk(Chunk chunk)
        {
            return InitializeChunk(chunk,false);
        }

        private Chunk InitializeEndChunk(Chunk chunk,Location location)
        {
            var instance = InitializeChunk(chunk,false);

            instance.OnChunkDisable += () => _currentLocations.Remove(location);

            return instance;
        }

        private Chunk InitializeTutorialChunk(Chunk chunk) => InitializeChunk(chunk,true);

        private Chunk InitializeChunk(Chunk chunk,bool destroyOnDisable)
        {
            var instance = Instantiate(chunk,transform);

            instance.OnChunkEnter += OnChunkEnter;
            instance.OnChunkDisable += destroyOnDisable ? instance.Destroy : instance.Disable;

            instance.Initialize();
            instance.Disable();

            return instance;
        }

        private void OnChunkEnter(Chunk chunk)
        {
            if (chunk == _current)
                return;

            //if (chunk != _next)
            //    throw new Exception("Invalid chunk enter");

            UpdateChunks();
        }

        private void UpdateChunks()
        {
            for (int i = 0; i < spawnedChanks; i++)
            {
                if (spawnedTimes >= 2)
                {
                    var chunk = chunks[0];
                    chunk.OnChunkDisable?.Invoke();
                    chunks.RemoveAt(0);
                    if(spawnedTimes == 2)
                    chunks.RemoveAt(0);
                }

                if (_current == null)
                {
                    EnqueueLocation();

                    _current = _queue.Dequeue();
                    chunks.Add(_current);
                    _current.Enable();
                }
                else
                {
                    if (i == 0)
                    {
                        _previous = _current;
                    }
                    _current = _next;

                    if (!_queue.Any())
                        EnqueueLocation();
                }              

                _next = _queue.Dequeue();
                _next.Enable(_current);
                chunks.Add(_current);
            }           
            spawnedChanks = 1;
            spawnedTimes++;
        }

        private void EnqueueLocation()
        {
            var locationInstance = _locationChunks.GetRandom(l => !_currentLocations.Contains(l.Location));

            _currentLocations.Add(locationInstance.Location);

            _queue.Enqueue(locationInstance.Start.GetRandom());
            _queue.Enqueue(locationInstance.Body.GetRandom());
            _queue.Enqueue(locationInstance.End.GetRandom());
        }

        private sealed class LocationInstance
        {
            public LocationInstance(Location location, Chunk[] start, Chunk[] body, Chunk[] end)
            {
                Location = location;

                Start = start;
                Body = body;
                End = end;
            }

            public Location Location { get; }

            public Chunk[] Start { get; }
            public Chunk[] Body { get; }
            public Chunk[] End { get; }
        }
    }
}
