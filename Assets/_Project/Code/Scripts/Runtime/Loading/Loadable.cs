using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraVRty.Loading
{
    public abstract class Loadable : ScriptableObject
    {
        public virtual float LoadProgress { get; protected set; }
        public virtual bool FinishedLoading { get; protected set; }

        public abstract IEnumerator LoadRoutine ();

        protected void assertLoaded (string caller)
        {
            if (!FinishedLoading)
            {
                throw new InvalidOperationException($"should not be calling {caller} because {name} has not finished loading");
            }
        }
    }
}
