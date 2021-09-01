using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;

public class Hand : MonoBehaviour
{
	[SerializeField] private OVRHand _ovrHand = default;
	[SerializeField] private OVRHand.Hand _handType = default;

	[Space]
	[SerializeField] public Transform IndexProximal;
	[SerializeField] public Transform IndexMiddle;
	[SerializeField] public Transform IndexDistal;
	[SerializeField] public Transform MiddleProximal;
	[SerializeField] public Transform RingProximal;
	[SerializeField] public Transform PinkyProximal;

	[Space] [SerializeField] public Transform IndexTip;
	[SerializeField] public Transform RingTip;
	[SerializeField] public Transform PinkyTip;
	[SerializeField] public Transform MiddleTip;

	// VISUALS
	[Space]
	//[SerializeField] private ParticleSystem _plop;
	//[SerializeField] private SkinnedMeshRenderer _meshRenderer;
	//[SerializeField] private Material _materialDefault;
	//[SerializeField] private Material _materialTransparent;

	private readonly Subject<Hand> _onStartGrasping = new Subject<Hand>();
	private readonly Subject<Hand> _onStopGrasping = new Subject<Hand>();

	public IObservable<Hand> OnStartGrasping => _onStartGrasping;
	public IObservable<Hand> OnStopGrasping => _onStopGrasping;

	public bool IsGrasping;
	public OVRHand.Hand HandType => _handType;

	public bool IsDoublePinching = false;


	private readonly Dictionary<OVRHand.HandFinger, IObservable<float>> _onPinchesStarted = new Dictionary<OVRHand.HandFinger, IObservable<float>>();
	private readonly Dictionary<OVRHand.HandFinger, IObservable<float>> _onPinchesEnded = new Dictionary<OVRHand.HandFinger, IObservable<float>>();
	private readonly Dictionary<OVRHand.HandFinger, IObservable<bool>> _onPinches = new Dictionary<OVRHand.HandFinger, IObservable<bool>>();

	public IObservable<float> OnPinchStarted(OVRHand.HandFinger finger)
	{
		if (!_onPinchesStarted.ContainsKey(finger))
		{
			_onPinchesStarted.Add(finger, CreateOnPinch(finger).Where(p => p).Select(_ => Time.time));
		}

		return _onPinchesStarted[finger];
	}

	public IObservable<float> OnPinchEnded(OVRHand.HandFinger finger)
	{
		if (!_onPinchesEnded.ContainsKey(finger))
		{
			_onPinchesEnded.Add(finger, CreateOnPinch(finger).Where(p => !p).Select(_ => Time.time));
		}

		return _onPinchesEnded[finger];
	}

	public IObservable<bool> OnPinch(OVRHand.HandFinger finger)
	{
		if (!_onPinches.ContainsKey(finger))
		{
			_onPinches.Add(finger, CreateOnPinch(finger));
		}

		return _onPinches[finger];
	}

	private void Awake()
	{

	}

	private void Start()
	{
		//OnPinchStarted(OVRHand.HandFinger.Index).Subscribe(_ => { print("PINCH"); });
		//OnPinchStarted(OVRHand.HandFinger.Middle).Subscribe(_ => { Plop(MiddleTip.position); });
		//OnPinchStarted(OVRHand.HandFinger.Ring).Subscribe(_ => { Plop(RingTip.position); });
		//OnPinchStarted(OVRHand.HandFinger.Pinky).Subscribe(_ => { Plop(PinkyTip.position); });

		//CreateOnDoublePinch(OVRHand.HandFinger.Index).Subscribe(_ => { print("DOUBLE PINCH"); });

		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
	}

	private void Update()
	{
		CheckGrasping();
	}

	public bool IsTrackingGood()
	{
		return _ovrHand.HandConfidence == OVRHand.TrackingConfidence.High;
	}

	public Vector3 GetPosition()
	{
		return _ovrHand.transform.position;
	}

	public Quaternion GetRotation()
	{
		return _ovrHand.transform.rotation;
	}

	public Vector3 GetLocalPosition()
	{
		return _ovrHand.transform.localPosition;
	}

	public bool IsPinching(OVRHand.HandFinger finger)
	{
		return _ovrHand.GetFingerIsPinching(finger);
	}

	public void CheckGrasping()
	{
		if (_ovrHand.HandConfidence == OVRHand.TrackingConfidence.Low) return;

		bool wasGrasping = IsGrasping;

		//bool indexFlexed = CheckInRange(IndexProximal.localRotation.eulerAngles.z, 200, 320);
		bool middleFlexed = CheckInRange(MiddleProximal.localRotation.eulerAngles.z, 200, 310);
		bool ringFlexed = CheckInRange(RingProximal.localRotation.eulerAngles.z, 200, 310);
		bool pinkyFlexed = CheckInRange(PinkyProximal.localRotation.eulerAngles.z, 200, 310);

		IsGrasping = /*indexFlexed &&*/ middleFlexed && ringFlexed && pinkyFlexed;

		if (IsGrasping != wasGrasping)
		{
			if (IsGrasping)
			{
				_onStartGrasping.OnNext(this);
			}
			else
			{
				_onStopGrasping.OnNext(this);
			}
		}
	}

	private bool CheckInRange(float value, float min, float max)
	{
		return value > min && value < max;
	}

	public IObservable<bool> CreateOnPinch(OVRHand.HandFinger finger)
	{
		var lastPinchStream =
			Observable.EveryUpdate().Select(_ => IsPinching(finger));

		var currentPinchStream = Observable.EveryUpdate()
			.Select(_ => IsPinching(finger)).Skip(1);

		return Observable.Zip(lastPinchStream, currentPinchStream)
			.Where(p => p[0] != p[1])
			.Select(p => !p[0] && p[1]);
	}

	public IObservable<Unit> CreateOnDoublePinch(OVRHand.HandFinger finger)
	{
		var lastPinchStream =
			Observable.EveryUpdate().Select(_ => IsPinching(finger));

		var currentPinchStream = Observable.EveryUpdate()
			.Select(_ => IsPinching(finger)).Skip(1);

		var pinchStartedStream = Observable.Zip(currentPinchStream, lastPinchStream).Where(p => p[0] && !p[1]);

		return pinchStartedStream.Buffer(TimeSpan.FromSeconds(1), 2).Where(pinches => pinches.Count >= 2).Select(x => Unit.Default);
	}


}
