using UnityEngine;

// Make sure to turn on 'Auto Sync Transforms' in Physics 2D,
// otherwise this script won't work as intended.
namespace FlipSide.Gameplay.Controller
{
	public class Controller2D : RaycastController
	{
		public float slopeAngleLimit = 45;

		public CollisionInfo collisions;
		Vector2 playerInput;

		bool flipHorizontal;
		bool flipVertical;

		public override void Start()
		{
			base.Start();
			collisions.faceDir = 1;

			flipHorizontal = transform.eulerAngles.y == 180 ^ transform.eulerAngles.z == 180;
			flipVertical = transform.eulerAngles.x == 180 ^ transform.eulerAngles.z == 180;
		}

		public void Move(Vector3 velocity, bool standingOnPlatform)
		{
			Move(velocity, Vector2.zero, standingOnPlatform);
		}

		public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
		{
			UpdateRaycastOrigins();
			collisions.Reset();
			collisions.velocityOld = velocity;
			playerInput = input;

			if (velocity.x != 0)
			{
				collisions.faceDir = (int)Mathf.Sign(velocity.x);
			}

			if (velocity.y < 0)
			{
				DescendSlope(ref velocity);
			}

			HorizontalCollisions(ref velocity);

			if (velocity.y != 0)
			{
				VerticalCollisions(ref velocity);
			}

			transform.Translate(velocity);

			if (standingOnPlatform)
			{
				collisions.below = true;
			}
		}

		#region Collision Detection

		void HorizontalCollisions(ref Vector3 velocity)
		{
			float directionX = collisions.faceDir;
			float rayLength = Mathf.Abs(velocity.x) + skinWidth;

			if (Mathf.Abs(velocity.x) < skinWidth)
			{
				rayLength = 2 * skinWidth;
			}

			for (int i = 0; i < horizontalRayCount; i++)
			{
				Vector2 rayOrigin = (directionX == -1 ^ flipHorizontal) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
				rayOrigin += (Vector2)transform.up * (horizontalRaySpacing * i) * (!flipVertical ? 1 : -1);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, transform.right * directionX, rayLength, collisionMask);

				Debug.DrawRay(rayOrigin, transform.right * directionX * rayLength, Color.red);

				if (hit)
				{
					if (hit.collider.tag == "Through" || hit.distance == 0)
					{
						continue;
					}

					float slopeAngle = Vector2.Angle(hit.normal, transform.up);

					if (i == (!flipVertical ? 0 : horizontalRayCount - 1) && slopeAngle <= slopeAngleLimit)
					{
						if (collisions.descendingSlope)
						{
							collisions.descendingSlope = false;
							velocity = collisions.velocityOld;
						}

						float distanceToSlopeStart = 0;
						if (slopeAngle != collisions.slopeAngleOld)
						{
							distanceToSlopeStart = hit.distance - skinWidth;
							velocity.x -= distanceToSlopeStart * directionX;
						}
						ClimbSlope(ref velocity, slopeAngle);
						velocity.x += distanceToSlopeStart * directionX;
					}

					if (!collisions.climbingSlope || slopeAngle > slopeAngleLimit)
					{
						velocity.x = Mathf.Min(Mathf.Abs(velocity.x), hit.distance - skinWidth) * directionX;
						rayLength = Mathf.Min(Mathf.Abs(velocity.x) + skinWidth, hit.distance);

						if (collisions.climbingSlope)
						{
							velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
						}

						collisions.left = directionX == -1;
						collisions.right = directionX == 1;
					}
				}
			}
		}

		void VerticalCollisions(ref Vector3 velocity)
		{
			float directionY = Mathf.Sign(velocity.y);
			float rayLength = Mathf.Abs(velocity.y) + skinWidth;

			for (int i = 0; i < verticalRayCount; i++)
			{
				Vector2 rayOrigin = (directionY == -1 ^ flipVertical) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
				rayOrigin += (Vector2)transform.right * (verticalRaySpacing * i + velocity.x) * (!flipHorizontal ? 1 : -1);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, transform.up * directionY, rayLength, collisionMask);

				Debug.DrawRay(rayOrigin, transform.up * directionY * rayLength, Color.red);

				if (hit)
				{
					if (hit.collider.tag == "Through" || hit.distance == 0)
					{
						if (directionY == 1 || hit.distance == 0)
						{
							continue;
						}
						if (collisions.fallingThroughPlatform)
						{
							continue;
						}
						if (playerInput.y == -1)
						{
							collisions.fallingThroughPlatform = true;
							Invoke("ResetFallingThroughPlatform", 0.3f);
							continue;
						}
					}

					velocity.y = (hit.distance - skinWidth) * directionY;
					rayLength = hit.distance;

					if (collisions.climbingSlope)
					{
						velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
					}

					collisions.above = directionY == 1;
					collisions.below = directionY == -1;
				}
			}

			if (collisions.climbingSlope)
			{
				float directionX = Mathf.Sign(velocity.x);
				rayLength = Mathf.Abs(velocity.x) + skinWidth;
				Vector2 rayOrigin = ((directionX == -1 ^ flipHorizontal) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + (Vector2)transform.up * velocity.y * (!flipVertical ? 1 : -1);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, transform.right * directionX, rayLength, collisionMask);

				if (hit)
				{
					float slopeAngle = Vector2.Angle(hit.normal, transform.up);
					if (slopeAngle != collisions.slopeAngle)
					{
						velocity.x = (hit.distance - skinWidth) * directionX;
						collisions.slopeAngle = slopeAngle;
					}
				}
			}
		}

		void ClimbSlope(ref Vector3 velocity, float slopeAngle)
		{
			float moveDistance = Mathf.Abs(velocity.x);
			float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

			if (velocity.y <= climbVelocityY)
			{
				velocity.y = climbVelocityY;
				velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
				collisions.below = true;
				collisions.climbingSlope = true;
				collisions.slopeAngle = slopeAngle;
			}
		}

		void DescendSlope(ref Vector3 velocity)
		{
			float directionX = Mathf.Sign(velocity.x);
			Vector2 rayOrigin = !flipVertical ? ((directionX == -1 ^ flipHorizontal) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft)
											  : ((directionX == -1 ^ flipHorizontal) ? raycastOrigins.topRight : raycastOrigins.bottomRight);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -transform.up, Mathf.Infinity, collisionMask);

			if (hit)
			{
				int slopeAngle = (int)Vector2.Angle(hit.normal, transform.up);
				if (slopeAngle != 0 && slopeAngle <= slopeAngleLimit)
				{
					if (Mathf.Sign(hit.normal.x) == directionX)
					{
						if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
						{
							float moveDistance = Mathf.Abs(velocity.x);
							float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
							velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
							velocity.y -= descendVelocityY;

							collisions.slopeAngle = slopeAngle;
							collisions.descendingSlope = true;
							collisions.below = true;
						}
					}
				}
			}
		}

		void ResetFallingThroughPlatform()
		{
			collisions.fallingThroughPlatform = false;
		}

		public struct CollisionInfo
		{
			public bool above, below;
			public bool left, right;

			public bool climbingSlope, descendingSlope;
			public float slopeAngle, slopeAngleOld;
			public Vector3 velocityOld;
			public int faceDir;
			public bool fallingThroughPlatform;

			public void Reset()
			{
				above = below = false;
				left = right = false;
				climbingSlope = descendingSlope = false;

				slopeAngleOld = slopeAngle;
				slopeAngle = 0;
			}
		}

		#endregion
	}
}