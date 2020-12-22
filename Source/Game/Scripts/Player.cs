using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class Player : Script
    {
        public Actor Model;
        public Vector3 position;
        public float moveSpeed;
        public override void OnStart()
        {
            Model = Actor.FindActor("Model");
        }

        public override void OnUpdate()
        {
            // Here you can add code that needs to be called every frame
            Actor.Position = Vector3.Lerp(Actor.Position, position, moveSpeed * Time.DeltaTime);
        }
    }
}
