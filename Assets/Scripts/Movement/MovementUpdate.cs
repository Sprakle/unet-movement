using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Movement
{
	public class MovementUpdate : NetworkBehaviour
	{
		[SerializeField] private MovementInputServer input;
		[SerializeField] private MovementClientPrediction clientPrediction;
		
		private Vector3 lastPosDebug;

		private void Start()
		{
			if (isClient)
				NetworkManager.singleton.client.RegisterHandler(MovementInputServer.MSG_ID, HandleMessage);
		}

		private NetworkWriter writer;

		public void SendUpdate()
		{
			if (writer == null)
				writer = new NetworkWriter();
			
			writer.StartMessage(MovementInputServer.MSG_ID);
			writer.Write((Vector2)transform.position);
			writer.Write(input.LatestInputID);
			writer.FinishMessage();

			NetworkServer.SendWriterToReady(gameObject, writer, 0);
		}

		private void HandleMessage(NetworkMessage message)
		{
			NetworkReader reader = message.reader;

			Vector2 position = reader.ReadVector2();
			int id = reader.ReadInt32();

			clientPrediction.ReceiveUpdate(position, id);

			Debug.DrawLine(lastPosDebug, transform.position, Color.white, 5);
			Debug.DrawLine(transform.position, transform.position + Vector3.up * 0.1f, Color.black, 5);
			lastPosDebug = transform.position;
		}
	}
}
