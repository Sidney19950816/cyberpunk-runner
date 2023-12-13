using Assets.Scripts.World;
using System;
using UnityEngine;

public class BikeReset : MonoBehaviour, IResettable
{
    [Min(1f)]
    [SerializeField] private float _resetTime = 1f;

    private Transform _chunkStartPoint;
    private Transform _chunkEndPoint;
    private bool _isChunkCurved;

    private Rigidbody _rigidbody;

    private float _timeFacingWrongWay = 0f;

    public void SetChunkContext(Chunk chunk)
    {
        _chunkStartPoint = chunk.StartPoint;
        _chunkEndPoint = chunk.EndPoint;
        _isChunkCurved = chunk.IsCurved();
    }

    public void Reset()
    {
        if (_isChunkCurved)
        {
            Vector3 chunkStartToBike = transform.position - _chunkStartPoint.position;
            Vector3 chunkStartToEnd = _chunkEndPoint.position - _chunkStartPoint.position;
            Vector3 cross = Vector3.Cross(chunkStartToBike, chunkStartToEnd);

            float y = 90 * CalculateDistanceOnChunk(transform.position, _chunkStartPoint.position, _chunkEndPoint.position);
            y = cross.y < 0 ? _chunkStartPoint.eulerAngles.y - y : y;
            transform.eulerAngles = new Vector3(0, y, 0);
        }
        else
        {
            transform.rotation = _chunkStartPoint.rotation;
        }
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (_chunkStartPoint == null) return;

        if (!(Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)))
        {
            CheckStuckState();
        }

        if (IsFacingWrongWay())
        {
            _timeFacingWrongWay += Time.deltaTime;

            if (_timeFacingWrongWay >= _resetTime)
            {
                Reset();
            }
        }
        else
        {
            _timeFacingWrongWay = 0f;
        }
    }

    private bool IsFacingWrongWay()
    {
        if (_isChunkCurved)
        {
            float distanceToStart = Vector3.Distance(transform.position, _chunkStartPoint.position);
            float distanceToEnd = Vector3.Distance(transform.position, _chunkEndPoint.position);
            Vector3 playerToPoint = distanceToEnd < distanceToStart ? _chunkEndPoint.position - transform.position : transform.position - _chunkStartPoint.position;
            float dotProduct = Vector3.Dot(transform.forward.normalized, playerToPoint.normalized);
            float checkValue = distanceToEnd < distanceToStart ? -0.7f : 0;

            if (dotProduct < checkValue)
            {
                return true;
            }
        }
        else
        {
            float dot = Vector3.Dot(transform.forward, (transform.position - _chunkStartPoint.position).normalized);
            if (dot < 0)
            {
                return true;
            }
        }

        return false;
    }

    private float CalculateDistanceOnChunk(Vector3 position, Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 chunkDirection = (endPoint - startPoint).normalized;
        float distance = Vector3.Dot(position - startPoint, chunkDirection);
        float chunkLength = Vector3.Distance(startPoint, endPoint);
        return Mathf.Clamp01(distance / chunkLength);
    }

    private void CheckStuckState()
    {
        if (_rigidbody.velocity.magnitude < 0.1f)
        {
            Reset();
        }
    }
}
