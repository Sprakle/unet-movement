using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Movement
{
	public class MovementClientPrediction : NetworkBehaviour
	{
		[SerializeField] private float speed;
		[SerializeField] private float movementDelta = 1f/20f;
		[SerializeField] private MovementInputHistory history;

		[SerializeField] private float smoothingSpeed;
		[SerializeField] private bool smoothCorrections;

		private MovementInput latestAcknowledgedInput;
		private Vector3 latestSentInput;
		private Vector2 correction;

		private Vector2 cumulativeInput;

		private void Awake()
		{
			latestAcknowledgedInput = new MovementInput {id = -1};
		}

		private Vector2 debugServerPos;

		public void ReceiveUpdate(Vector2 position, int id)
		{
			if (id != latestAcknowledgedInput.id + 1)
				Debug.LogWarning("Packet received out of order! Expecting " + (latestAcknowledgedInput.id + 1) + ", got " + id);

			// make sure we didn't get an update out of order
			if (latestAcknowledgedInput.id >= id)
				return;

			Debug.DrawLine(debugServerPos, position, Color.green, 5);
			debugServerPos = position;

			latestAcknowledgedInput = history.GetMove(id);
			
			// replay all inputs that occured after this state update
			MovementInput[] inputs = history.GetToLatest(id);
			MovementInput previous = history.GetMove(id - 1);
			for (int i = 0; i < inputs.Length; i++)
			{
				//float deltaTime = inputs[i].time - previous.time;
				float deltaTime = movementDelta;
				position += inputs[i].move*speed*deltaTime;
				previous = inputs[i];
			}

			// add the input that hasen't been sent to the server at all yet
			// disabled cuz it doesn't really seem to do anything, but maybe this is a good idea for later
			//position += cumulativeInput*speed*(Time.time - previous.time);
			
			if (smoothCorrections)
			{
				correction = position - (Vector2) transform.position;
				Debug.DrawRay(transform.position, correction, Color.red, 5);
			}
			else
				transform.position = position;
		}

		private void FixedUpdate()
		{
			if (!hasAuthority)
				return;

			if (!localPlayerAuthority)
				return;

			if (NetworkServer.active)
				return;
			
			UpdatePosition();
		}
		
		private void UpdatePosition()
		{
			if (smoothCorrections)
			{
				Vector2 oldCorrection = correction;
				correction = Vector2.Lerp(correction, Vector2.zero, smoothingSpeed*Time.deltaTime);
				transform.position += (Vector3) (oldCorrection - correction);

				Debug.DrawRay(transform.position, correction, Color.magenta);
			}
			
            transform.position += latestSentInput * Time.fixedDeltaTime * speed;
		}

		private float moveTime;

		private float lastSendTime;

		public void Predict(Vector2 input)
		{
			latestSentInput = Vector2.ClampMagnitude(input, 1);
			
			lastSendTime = Time.deltaTime;
		}

		public void UpdateCumulative(Vector2 cumulative)
		{
			cumulativeInput = cumulative;
		}
	}
}
