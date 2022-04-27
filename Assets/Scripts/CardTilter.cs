using System;
using UnityEngine;

namespace Shared.UI.Helpers
{
	public class CardTilter : MonoBehaviour
	{
		// How much to scale the card movement by when tilting.
		[SerializeField] private float _dragTiltForceMultiplier = 3;
		// Maximum pitch/roll angle when dragging.
		[SerializeField] private float _maxDragTilt = 10;
		// Time to go back to resting.
		[SerializeField] private float _dragTiltRestTime = 0.15f;

		// Base rotation to apply deltas to.
		[HideInInspector] public Vector3 BaseRotation = Vector3.zero;

		private float _targetPitchDelta;
		private float _targetRollDelta;
		private float _pitchVelocity;
		private float _rollVelocity;
		private Vector3 _prevPos;
		bool rotationIsSetToZero = false;

		// Whether the titler is currently tilting by a significant amount.
		public bool IsStable
		{
			get { return Mathf.Abs(_targetPitchDelta) < 1 && Mathf.Abs(_targetRollDelta) < 1; }
		}

		private void OnEnable()
		{
			_prevPos = transform.position;
			_targetPitchDelta = 0;
			_targetRollDelta = 0;
			Update();
		}

		private void Update()
		{
			var pos = transform.position;
			var posDelta = pos - _prevPos;

			// Pitch based on how fast the card is moving, scaled by the force multiplier.
			_targetPitchDelta += posDelta.z * _dragTiltForceMultiplier;
			_targetPitchDelta = Mathf.Clamp(_targetPitchDelta, -_maxDragTilt, _maxDragTilt);

			// Roll based on how fast the card is moving, scaled by the force multiplier.
			_targetRollDelta -= posDelta.x * _dragTiltForceMultiplier;
			_targetRollDelta = Mathf.Clamp(_targetRollDelta, -_maxDragTilt, _maxDragTilt);

			// Return back to rest.
			_targetPitchDelta = Mathf.SmoothDamp(_targetPitchDelta, 0f, ref _pitchVelocity, _dragTiltRestTime);
			_targetRollDelta = Mathf.SmoothDamp(_targetRollDelta, 0f, ref _rollVelocity, _dragTiltRestTime);

			var newRot = BaseRotation + new Vector3(_targetPitchDelta, transform.eulerAngles.y, _targetRollDelta);
			// Use transform's current z rot if the base rotation is default.
			if (Math.Abs(newRot.z) < Mathf.Epsilon)
			{
				newRot.z = transform.localEulerAngles.z;
			}
			if (!rotationIsSetToZero)
			{
				transform.localEulerAngles = newRot; 
			}
			_prevPos = pos;
		}
		public void SetRotationToZero()
        {
			rotationIsSetToZero = true;
        }
		public void SetRotationToNotZero()
		{
			rotationIsSetToZero = false;
		}
	}
}

