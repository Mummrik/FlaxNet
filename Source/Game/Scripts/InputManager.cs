using System;
using System.Collections.Generic;
using System.Net.Sockets;
using FlaxEngine;

namespace Game
{
    public class InputManager : Script
    {
        public Player owner = null;
        private Vector2 moveDirection;

        public override void OnUpdate()
        {
            OnMove();
        }

        private void OnMove()
        {
            moveDirection = Vector2.UnitX * Input.GetAxis("Horizontal") + Vector2.UnitY * Input.GetAxis("Vertical");
            if (moveDirection != Vector2.Zero)
            {
                NetworkMessage msg = new NetworkMessage(MsgType.MoveDirection);
                msg.Write(moveDirection);
                msg.Send(ProtocolType.Udp);
            }

        }
    }
}
