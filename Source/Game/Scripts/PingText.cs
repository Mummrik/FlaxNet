using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game
{
    public class PingText : Script
    {
        public static Label s_PingText;
        public override void OnAwake()
        {
            s_PingText = Actor.As<UIControl>().Get<Label>();
        }
    }
}
