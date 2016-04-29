using System;
using UnityEngine;

namespace Assets.Scripts.Movement
{
	public class MovementInputHistory : MonoBehaviour
	{
		[SerializeField] private int historySize;

		// rolling history of past input commands
		private MovementInput[] history;
		private int currentID;

		private void Awake()
		{
			history = new MovementInput[historySize];
		}

		public int Move(Vector2 move)
		{
			MovementInput input = new MovementInput
			{
				move = move,
				id = currentID,
				time = Time.time
			};

			int index = currentID%historySize;
			history[index] = input;

			currentID++;

			return input.id;
		}

		public MovementInput GetMove(int id)
		{
			int index = id%historySize;
			MovementInput result = history[index];

			if (result.id != id)
				Debug.LogWarning("Looking for input history with ID " + id + ", found " + result.id + ". You may need to increase the history size.");

			return result;
		}

		public MovementInput[] GetToLatest(int startID)
		{
			int length = currentID - startID;
			MovementInput[] result = new MovementInput[length];

			for (int i = 0; i < length; i++)
				result[i] = history[(startID + i)%historySize];
			
			if (result.Length > 0)
			{
				if (result[0].id != startID)
					throw new Exception("Bad code! Exprected " + startID + " got " + result[0].id);

				if (result[result.Length - 1].id != currentID - 1)
					throw new Exception("Bad code! Exprected " + (currentID - 1) + " got " + result[result.Length - 1].id);
			}

			return result;
		}
	}

	public struct MovementInput
	{
		public Vector2 move;
		public int id;
		public float time;
	}
}
