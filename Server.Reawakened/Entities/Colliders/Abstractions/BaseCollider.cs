using System;
using System.Collections.Generic;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Attributes;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders.Abstractions;

public abstract class BaseCollider
{
    [ThreadStatic]
    private static List<BaseCollider> s_colliderBuffer;

    [ThreadStatic]
    private static List<string> s_collidedIdBuffer;

    [ThreadStatic]
    private static HashSet<string> s_collidedSet;

    public abstract Room Room { get; }
    public abstract string Id { get; }
    public abstract Vector3Model Position { get; }
    public abstract RectModel BoundingBox { get; }
    public abstract string Plane { get; }
    public abstract ColliderType Type { get; }

    public bool IsInvisible { get; private set; }
    public bool Active { get; set; }

    public Rect ColliderBox => new(
        Position.X + BoundingBox.X,
        Position.Y + BoundingBox.Y,
        BoundingBox.Width,
        BoundingBox.Height
    );

    protected BaseCollider() => Active = true;
    protected void InitializeCollider(bool addToRoom = true)
    {
        var invisible = Room.GetEntityFromId<InvisibilityControllerComp>(Id);
        IsInvisible = invisible != null && invisible.ApplyInvisibility;

        if (addToRoom)
            Room.AddColliderToList(this);
    }

    public virtual string[] RunCollisionDetection() => Array.Empty<string>();

    public virtual void SendCollisionEvent(BaseCollider received) { }

    public bool CheckCollision(BaseCollider collided)
    {
        if (Plane != collided.Plane)
            return false;

        // Direct bounding box overlap check without instantiating UnityEngine.Rect
        var selfMinX = Position.X + BoundingBox.X;
        var selfMaxX = selfMinX + BoundingBox.Width;
        var selfMinY = Position.Y + BoundingBox.Y;
        var selfMaxY = selfMinY + BoundingBox.Height;

        var otherMinX = collided.Position.X + collided.BoundingBox.X;
        var otherMaxX = otherMinX + collided.BoundingBox.Width;
        var otherMinY = collided.Position.Y + collided.BoundingBox.Y;
        var otherMaxY = otherMinY + collided.BoundingBox.Height;

        return selfMaxX > otherMinX && selfMinX < otherMaxX &&
               selfMaxY > otherMinY && selfMinY < otherMaxY;
    }

    public virtual bool CanCollideWithType(BaseCollider collider) => false;
    public virtual bool CanOverrideInvisibleDetection() => true;

    public string[] RunBaseCollisionDetection()
    {
        s_colliderBuffer ??= new List<BaseCollider>(256);
        s_collidedIdBuffer ??= new List<string>(16);
        s_collidedSet ??= new HashSet<string>();

        Room.GetColliders(s_colliderBuffer);
        s_collidedIdBuffer.Clear();
        s_collidedSet.Clear();

        for (var i = 0; i < s_colliderBuffer.Count; i++)
        {
            var collider = s_colliderBuffer[i];

            if (collider.Active &&
                (!collider.IsInvisible || CanOverrideInvisibleDetection()) &&
                CanCollideWithType(collider) &&
                CheckCollision(collider))
            {
                if (s_collidedSet.Add(collider.Id))
                {
                    s_collidedIdBuffer.Add(collider.Id);
                }

                collider.SendCollisionEvent(this);
            }
        }

        return s_collidedIdBuffer.ToArray();
    }
}
