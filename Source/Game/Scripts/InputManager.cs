using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class InputManager : Script
    {
        CameraManager camera;

        public override void OnStart()
        {
            camera = Actor.GetScript<CameraManager>();
        }
        public override void OnFixedUpdate()
        {
            OnMovementInput();
            if (Input.MouseScrollDelta != 0)
            {
                camera.Zoom(Input.MouseScrollDelta);
            }
        }

        private void OnMovementInput()
        {
            Vector2 direction = Vector2.UnitX * Input.GetAxis("Horizontal") +
                                    Vector2.UnitY * Input.GetAxis("Vertical");

            if (direction != Vector2.Zero)
            {
                NetworkMessage msg = new NetworkMessage(MsgType.Movement);
                msg.Write(direction);
                msg.Send();
            }
        }
    }
}
