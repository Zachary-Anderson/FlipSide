using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlipSide.Gameplay.Controller;

namespace FlipSide.Gameplay
{
	[RequireComponent(typeof(Controller2D))]
	public class PlayerController : MonoBehaviour
	{
		#region References 'n Variables

		[Header("Jump Stuff")]
		[SerializeField, Tooltip("Maximum height of the player's jump.\nResult of a held press.")]
		private float maxJumpHeight = 4;
		[SerializeField, Tooltip("Minimum height of the player's jump.\nResult of a tap.")]
		private float minJumpHeight = 1;
		[SerializeField, Tooltip("The time it takes for the player to reach the apex of their high jump.")]
		private float timeToJumpApex = 0.4f;

		/// <summary>
		/// Affects the player's downward acceleration. Calculated using maxJumpHeight and timeToJumpApex.
		/// </summary>
		private float gravity;
		/// <summary>
		/// The velocity needed to achieve maximum jump height. Calculated using gravity and maxJumpHeight.
		/// </summary>
		private float maxJumpVelocity;
		/// <summary>
		/// The velocity needed to achieve minimum jump height. Calculated using gravity and minJumpHeight.
		/// </summary>
		private float minJumpVelocity;

		[Header("Movement Stuff")]

		[HideInInspector] // Reference to the Controller2D that handles moving.
		public Controller2D controller;

		[SerializeField, Tooltip("Movement speed of the player."), Space]
		private float moveSpeed = 6;

		/// <summary>
		/// Affects the player's acceleration while grounded.
		/// </summary>
		private float accelerationTimeGrounded = 0f;
		/// <summary>
		/// Affects the player's acceleration while airborne.
		/// </summary>
		private float accelerationTimeAirborne = 0f;

		[HideInInspector] // The player's velocity, that will be passed to the Controller2D.
		public Vector3 velocity;
		/// <summary>
		/// Gonna be honest I have no idea what this variable is for, doesn't matter though.
		/// </summary>
		private float velocityXSmoothing;

		// This code is disabled, since it isn't applicable to this project.
		/*public Vector2 wallJumpClimb;
		public Vector2 wallJumpOff;
		public Vector2 wallLeap;
		public float wallSlidingSpeedMax = 3;
		public float wallStickTime = 0.25f;
		float timeToWallUnstick;*/

		#endregion

		private void Start()
		{
			controller = GetComponent<Controller2D>();      // Get the reference for the Controller 2D.

			gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);          // Calculate the gravity using MATH!
			maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;                  // Calculate maxJumpVelocity using MORE MATH!!
			minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);   // Calculate minJumpVelocity using EVEN MORE MATH!!!
		}

		private void Update()
		{
			Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));  // Gather

			float targetVelocityX = input.x * moveSpeed;    // This code generates the horizontal velocity, but since accelerationTime is 0, it's a little bloated for its purpose.
			velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

			// Wall jump code disabled. Not applicable to this project.
			{
				/*int wallDirX = (controller.collisions.left) ? -1 : 1;

				bool wallSliding = false;
				if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
				{
					wallSliding = true;

					if (velocity.y < -wallSlidingSpeedMax)
					{
						velocity.y = -wallSlidingSpeedMax;
					}

					if (timeToWallUnstick > 0)
					{
						velocity.x = 0;
						velocityXSmoothing = 0;

						if (input.x != wallDirX && input.x != 0)
						{
							timeToWallUnstick -= Time.deltaTime;
						}
						else
						{
							timeToWallUnstick = wallStickTime;
						}
					}
					else
					{
						timeToWallUnstick = wallStickTime;
					}
				}*/
			}
			
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
			{	// If the player attempts to jump...
				// Wall jump code disabled.
				{
					/*if (wallSliding)
					{
						if (wallDirX == input.x)
						{
							velocity.x = -wallDirX * wallJumpClimb.x;
							velocity.y = wallJumpClimb.y;
						}
						else if (input.x == 0)
						{
							velocity.x = -wallDirX * wallJumpOff.x;
							velocity.y = wallJumpOff.y;
						}
						else
						{
							velocity.x = -wallDirX * wallLeap.x;
							velocity.y = wallLeap.y;
						}
					}*/
				}

				if (controller.collisions.below)
				{                                   // If the player is grounded...
					velocity.y = maxJumpVelocity;   // Set the player's velocity to make them go up.
				}
			}
			if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.Space))
			{                                                           // If the player lets go of the jump button...
				velocity.y = Mathf.Min(velocity.y, minJumpVelocity);    // Limit the player's vertical velocity, so they jump lower.
			}

			velocity.y += gravity * Time.deltaTime;                     // Decrease the player's velocity according to the gravity.
			velocity.y = Mathf.Max(velocity.y, -2 * maxJumpVelocity);   // Repurpose maxJumpVelocity as a terminal velocity so the player doesn't fall too fast.
			controller.Move(velocity * Time.deltaTime, input);          // Call the Controller2D to move the player.

			if (controller.collisions.above || controller.collisions.below)
			{                       // If the player is grounded, or hit their head...
				velocity.y = 0;     // Set their vertical velocity to 0.
			}
		}
	}
}