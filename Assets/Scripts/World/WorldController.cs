using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

        private async void InitializeLocations()
        {
            if (!PlayerPrefsUtil.GetTutorialCompleted())
            {
                InitializeTutorial();
            }

            foreach (var location in _locations)
                await InitializeLocationAsync(location);

            UpdateChunks();
            StateManager.SetState(new GameState());
        }

        private void InitializeTutorial()
        {
            var chunks = _tutorial.Select(InitializeTutorialChunk);

            foreach (var chunk in chunks)
                _queue.Enqueue(chunk);
        }

        private async Task<Chunk> InitializeChunkAsync(AssetReferenceGameObject chunkReference, bool destroyOnDisable)
        {
            var operation = chunkReference.LoadAssetAsync<GameObject>();
            await operation.Task;

            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                var loadedChunk = Instantiate(operation.Result, transform);
                var instance = loadedChunk.GetComponent<Chunk>();

                instance.OnChunkEnter += OnChunkEnter;
                instance.OnChunkDisable += destroyOnDisable ? instance.Destroy : instance.Disable;

                instance.Initialize();
                instance.Disable();

                return instance;
            }
            else
            {
                Debug.LogError($"Failed to load chunk: {chunkReference.editorAsset}");
                return null;
            }
        }

        private async Task InitializeLocationAsync(Location location)
        {
            var startChunks = await LoadChunksAsync(location.Start);
            var bodyChunks = await LoadChunksAsync(location.Body);
            var endChunks = await LoadChunksAsync(location.End);

            _locationChunks.Add(new LocationInstance(location, startChunks, bodyChunks, endChunks));
        }

        private async Task<List<Chunk>> LoadChunksAsync(IEnumerable<AssetReferenceGameObject> chunkReferences)
        {
            var loadTasks = chunkReferences.Select(chunkRef => InitializeChunkAsync(chunkRef, false));
            var chunks = await Task.WhenAll(loadTasks);

            return chunks.Where(chunk => chunk != null).ToList();
        }

        private async Task<Chunk> InitializeEndChunk(AssetReferenceGameObject chunkReference, Location location)
        {
            var instance = await InitializeChunkAsync(chunkReference, false);

            if (instance != null)
            {
                instance.OnChunkDisable += () => _currentLocations.Remove(location);
            }

            return instance;
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
            if (_previous != null)
                _previous.OnChunkDisable?.Invoke();

            if (_current == null)
            {
                EnqueueLocation();

                _current = _queue.Dequeue();
                _current.Enable();
            }
            else
            {
                _previous = _current;
                _current = _next;

                if (!_queue.Any())
                    EnqueueLocation();
            }

            _next = _queue.Dequeue();

            _next.Enable(_current);
        }

        private void EnqueueLocation()
        {
            var locationInstance = _currentLocations.Count > 1
                ? _locationChunks.GetRandom(l => !_currentLocations.Contains(l.Location))
                : _locationChunks[0];

            _currentLocations.Add(locationInstance.Location);

            _queue.Enqueue(locationInstance.Start.GetRandom());
            _queue.Enqueue(locationInstance.Body.GetRandom());
            _queue.Enqueue(locationInstance.End.GetRandom());
        }

        private sealed class LocationInstance
        {
            public LocationInstance(Location location, List<Chunk> start, List<Chunk> body, List<Chunk> end)
            {
                Location = location;

                Start = start;
                Body = body;
                End = end;
            }

            public Location Location { get; }

            public List<Chunk> Start { get; }
            public List<Chunk> Body { get; }
            public List<Chunk> End { get; }
        }
    }
}
