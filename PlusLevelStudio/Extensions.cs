using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio
{
    public static class EditorExtensions
    {

        /// <summary>
        /// Converts a given position to an int vector corresponding to a cell position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static IntVector2 ToCellVector(this Vector3 position)
        {
            float x = (position.x - 5f) / 10f;
            float y = (position.z - 5f) / 10f;
            return new IntVector2(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        }

        public static IntVector2 ToMystVector(this Vector2Int me)
        {
            return new IntVector2(me.x, me.y);
        }

        /// <summary>
        /// Converts a given IntVector to a world position
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 ToWorld(this IntVector2 vector)
        {
            return new Vector3((vector.x * 10f + 5f), 0f, (vector.z * 10f + 5f));
        }

        /// <summary>
        /// From the given direction and amount, calculate the size and position difference required to keep the origin in the top left.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="dif"></param>
        /// <param name="sizeDif"></param>
        /// <param name="posDif"></param>
        public static void CalculateDifferencesForHandleDrag(Direction direction, int dif, out IntVector2 sizeDif, out IntVector2 posDif)
        {
            IntVector2 difference = direction.ToIntVector2() * dif;
            sizeDif = new IntVector2(0, 0);
            posDif = new IntVector2(0, 0);
            switch (direction) // handle the movement of each arrow individually
            {
                case Direction.North:
                    sizeDif = new IntVector2(0, difference.z);
                    break;
                case Direction.South:
                    sizeDif = new IntVector2(0, -difference.z);
                    posDif = new IntVector2(0, difference.z);
                    break;
                case Direction.East:
                    sizeDif = new IntVector2(difference.x, 0);
                    break;
                case Direction.West:
                    sizeDif = new IntVector2(-difference.x, 0);
                    posDif = new IntVector2(difference.x, 0);
                    break;
            }
        }

        public static int DistanceInDirection(this IntVector2 me, Direction dir)
        {
            return me.RawDistanceVector(dir).GetValueForDirection(dir);
        }

        /// <summary>
        /// An IntVector2 where both numbers are always positive.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static IntVector2 RawDistanceVector(this IntVector2 me, Direction dir)
        {
            return (me.Scale(dir.ToIntVector2()));
        }

        // conversion helpers

        public static Vector3 ToAxisMultipliers(this Direction me)
        {
            Vector3 resultVector = me.ToVector3();
            return new Vector3(Mathf.Abs(resultVector.x), Mathf.Abs(resultVector.y), Mathf.Abs(resultVector.z));
        }

        public static IntVector2 LockAxis(this IntVector2 me, IntVector2 origin, Direction toLock)
        {
            Vector3 locked = toLock.ToAxisMultipliers();
            return new IntVector2(locked.x == 1f ? me.x : origin.x, locked.z == 1f ? me.z : origin.z);
        }

        public static int GetValueForDirection(this IntVector2 me, Direction d)
        {
            return (d == Direction.North || d == Direction.South) ? me.z : me.x;
        }

        public static IntVector2 EraseAxis(this IntVector2 me, Direction toPreserve)
        {
            return LockAxis(me, new IntVector2(), toPreserve);
        }

        public static IntVector2 Max(this IntVector2 me, IntVector2 max)
        {
            return new IntVector2(Mathf.Max(me.x, max.x), Mathf.Max(me.z, max.z));
        }
    }
}
