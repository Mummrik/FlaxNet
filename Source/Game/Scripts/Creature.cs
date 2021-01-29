using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class Creature : Script
    {
        public Guid id;
        public Vector3 position;
        public Quaternion rotation;
        public float moveSpeed;

        public SkinnedModel Model;
        public AnimationGraph AnimGraph;
        public AnimatedModel AnimModel;

        private AnimGraphParameter AnimationBlend;
        private Vector3 XNegative = new Vector3(-1, 1, 1);  // Temporary fix for rotation

        public override void OnStart()
        {
            AnimModel.SkinnedModel = Model;
            AnimModel.AnimationGraph = AnimGraph;
            AnimationBlend = AnimModel.GetParameter("Alpha");
        }

        public override void OnUpdate()
        {
            OnMove();
        }

        private void OnMove()
        {
            if (Vector3.NearEqual(Actor.Position, position, 0.5f))
            {
                if (Actor.Position != position)
                    Actor.Position = position;

                return;
            }

            Vector3 lastFramePosition = Actor.Position;
            Actor.Position = Vector3.Lerp(Actor.Position, position, moveSpeed * Time.DeltaTime);
            // Temporary fix for rotation
            Actor.Rotation = Matrix.RotationQuaternion(Quaternion.LookAt(Actor.Position * XNegative, position * XNegative, Vector3.Up));
            //Actor.Rotation = Matrix.RotationQuaternion(Quaternion.LookAt(Actor.Position, position, Vector3.Up));

            AnimationBlend.Value = Mathf.Clamp(Vector3.Distance(lastFramePosition, Actor.Position), 0, 1);
        }
    }
}
