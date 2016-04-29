using Assets.Scripts.Movement;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts
{
	public class HandlerRegistration : MonoBehaviour
	{
		private void Awake()
		{
			if (NetworkServer.active)
				Register();
		}

		private void Register()
		{
			NetworkServer.RegisterHandler(MovementInputServer.MSG_ID, MovementInputServer.HandleInput);
		}
	}
}
