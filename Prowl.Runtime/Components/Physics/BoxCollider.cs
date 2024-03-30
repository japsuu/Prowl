﻿using Jitter2.Collision.Shapes;
using Prowl.Icons;
using System.Collections.Generic;

namespace Prowl.Runtime
{
    [AddComponentMenu($"{FontAwesome6.HillRockslide}  Physics/{FontAwesome6.Box}  Box Collider")]
    public class BoxCollider : Collider
    {
        public Vector3 size = Vector3.one;

        public override List<Shape> CreateShapes() => [ new BoxShape(size) ];
        public override void OnValidate()
        {
            (Shape[0] as BoxShape).Size = size;
            Shape[0].UpdateShape();
            var rigid = GetComponentInParent<Rigidbody>();
            if(rigid != null)
                rigid.IsActive = true;
        }

        public void DrawGizmosSelected()
        {
            Gizmos.Matrix = Matrix4x4.CreateScale(size * 1.0025f) * GameObject.GlobalCamRelative;
            Gizmos.Matrix = Matrix4x4.Multiply(Gizmos.Matrix, Matrix4x4.CreateScale(GameObject.transform.lossyScale));
            Gizmos.Cube(Color.yellow);
        }
    }

}