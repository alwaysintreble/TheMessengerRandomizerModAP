using System;
using System.Collections.Generic;
using System.Linq;
using MessengerRando.Exceptions;
using UnityEngine;

namespace MessengerRando.Utils.Constants
{
    public class RoomConstants
    {
        public struct RoomTransition: IEquatable<RoomTransition>
        {
            private static readonly List<string> ValidDirections = new List<string> { "Up", "Left", "Down", "Right" };
            public string Direction;
            public Vector3 Position;
            public EBits Dimension;

            public RoomTransition(string direction, Vector3 position)
            {
                if (!ValidDirections.Contains(direction))
                    throw new RandomizerException($"{direction} is an invalid direction for a RoomTransition!");
                Direction = direction;
                Position = position;
                Dimension = EBits.NONE;
            }

            public RoomTransition(string direction, Vector3 position, EBits dimension)
            {
                if (!ValidDirections.Contains(direction))
                    throw new RandomizerException($"{direction} is invalid direction!");
                Direction = direction;
                Position = position;
                Dimension = dimension;
            }
            
            public bool Equals(RoomTransition other)
            {
                return Direction == other.Direction;
            }

            public override bool Equals(object obj)
            {
                return obj is RoomTransition other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Direction != null ? Direction.GetHashCode() : 0) * 397) ^ Position.GetHashCode();
                }
            }
        }

        public struct RandoRoom: IEquatable<RandoRoom>
        {
            public readonly ELevel Region;
            public readonly string RoomKey;

            public RandoRoom(string key, ELevel area)
            {
                RoomKey = key;
                Region = area;
            }

            public RandoRoom(string key)
            {
                RoomKey = key;
                Region = ELevel.NONE;
            }
            
            public bool Equals(RandoRoom other)
            {
                return RoomKey == other.RoomKey && Region == other.Region;
            }

            public override bool Equals(object obj)
            {
                return obj is RandoRoom other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (RoomKey != null ? RoomKey.GetHashCode() : 0);
            }
        }
        
        public static readonly Dictionary<string, RandoRoom> RoomLookup = new Dictionary<string, RandoRoom>
        {
            {"room name", new RandoRoom("roomKey") },
        };

        public static readonly Dictionary<RandoRoom, string> RoomNameLookup =
            RoomLookup.ToDictionary(x => x.Value, x => x.Key);


        public static readonly Dictionary<string, List<RoomTransition>> TransitionLookup =
            new Dictionary<string, List<RoomTransition>>
            {
                { 
                    "room name", new List<RoomTransition>
                    { 
                        new RoomTransition("Up", new Vector3()), 
                        new RoomTransition("Left", new Vector3()), 
                        new RoomTransition("Down", new Vector3()), 
                        new RoomTransition("Right", new Vector3())
                    }
                },
                {
                    "room name", new List<RoomTransition>
                    {
                        new RoomTransition("Up", new Vector3()),
                        new RoomTransition("Down", new Vector3()),
                        new RoomTransition("Left", new Vector3()),
                        new RoomTransition("Right", new Vector3())
                    }
                },
                
            };

    }
}