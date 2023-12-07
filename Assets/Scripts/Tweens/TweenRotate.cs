using UnityEngine;

public class TweenRotate : MonoBehaviour
{
	private const string TIME = "time";
	private const string EASE_TYPE = "easeType";
	private const string LOOP_TYPE = "loopType";
	private const string DELAY = "delay";

	enum AxisEnum
    {
		x,
		y,
		z,
    };
	enum DirectionEnum
    {
		backward = -1,
		forward = 1
    };

	[Header("Rotates the axis 360 degrees in a given time")]

	[SerializeField] private AxisEnum _axis;
	[SerializeField] private DirectionEnum _direction = DirectionEnum.forward;
	[SerializeField] private float _time;
	[SerializeField] private iTween.EaseType _easeType = iTween.EaseType.linear;
	[SerializeField] private iTween.LoopType _loopType = iTween.LoopType.none;
	[SerializeField] private float _delay;

	void Start()
	{
		iTween.RotateBy(gameObject, iTween.Hash(_axis, (float)_direction, TIME, _time, EASE_TYPE, _easeType, LOOP_TYPE, _loopType, DELAY, _delay));
	}
}
