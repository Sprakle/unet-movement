using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Movement
{
	public class MovementInputServer : NetworkBehaviour
	{
		internal const short MSG_ID = MsgType.Highest + 2;
		
		[SerializeField] private float speed = 1;
		[SerializeField] private float movementDelta = 1f / 20f;
		[SerializeField] private MovementUpdate updater;

		public int LatestInputID { get; private set; }

		private Vector3 currentInput;

		private void Awake()
		{
			LatestInputID = -1;
		}

		public static void HandleInput(NetworkMessage netMsg)
		{
			NetworkInstanceId netId = netMsg.reader.ReadNetworkId();

			GameObject foundObj = NetworkServer.FindLocalObject(netId);
			if (foundObj == null)
				throw new Exception("HandleTransform no gameObject");

			MovementInputServer foundSync = foundObj.GetComponent<MovementInputServer>();
			if (foundSync == null)
				throw new Exception("HandleTransform null target");

			if (!foundSync.localPlayerAuthority)
				throw new Exception("HandleTransform no localPlayerAuthority");

			if (netMsg.conn.clientOwnedObjects == null)
				throw new Exception("HandleTransform object not owned by connection");

			if (!netMsg.conn.clientOwnedObjects.Contains(netId))
				throw new Exception("HandleTransform netId:" + netId + " is not for a valid player");

			foundSync.UnserializeInput(netMsg.reader);
		}

		private void UnserializeInput(NetworkReader reader)
		{
			Vector2 input = reader.ReadVector2();
			int id = reader.ReadInt32();
			
			if (id != LatestInputID + 1)
				Debug.LogWarning("Packet received out of order! Expecting " + (LatestInputID + 1) + ", got " + id);

			// make sure we didn't get an input out of order
			if (LatestInputID >= id)
				return;

			LatestInputID = id;
			
			// no cheating :v
			currentInput = Vector2.ClampMagnitude(input, 1);
			
			updater.SendUpdate();

			transform.position += currentInput*speed*movementDelta;
		}
	}
}
