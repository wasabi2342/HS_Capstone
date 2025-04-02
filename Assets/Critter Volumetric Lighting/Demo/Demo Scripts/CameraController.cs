using System.Collections;
using UnityEngine;

/**
Crude camera controller for demonstrating how volumetric light behaves with camera movement and rotations.
*/
namespace CritterVolumetricLighting
{		
	public class CameraController : MonoBehaviour
	{
		public Transform target;
		public float rotationSpeed = 125f;
		public float rotationAngle = 90f;
		public float movementSpeed = 10f;
		
		
		private Coroutine _currentRotationCoroutine = null;
		
		private float _movementMultiplier = 1f;
		private float _lastAngle;
		private float _lastStationaryAngle;
		private float _newTargetAngle;
		private float _cooldownTimer = 0;
		private float _cdTimerQ = 0;
		private float _cdTimerE = 0;
		private float _startDistance;
		
		private int _movingClockWise; // -1 = not moving, 0 = false, 1 = true
		private int _que = 0;
		

		void Start()
		{
			Vector3 delta = this.transform.position - target.position;
			_startDistance = delta.magnitude;
			
			_movingClockWise = -1;
			_lastStationaryAngle = this.transform.eulerAngles.y;
			

			// StartCoroutine(TempCoroutine());

		}
		
		
		IEnumerator TempCoroutine()
		{
			yield return new WaitForSeconds(1f);
				_cdTimerQ = 0;
				float moveAngle;
				if (_movingClockWise == 0)
				{
					_que = 0;
					moveAngle = 0;
				} else if (_movingClockWise == 1)
				{
					if (_que >= 1)
					{
					} else 
					{
						_que++;
					}
				} else // -1... 
				{
					moveAngle = rotationAngle;
				}
				
				_movingClockWise = 1;
				StartRotation(rotationAngle);
				yield return null;
		}
		

		void Update()
		{

			if (_cooldownTimer < 0.1f)
			{
				_cooldownTimer += Time.deltaTime;
				return;
			}
			
			_lastAngle = this.transform.eulerAngles.y;
			
			if (_cdTimerQ < 0.15f)
			{
				_cdTimerQ += Time.deltaTime;
			} else 
			{
				HandleQ();
			}
			
			if (_cdTimerE < 0.15f)
			{
				_cdTimerE += Time.deltaTime;
			} else 
			{
				HandleE();
			}
			
			this.transform.position = target.position - _startDistance * this.transform.forward;
			HandleMovement(target);
		}
		
		private void HandleQ()
		{
			if (Input.GetKeyDown(KeyCode.Q))
			{
				_cdTimerQ = 0;
				float moveAngle;
				if (_movingClockWise == 0)
				{
					_que = 0;
					moveAngle = 0;
				} else if (_movingClockWise == 1)
				{
					if (_que >= 1)
					{
						return; // We're already going in that direction
					} else 
					{
						_que++;
						return;
					}
				} else // -1... 
				{
					moveAngle = rotationAngle;
				}
				
				_movingClockWise = 1;
				StartRotation(moveAngle);
				return;
			}
		}
		
		private void HandleE()
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				_cdTimerE = 0;
				float moveAngle;
				if (_movingClockWise == 0)
				{
					if (_que <= -1)
					{
						return; // We're already going in that direction
					} else 
					{
						_que--;
						return;
					}
				} else if (_movingClockWise == 1)
				{
					_que = 0;
					moveAngle = 0;
				} else // -1... 
				{
					moveAngle = -rotationAngle;
				}
				
				_movingClockWise = 0;
				StartRotation(moveAngle);
			}
		}
		
		void HandleMovement(Transform mover)
		{
			Vector3 forwardDirection = this.transform.forward;
			Vector3 rightDirection = this.transform.right;

			forwardDirection.y = 0f; // Remove vertical component
			rightDirection.y = 0f; // Remove vertical component
			forwardDirection.Normalize();
			rightDirection.Normalize();

			// Movement based on input
			Vector3 movement = Vector3.zero;
			if (Input.GetKey(KeyCode.W))
			{
				movement += forwardDirection;
			}
			if (Input.GetKey(KeyCode.S))
			{
				movement -= forwardDirection;
			}
			if (Input.GetKey(KeyCode.A))
			{
				movement -= rightDirection;
			}
			if (Input.GetKey(KeyCode.D))
			{
				movement += rightDirection;
			}

			// Normalize the movement vector to prevent faster diagonal movement
			if (movement.magnitude > 1f)
			{
				movement.Normalize();
			}

			if (Input.GetKey(KeyCode.LeftShift))
			{
				_movementMultiplier = 3f;
			} else 
			{
				_movementMultiplier = 1f;
			}
			
			mover.Translate(movement * movementSpeed * Time.deltaTime * _movementMultiplier);
		}

		void StartRotation(float angle)
		{
			_cooldownTimer = 0;
			if (_currentRotationCoroutine != null)
			{
				StopCoroutine(_currentRotationCoroutine);
			}
			

			
			_newTargetAngle = _lastStationaryAngle + angle;
			_currentRotationCoroutine = StartCoroutine(RotateAroundTarget(_newTargetAngle));
		}
			
		IEnumerator RotateAroundTarget(float targetAngle, bool cameFromCoroutine = false)
		{
			// Debug.Log("TargetAngle: "+targetAngle+"		lastAngle: "+lastAngle);
			float angle = Mathf.DeltaAngle(_lastAngle, targetAngle);

			Vector3 rotationAxis = target.up;

			float totalRotationTime = Mathf.Abs(angle) / rotationSpeed; // Total time to complete the rotation
			// totalRotationTime = 10000f;
			float elapsedTime = 0f;

			float initialRotation = transform.eulerAngles.y;

			while (elapsedTime < totalRotationTime)
			{
				elapsedTime += Time.deltaTime;
				float t = elapsedTime / totalRotationTime; // calculate current progress
				
				float usedT;
				if (_que == 0)
				{
					usedT = SmoothStep(t, cameFromCoroutine); // apply easing
				} else 
				{
					usedT = SmoothStep(t, cameFromCoroutine);  // Don't apply easing when we are in que.
				}

				Vector3 rotationCenter = target.position; // update rotation center each frame

				float targetRotation = initialRotation + usedT * angle; // target rotation after applying easing
				float rotationThisFrame = targetRotation - transform.eulerAngles.y; // rotation to be done this frame

				transform.RotateAround(rotationCenter, rotationAxis, rotationThisFrame);

				yield return null;
			}
			
			_lastStationaryAngle = this.transform.eulerAngles.y;

			if (_que != 0)
			{
				_que = 0;
				_newTargetAngle = _lastStationaryAngle + angle;
				StopCoroutine(_currentRotationCoroutine);
				_currentRotationCoroutine = StartCoroutine(RotateAroundTarget(_newTargetAngle, true));
				yield return null;
			}

			// apply a final rotation to ensure we exactly hit the target angle
			Vector3 finalRotationCenter = target.position;
			float finalRotation = targetAngle - transform.eulerAngles.y;
			transform.RotateAround(finalRotationCenter, rotationAxis, finalRotation);
			
			_movingClockWise = -1;
			_lastStationaryAngle = this.transform.eulerAngles.y;
			_currentRotationCoroutine = null;
		}



		
		float SmoothStep(float hardT, bool cameFromCoroutine)
		{
			return SmoothStepLogic(hardT);
		}
		
		float SmoothStepLogic(float steppedValue)
		{
			return steppedValue * steppedValue * (3f - 2f* steppedValue);
		}
	}
}