using AmplifyImpostors;
using UnityEngine;

namespace DrivableRock.Components
{
	internal class Drivable : MonoBehaviour
	{
		private Rigidbody _rb;
		private bool _isSeated = false;
        private bool _isInitialised = false;

        private Input _input;

        private float _defaultMaxVelocity = 600f;
		private float _maxVelocity = 600f;
		private float _rotationSpeed = 50f;
        private int _appliedMultiplier = 1;

		private float _speed = 0f;

		public void Start()
		{
			_maxVelocity *= DrivableRock.powerMultiplier;
            _appliedMultiplier = DrivableRock.powerMultiplier;
		}

		public void Update()
		{
			// Check if player is sitting on rock.
			// seatscript.inUse always returns false so this is a hacky workaround.
			if (mainscript.M.player.seat != null && mainscript.M.player.seat.transform.parent.gameObject == gameObject)
				_isSeated = true;
			else
				_isSeated = false;

            // Check for power multiplier changes.
            if (_appliedMultiplier != DrivableRock.powerMultiplier)
            {
                _maxVelocity = _defaultMaxVelocity * DrivableRock.powerMultiplier;
                _appliedMultiplier = DrivableRock.powerMultiplier;
            }

            // Rock isn't sat on, return early.
            if (!_isSeated) return;

            if (!_isInitialised)
                InitialiseSeating();

			UpdateInput();
		}

        private void InitialiseSeating()
        {
            // Create the rigidbody upon sitting on the rock so they sit in the ground like normal.
            if (_rb == null)
            {
                _rb = gameObject.AddComponent<Rigidbody>();
                _rb.isKinematic = false;
                _rb.useGravity = true;
                _rb.constraints = RigidbodyConstraints.FreezeRotation;
                _rb.mass = 3000f;
            }

            // Remove rock from object generation chunk to prevent despawning.
            temporaryTurnOffGeneration temp = mainscript.M.menu.GetComponentInChildren<temporaryTurnOffGeneration>();
            bool foundObj = false;
            if (temp != null && temp.Objects != null)
            {
                foreach (var chunk in temp.Objects.OutputChunks)
                {
                    if (foundObj) break;

                    foreach (var obj in chunk.placedObjs)
                    {
                        // Found object, remove it from chunk and break.
                        if (obj == gameObject)
                        {
                            chunk.placedObjs.Remove(obj);
                            foundObj = true;
                            break;
                        }
                    }
                }
            }

            _isInitialised = true;
        }

		public void FixedUpdate()
		{
			// Rock isn't sat on, return early.
			if (!_isSeated) return;

			float input;
			if (_input.Throttle > 0)
			{
				// Forward speed.
				input = _input.Throttle * 100f;
			}
			else if (_input.Brake > 0)
			{
				// Brake force.
				input = -_input.Brake * 200f;
			}
			else
			{
				// Coasting slowdown force.
				input = -75f;
			}

            input *= DrivableRock.powerMultiplier;

			_speed = Mathf.Clamp(_speed + input * Time.fixedDeltaTime, 0, _maxVelocity);

            Vector3 velocity = transform.forward * _speed * Time.fixedDeltaTime;
            velocity.y = _rb.velocity.y;
            _rb.velocity = velocity;
            transform.Rotate(transform.up * _input.Steering * _rotationSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Get user input values.
        /// </summary>
        private void UpdateInput()
		{
            _input = new Input()
            {
                Throttle = inputscript.i.throttle,
                Brake = inputscript.i.brake,
                Steering = Mathf.Clamp(inputscript.i.steer, -1f, 1f),
                Jump = inputscript.i.jump,
            };
		}

        private class Input
        {
            public float Throttle { get; set; }
            public float Brake { get; set; }
            public float Steering { get; set; }
            public bool Jump { get; set; }
        }
	}
}
