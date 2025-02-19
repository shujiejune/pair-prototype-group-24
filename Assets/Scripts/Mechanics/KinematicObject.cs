﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Implements game physics for some in game entity.
    /// </summary>
    public class KinematicObject : MonoBehaviour
    {
        /// <summary>
        /// The minimum normal (dot product) considered suitable for the entity sit on.
        /// </summary>
        public Vector2 minGroundNormal = new Vector2(0.65f, 0.65f);

        /// <summary>
        /// A custom gravity coefficient applied to this entity.
        /// </summary>
        public float gravityModifier = 1f;
        public Vector2 gravityDirection = Vector2.down;

        /// <summary>
        /// The current velocity of the entity.
        /// </summary>
        public Vector2 velocity;

        /// <summary>
        /// Is the entity currently sitting on a surface?
        /// </summary>
        /// <value></value>
        /// allow other scripts to read IsGrounded
        /// but not write to it.
        public bool IsGrounded { get; private set; }

        protected Vector2 targetVelocity; //desired velocity set by user input
        protected Vector2 groundNormal;
        protected Rigidbody2D body;
        protected ContactFilter2D contactFilter;
        protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

        protected const float minMoveDistance = 0.001f;
        protected const float shellRadius = 0.01f;

        /// <summary>
        /// Teleport to some position.
        /// </summary>
        /// <param name="position"></param>
        public void Teleport(Vector3 position)
        {
            body.position = position;
            velocity *= 0;
            body.velocity *= 0;
        }

        protected virtual void OnEnable()
        {
            body = GetComponent<Rigidbody2D>();
            body.isKinematic = true;
        }

        protected virtual void OnDisable()
        {
            body.isKinematic = false;
        }

        protected virtual void Start()
        {
            contactFilter.useTriggers = false;
            contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            contactFilter.useLayerMask = true;
        }

        protected virtual void Update()
        {
            targetVelocity = Vector2.zero;
            ComputeVelocity();
        }

        protected virtual void ComputeVelocity()
        {

        }

        protected virtual void FixedUpdate()
        {
            Debug.Log("targetVelocity: " + targetVelocity);
            Debug.Log("velocity: " + velocity);
            Debug.Log("groundNormal: " + groundNormal);
            //velocity.x = targetVelocity.x;
            //convertAxes(velocity);
            //if already falling, fall faster than the jump speed, otherwise use normal gravity.
            if (Vector2.Dot(velocity, -gravityDirection) < 0)
                velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
            else
                velocity += Physics2D.gravity * Time.deltaTime;

            velocity.x = targetVelocity.x;

            IsGrounded = false;  //ensure ground detection happens every frame

            var deltaPosition = velocity * Time.deltaTime;

            //ensure the object moves parallel to the ground
            var moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

            var move = moveAlongGround * deltaPosition.x;

            PerformMovement(move, false);

            move = Vector2.up * deltaPosition.y;

            PerformMovement(move, true);

        }

        void PerformMovement(Vector2 move, bool yMovement)
        {
            var distance = move.magnitude;

            if (distance > minMoveDistance)
            {
                //check if we hit anything in current direction of travel
                //this is an overload taking 4 parameters
                var count = body.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
                for (var i = 0; i < count; i++)
                {
                    var currentNormal = hitBuffer[i].normal;

                    //is this surface flat enough to land on? 
                    //(based on the current gravity direction)
                    if (Vector2.Dot(currentNormal, -gravityDirection) > minGroundNormal.magnitude)
                    {
                        IsGrounded = true;
                        // if moving up, change the groundNormal to new surface normal.
                        if (yMovement)
                        {
                            groundNormal = currentNormal;
                            currentNormal.x = 0;
                        }
                    }
                    if (IsGrounded)
                    {
                        //how much of our velocity aligns with surface normal?
                        var projection = Vector2.Dot(velocity, currentNormal);
                        if (projection < 0)
                        {
                            //slower velocity if moving against the normal (up a hill).
                            velocity = velocity - projection * currentNormal;
                        }
                    }
                    else
                    {
                        //We are airborne, but hit something, so cancel vertical up and horizontal velocity.
                        //no rebound, because we don't have bouncy surfaces
                        //velocity.x *= 0;
                        //velocity.y = Mathf.Min(velocity.y, 0);
                        velocity -= Vector2.Dot(velocity, currentNormal) * currentNormal;
                    }
                    //remove shellDistance from actual move distance.
                    var modifiedDistance = hitBuffer[i].distance - shellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }
            body.position = body.position + move.normalized * distance;
        }

        public void ShiftGravity(Vector2 newGravityDirection)
        {
            //gravityModifier = newGravityDirection * Mathf.Abs(gravityModifier.magnitude);
            gravityDirection = newGravityDirection.normalized;
            Physics2D.gravity = newGravityDirection * 9.8f;
        }

        //convert velocity vector from conventional (x, y) to (tangent, normal)
        //based on the slope of current surface
        private void convertAxes(Vector2 velocity) 
        {
            float a = targetVelocity.magnitude;
            float b = velocity.y;
            float tang = groundNormal.x;
            float norm = groundNormal.y;
            
            float sqrtSum = tang * tang + norm * norm;

            velocity.x = (a * norm + b * tang) / sqrtSum;
            velocity.y = (b * norm - a * tang) / sqrtSum;
        }
    }
}
