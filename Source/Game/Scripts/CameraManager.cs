using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class CameraManager : Script
    {
        private const float minZoom = 350;
        private const float maxZoom = 750;
        [Serialize, ShowInEditor, Limit(minZoom, maxZoom)]
        private float CameraZoom = maxZoom;
        private Actor focusTarget = null;
        private Creature creature = null;
        private Camera camera = null;


        private Vector3 CameraOffset { get => (Vector3.Up + (Vector3.Backward * 0.6f)) * CameraZoom; }
        private float newZoomTarget;

        public override void OnAwake()
        {
            camera = Camera.MainCamera;
            newZoomTarget = CameraZoom;
        }

        public void SetFocusTarget(Actor target)
        {
            focusTarget = target;
            camera.Position = focusTarget.Position + CameraOffset;
            creature = focusTarget.GetScript<Creature>();
        }

        public override void OnLateUpdate()
        {
            SetCameraZoom();
            SetCameraPosition();
        }

        public void Zoom(float amount)
        {
            float newZoom = CameraZoom - (amount * 50);
            if (newZoom >= minZoom && newZoom <= maxZoom)
            {
                newZoomTarget = newZoom;
            }
        }

        private void SetCameraPosition()
        {
            if (focusTarget != null && camera.Position != focusTarget.Position + CameraOffset)
            {
                float lerpSpeed = (creature.moveSpeed > 0 ? creature.moveSpeed : 5f) * Time.DeltaTime;
                camera.Position = Vector3.Lerp(camera.Position, focusTarget.Position + CameraOffset, lerpSpeed);
            }
        }

        private void SetCameraZoom()
        {
            if (CameraZoom != newZoomTarget)
            {
                CameraZoom = Mathf.Lerp(CameraZoom, newZoomTarget, 1f);
            }
        }
    }
}
