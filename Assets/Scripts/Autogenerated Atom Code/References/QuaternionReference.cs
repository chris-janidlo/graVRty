using System;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Reference of type `Quaternion`. Inherits from `AtomReference&lt;Quaternion, QuaternionPair, QuaternionConstant, QuaternionVariable, QuaternionEvent, QuaternionPairEvent, QuaternionQuaternionFunction, QuaternionVariableInstancer, AtomCollection, AtomList&gt;`.
    /// </summary>
    [Serializable]
    public sealed class QuaternionReference : AtomReference<
        Quaternion,
        QuaternionPair,
        QuaternionConstant,
        QuaternionVariable,
        QuaternionEvent,
        QuaternionPairEvent,
        QuaternionQuaternionFunction,
        QuaternionVariableInstancer>, IEquatable<QuaternionReference>
    {
        public QuaternionReference() : base() { }
        public QuaternionReference(Quaternion value) : base(value) { }
        public bool Equals(QuaternionReference other) { return base.Equals(other); }
        protected override bool ValueEquals(Quaternion other)
        {
            throw new NotImplementedException();
        } 
    }
}
