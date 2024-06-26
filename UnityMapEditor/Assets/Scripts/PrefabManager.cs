using Assets.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace scripts
{
    /// <summary>
    /// This class handles the prefabs that are spawned on the map, once the user selects a road piece from the bottom bar
    /// </summary>
    public class PrefabManager : MonoBehaviour
    {
        private static PrefabManager instance;
        public static PrefabManager Instance
        {
            get
            {
                return instance;
            }
        }

        public RoadPiece Straight;
        public RoadPiece Turn;
        public RoadPiece Turn15;
        public RoadPiece ThreeWayIntersection;
        public RoadPiece FourWayIntersection;
        public RoadPiece ParkingBottom;
        public RoadPiece ParkingTopAndBottom;
        public RoadPiece ParkingTop;
        public RoadPiece Crosswalk;
        public RoadPiece FourWayRoundAbout;
        public RoadPiece ThreeWayRoundAbout;
        public RoadPiece ThreeWayRoundAboutAdvanced;
        public RoadPiece FourWayRoundAboutAdvanced;
        public RoadPiece StraightShort;
        public RoadPiece None;

        /// <summary>
        /// This method is called before the first frame update and will initialize the prefab manager
        /// </summary>
        void Start()
        {
            instance = this;
        }

        /// <summary>
        /// This method will return the road piece from the prefab based on the road type
        /// </summary>
        /// <param name="roadType"> The roadtype that the prefab has and on which the road is genereated from </param>
        /// <returns> The road piece from the corresponding type </returns>
        public RoadPiece GetPieceOfType(RoadType roadType)
        {
            switch (roadType)
            {
                case RoadType.StraightRoad:
                    return Straight;
                case RoadType.Turn:
                    return Turn;
                case RoadType.Turn15:
                    return Turn15;
                case RoadType.ThreeWayIntersection:
                    return ThreeWayIntersection;
                case RoadType.FourWayIntersection:
                    return FourWayIntersection;
                case RoadType.ParkingBottom:
                    return ParkingBottom;
                case RoadType.ParkingTop:
                    return ParkingTop;
                case RoadType.ParkingTopAndBottom:
                    return ParkingTopAndBottom;
                case RoadType.Crosswalk:
                    return Crosswalk;
                case RoadType.FourWayRoundAbout:
                    return FourWayRoundAbout;
                case RoadType.ThreeWayRoundAbout:
                    return ThreeWayRoundAbout;
                case RoadType.ThreeWayRoundAboutAdvanced:
                    return ThreeWayRoundAboutAdvanced;
                case RoadType.FourWayRoundAboutAdvanced:
                    return FourWayRoundAboutAdvanced;
                case RoadType.None:
                    return None;
                default:
                    return null;
            }
        }
    }
}
