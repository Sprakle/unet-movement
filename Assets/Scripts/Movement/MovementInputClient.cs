using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Movement
{
	public class MovementInputClient : NetworkBehaviour
	{
		[SerializeField] private float sendInterval = 1f/20f;

		[SerializeField] private MovementInputHistory history;
		[SerializeField] private MovementClientPrediction prediction;

		public float sendDelay;

		private float lastClientSendTime;
		private Vector2 lastInput;
		private Vector2 cumulativeInput;

		private NetworkWriter writer;

		private void Awake()
		{
			if (localPlayerAuthority)
				writer = new NetworkWriter();
		}

		private void Update()
		{
			if (!hasAuthority)
				return;

			if (!localPlayerAuthority)
				return;

			if (NetworkServer.active)
				return;

			CollectInput();

			if (Time.time - lastClientSendTime > sendInterval)
			{
				SendInput();
				lastClientSendTime = Time.time;
			}
		}

		public override float GetNetworkSendInterval()
		{
			return sendInterval;
		}

		private void CollectInput()
		{
			Vector2 input = new Vector2
			{
				x = Input.GetAxis("Horizontal"),
				y = Input.GetAxis("Vertical")
			};

			input = Vector2.ClampMagnitude(input, 1);

			cumulativeInput += (input*Time.deltaTime)/sendInterval;
			prediction.UpdateCumulative(cumulativeInput);
		}

		private void SendInput()
		{
			cumulativeInput = Vector2.ClampMagnitude(cumulativeInput, 1);
			
			int id = history.Move(cumulativeInput);
			
			StartCoroutine(SendDelay(id, cumulativeInput));
			
			lastInput = cumulativeInput;
			cumulativeInput = Vector2.zero;

			prediction.Predict(lastInput);
			prediction.UpdateCumulative(cumulativeInput);
		}

		private IEnumerator SendDelay(int id, Vector2 input)
		{
			yield return new WaitForSeconds(sendDelay);
			
			writer.StartMessage(MovementInputServer.MSG_ID);
			writer.Write(netId);
			writer.Write(input);
			writer.Write(id);
			writer.FinishMessage();

			ClientScene.readyConnection.SendWriter(writer, Channels.DefaultUnreliable);
		}
	}
}
