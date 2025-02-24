﻿using System.Collections.Generic;
using UnityEngine;

namespace MessengerRando.Overrides
{
	/// <summary>
	/// This class is meant to override functions of CatacombLevelInitializer for ease of access.
	/// </summary>
	static class RandoCatacombLevelInitializer
    {
		/// <summary>
		/// Overriding FixPlayerStuckInChallengeRoom method that is private in orig code. No changes made to orig code.
		/// </summary>
		public static void FixPlayerStuckInChallengeRoom()
		{
			if (Manager<Level>.Instance.CurrentRoom.roomKey == "492524-44-28" &&
			    Manager<Level>.Instance.LevelRooms.TryGetValue("492524-60-44", out var room))
			{
				List<GameObject> roomObjects = room.roomObjects;
				for (int i = roomObjects.Count - 1; i >= 0; i--)
				{
					BreakableCollision component = roomObjects[i].GetComponent<BreakableCollision>();
					if (component != null)
					{
						component.BreakInstant();
					}
				}
			}
		}
	}
}
