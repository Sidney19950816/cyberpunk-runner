using Assets.Scripts.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using System;

namespace Assets.Scripts
{
    public sealed class Bike : MonoBehaviour, IResettable
    {
        [SerializeField] [UsedImplicitly] private Player _player;
        [SerializeField] [UsedImplicitly] private LayerMask _ground;
        [SerializeField] [UsedImplicitly] private Rigidbody _rigidBody;
        [SerializeField] [UsedImplicitly] private BoxCollider _collider;

        public Rigidbody Rigidbody => _rigidBody;

        [field: SerializeField]
        [field: Range(-100f, 100f)]
        public float FightOffset { get; [UsedImplicitly] private set; }

        [field: SerializeField]
        [field: Range(0, 25)]
        public float AirForce { get; [UsedImplicitly] private set; }

        public Vector3 ResetPosition { get; private set; }
        public Vector3 ResetAngles { get; private set; }

        public Player Player { get { return _player != null ? _player : GetComponentInChildren<Player>(); } }

        public BoxCollider Collider { get { return _collider != null ? _collider : GetComponent<BoxCollider>(); } }

        private Transform chunkStartPoint;
        private Transform chunkEndPoint;

        [SerializeField] private float resetTime = 1f;
        private float timeFacingWrongWay = 0f;

        private int passedChunksCount;

        public void SetResetPosition(Transform startPos, Transform endPos)
        {
            ResetPosition = startPos.position;
            ResetAngles = startPos.eulerAngles;

            chunkStartPoint = startPos;
            chunkEndPoint = endPos;

            IncrementPassedChunksCount();
        }

        [UsedImplicitly]
        private void Start()
        {
            ResetPosition = transform.position;
            ResetAngles = transform.eulerAngles;
        }

        public float Height => gameObject.GetHeight(_ground);

        private void Update()
        {
            if (chunkStartPoint != null)
            {
                if (!(Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)))
                    CheckStuckState();

                if(IsFacingWrongWay())
                {
                    timeFacingWrongWay += Time.deltaTime;

                    if (timeFacingWrongWay >= resetTime)
                    {
                        Reset();
                    }
                }
                else
                {
                    timeFacingWrongWay = 0f;
                }
            }
        }

        void CheckStuckState()
        {
            if (Rigidbody.velocity.magnitude < 0.1f)
            {
                Reset();
            }
        }

        private float CalculateDistanceOnChunk(Vector3 position, Vector3 startPoint, Vector3 endPoint)
        {
            Vector3 chunkDirection = (endPoint - startPoint).normalized;
            float distance = Vector3.Dot(position - startPoint, chunkDirection);
            float chunkLength = Vector3.Distance(startPoint, endPoint);
            return Mathf.Clamp01(distance / chunkLength);
        }

        private bool IsFacingWrongWay()
        {
            if (IsChunkCurved())
            {
                float distanceToStart = Vector3.Distance(transform.position, chunkStartPoint.position);
                float distanceToEnd = Vector3.Distance(transform.position, chunkEndPoint.position);
                Vector3 playerToPoint = distanceToEnd < distanceToStart ? chunkEndPoint.position - transform.position : transform.position - chunkStartPoint.position;
                float dotProduct = Vector3.Dot(transform.forward.normalized, playerToPoint.normalized);
                float checkValue = distanceToEnd < distanceToStart ? -0.7f : 0;

                if (dotProduct < checkValue)
                {
                    return true;
                }
            }
            else
            {
                float dot = Vector3.Dot(transform.forward, (transform.position - chunkStartPoint.position).normalized);
                if (dot < 0)
                {
                    return true;
                }
            }

            return false;
            }

        private bool IsChunkCurved()
        {
            return chunkStartPoint.forward != chunkEndPoint.forward;
        }

        public int GetPassedChunksCount()
        {
            return passedChunksCount;
        }

        private void IncrementPassedChunksCount()
        {
            passedChunksCount++;
        }

        public void Reset()
        {
            if (IsChunkCurved())
            {
                Vector3 chunkStartToBike = transform.position - chunkStartPoint.position;
                Vector3 chunkStartToEnd = chunkEndPoint.position - chunkStartPoint.position;
                Vector3 cross = Vector3.Cross(chunkStartToBike, chunkStartToEnd);

                float y = 90 * CalculateDistanceOnChunk(transform.position, chunkStartPoint.position, chunkEndPoint.position);
                y = cross.y < 0 ? chunkStartPoint.eulerAngles.y - y : y;
                transform.eulerAngles = new Vector3(0, y, 0);
            }
            else
            {
                transform.rotation = chunkStartPoint.rotation;
            }
        }
    }
}