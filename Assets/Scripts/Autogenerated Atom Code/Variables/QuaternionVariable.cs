using System;
using UnityEngine;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Variable of type `Quaternion`. Inherits from `AtomVariable&lt;Quaternion, QuaternionPair, QuaternionEvent, QuaternionPairEvent, QuaternionQuaternionFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/Quaternion", fileName = "QuaternionVariable")]
    public sealed class QuaternionVariable : AtomVariable<Quaternion, QuaternionPair, QuaternionEvent, QuaternionPairEvent, QuaternionQuaternionFunction>
    {
        protected override bool ValueEquals(Quaternion other)
        {
            return Value == other;
        }
    }
}
