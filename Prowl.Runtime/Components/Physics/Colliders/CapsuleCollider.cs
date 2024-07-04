﻿using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Prowl.Icons;
using NRigidPose = BepuPhysics.RigidPose;

namespace Prowl.Runtime;

[AddComponentMenu($"{FontAwesome6.HillRockslide}  Physics/{FontAwesome6.Capsules}  Capsule Collider")]
public sealed class CapsuleCollider : Collider
{
    [SerializeField, HideInInspector] private float _radius = 0.5f;
    [SerializeField, HideInInspector] private float _length = 1f;

    [ShowInInspector]
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            Container?.ReAttach();
        }
    }

    [ShowInInspector]
    public float Length
    {
        get => _length;
        set
        {
            _length = value;
            Container?.ReAttach();
        }
    }

    public float WorldRadius
    {
        get {
            var scale = this.Transform.lossyScale;
            return _length * (float)MathD.Max(scale.x, scale.z);
        }
    }

    public float WorldLength
    {
        get => _length * (float)this.Transform.lossyScale.y;
    }

    internal override void AddToCompoundBuilder(BufferPool pool, ref CompoundBuilder builder, NRigidPose localPose)
    {
        builder.Add(new Capsule(WorldRadius, WorldLength), localPose, Mass);
    }
}