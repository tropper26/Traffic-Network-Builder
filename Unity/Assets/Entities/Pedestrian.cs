﻿using Assets.Enums;

namespace Entity
{
    public class Pedestrian : BaseEntity
    /// <summary>Creates Pedestrian Object. Contains all Pedestiran-Entity specific info created by Gui-User.</summary>
    {
        private static int autoIncrementId = 0;

        public Pedestrian(Location spawnPoint, Path path, PedestrianType pedestrianType = PedestrianType.Null, double initialSpeed = 0) : base(string.Format("{0} {1}", "Vehicle", ++autoIncrementId), spawnPoint, initialSpeed)
        {
            Path = path;
            Type = pedestrianType;
        }

        public Pedestrian(Location spawnPoint, EntityModel model, Path path, PedestrianType pedestrianType = PedestrianType.Null, double initialSpeed = 0) : base(string.Format("{0} {1}", "Vehicle", ++autoIncrementId), spawnPoint, initialSpeed)
        {
            Model = model;
            Path = path;
            Type = pedestrianType;
        }

        public EntityModel Model { get; set; }
        public PedestrianType Type { get; set; }
        public Path Path { get; set; }
    }
}