using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GraVRty.XR
{
    public class XRGrabInteractableExtended : XRGrabInteractable
    {
        public bool AllowHover;

        protected override void OnActivated (ActivateEventArgs args)
        {
            if (isSelected || AllowHover) base.OnActivated(args);
        }

        protected override void OnDeactivated (DeactivateEventArgs args)
        {
            if (isSelected || AllowHover) base.OnDeactivated(args);
        }
    }
}
