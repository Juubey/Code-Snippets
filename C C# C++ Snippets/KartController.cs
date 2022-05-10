/*
 * KartController.cs
 * Author(s): Albert Njubi
 * Date Created: 6/3/21
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Class that controls attributes of the Kart Movement.
/// </summary>
public abstract partial class KartController : MonoBehaviour
{
    #region Kart Orientation
    [Header("Orientation")]
	public Transform FrontAnchor;
	public Transform BackAnchor;
	public LayerMask FloorLayer;
    #endregion

    #region Kart Movement
    [Header("Movement")]
	public float MaximumSpeed;
	public float Acceleration;
	public float TurnSpeed;
	public AnimationCurve Gravity;
	public bool FollowAlternativePath;
	#endregion

	#region Kart Drift Attributes
	[Header("Drift")]
	public AnimationCurve DriftTurn;
	public AnimationCurve DriftTurnMultiplier;
	public AnimationCurve DriftLateralMovement;
	public float TimeToDriftLevel1, TimeToDriftLevel2;
	public Transform DriftSparksContainer;
	public ParticleSystem DriftSparksEvent, DriftSparksContinuous0, DriftSparksContinuous1, DriftSparksContinuous2;
    #endregion

    #region Audio
    [Header("Audio")]
	public AudioClip[] CollisionSounds;
	public AudioClip[] SpeedBoostSounds;
	public AudioClip[] DriftStartSounds;
	public AudioClip[] DriftEventSounds;
	public AudioSource DriftContinuousSource;
    #endregion

    #region Other
    [Header("Other")]
	[System.NonSerialized]
	public RectTransform MapPin;
	public Animator Animator;
	public Sprite Icon;
    #endregion

    // Inputs.
    protected float inputSpeedRatio, inputTurn;
	protected bool inputDrift;
	private float inputDeactivationTime = 4f;

	// Movement variables.
	private float currentSpeedRatio, currentTurn, appliedTurn;
	private Vector3 velocity;
	protected float airTime;
	protected bool isOnSlowZone;
	protected bool startingBoost;
	private float movementDeactivationTime = 7f;

	// Drift variables.
	protected bool isDrifting;
	protected float driftTime, driftDirection;
	private int driftLevelReached = 0;
	private bool isDriftingConfirmed;

	// Position variables.
	[Header("Position")]
	public int LapIndex;
	public int WaypointIndex;
	public float Ratio;
	public int FurthestWaypointIndex;
	public int PositionIndex;
	protected bool hasFinishedRace;

	// Other variables.
	private float lookDot;

	private new Rigidbody rigidbody;
	private AudioSource audioSource;


	/// <summary>
	/// This method gets the components of KartController from the inspector 
	/// </summary>
	protected virtual void Start()
	{
		this.rigidbody = this.GetComponent<Rigidbody>();
		this.audioSource = this.GetComponent<AudioSource>();
		this.hand = this.GetComponentInChildren<KartHand>();

		if (this.Animator == null)
		{
			this.Animator = this.GetComponentInChildren<Animator>();
		}

		this.DriftSparksEvent.Stop();
	}


	/// <summary>
	/// The update method takes inputs from the other methods
    /// each frame.
	/// </summary>
	protected virtual void Update()
	{
		this.inputDeactivationTime -= Time.deltaTime;
		this.movementDeactivationTime -= Time.deltaTime;

		this.ApplyGravityAndFloorAngle(Time.deltaTime);

		if (this.inputDeactivationTime <= 0f)
		{
			this.PollInput();
		}

		this.ApplyInput(Time.deltaTime);
		this.UpdateDrift(Time.deltaTime);
		this.Move(Time.deltaTime);
		this.UpdateItem(Time.deltaTime);

		this.UpdateMapPin();
		this.LookAtOtherKarts(Time.deltaTime);
	}

	/// <summary>
	/// This method controls whether the Kart is airborne.
    /// If neither Anchors are floored apply verticle velocity.
	/// </summary>
	private void ApplyGravityAndFloorAngle(float deltaTime)
	{
		this.isOnSlowZone = false;

		bool bothHit = true;
		bool noneHit = true;
		Vector3 frontPoint, backPoint, frontNormal, backNormal;
		this.GetFloorPosition(this.FrontAnchor, deltaTime, out frontPoint, out frontNormal, ref bothHit, ref noneHit);
		this.GetFloorPosition(this.BackAnchor, deltaTime, out backPoint, out backNormal, ref bothHit, ref noneHit);

		Vector3 middle = (frontPoint + backPoint) / 2f;
		Vector3 forward = Vector3.Normalize(frontPoint - backPoint);
		Vector3 normal = (frontNormal + backNormal) / 2f;

		if (noneHit)
		{
			forward.y *= 0.8f;
		}

		this.transform.position = Vector3.MoveTowards(this.transform.position, middle, 15f * deltaTime * (1 + this.airTime));
		this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(forward, normal), 50f * deltaTime);

		if (bothHit)
		{
			this.airTime = 0f;
		}
		else
		{
			this.airTime += deltaTime;
		}
	}

	protected abstract void PollInput();


	/// <summary>
	/// Movement logic
	/// </summary>
	private void ApplyInput(float deltaTime)
	{
		if (this.inkTime > 0f)
		{
			this.inputTurn = Mathf.Lerp(this.inputTurn, this.InkTurn.Evaluate(this.inkTime), 0.15f);
		}

		// Fully turning back takes 0.25s.
		this.currentTurn = Mathf.MoveTowards(this.currentTurn, this.inputTurn, deltaTime * 4f);

		if (this.startingBoost)
		{
			this.startingBoost = false;
			this.Boost(1.1f);
		}

		if (this.isOnSlowZone && this.starTime <= 0f)
		{
			this.inputSpeedRatio = Mathf.Min(this.inputSpeedRatio, 0.5f);
			this.StopDrifting(true);
		}

		if (this.movementDeactivationTime <= 0)
		{
			float ratio;

			// Fully accelerating takes time based on Acceleration.
			if (this.inputSpeedRatio == 0)
			{
				// No speed input: low acceleration (only inertia).
				ratio = 0.5f;
			}
			else if (Mathf.Sign(this.inputSpeedRatio) == this.currentSpeedRatio)
			{
				// Same direction: normal acceleration (unless boosted).
				if (this.currentSpeedRatio > 1f)
				{
					ratio = 0.35f;
				}
				else
				{
					ratio = 1.0f;
				}
			}
			else
			{
				// Opposite direction: doubled acceleration.
				ratio = 2.0f;
			}

			this.currentSpeedRatio = Mathf.MoveTowards(this.currentSpeedRatio, this.inputSpeedRatio, this.Acceleration * deltaTime * ratio);
		}

		this.Animator.SetFloat("Speed", this.currentSpeedRatio);
		this.Animator.SetBool("Drift", this.isDrifting);
		this.Animator.SetFloat("Turn", this.appliedTurn);
	}

	/// <summary>
	/// This method checks if the player is still drifting.
	/// </summary>
	private void UpdateDrift(float deltaTime)
	{
		if (this.isDrifting)
		{
			if (!this.inputDrift)
			{
				this.StopDrifting();
				return;
			}

			bool driftStarting = this.driftTime < 0.5f;

			// Increment.
			this.driftTime += deltaTime;

			// Set the direction of turn during the first 0.5s and cancel if none was set.
			if (driftStarting)
			{
				this.driftDirection += this.inputTurn * deltaTime;
				if (this.inputTurn != 0f)
				{
					this.isDriftingConfirmed = true;
				}

				// The 0.5s window ended this frame.
				if (this.driftTime > 0.5f)
				{
					if (!this.isDriftingConfirmed)
					{
						this.inputDrift = false;
						this.StopDrifting();
					}
					else
					{
						this.driftDirection = Mathf.Sign(this.driftDirection);
						this.DriftSparksContainer.localEulerAngles = Vector3.up * (1 - this.driftDirection) * 90;
					}
				}
			}

			this.UpdateDriftSparks();
		}
		else
		{
			// Decrement.
			if (this.driftTime > 0)
			{
				this.driftTime -= deltaTime;
			}
			else
			{
				this.driftTime = 0;
			}

			this.DisableDriftSparksAndBoost();

			// Start drifting again.
			if (this.driftTime <= 0 && this.inputDrift)
			{
				this.StartDrifting();
			}
		}
	}

	/// <summary>
	/// This is called when the player just started drifting.
    /// If they they are play audio.
	/// </summary>
	protected virtual void StartDrifting()
	{
		this.isDrifting = true;
		this.isDriftingConfirmed = false;
		this.driftLevelReached = -1;
		this.driftTime = 0;

		this.audioSource.PlayOneShot(this.DriftStartSounds[Random.Range(0, this.DriftStartSounds.Length)]);
	}

	/// <summary>
	/// If the kart is still drifting apply audio and Driftspark event to the kart.
	/// </summary>
	private void UpdateDriftSparks()
	{
		if (this.driftLevelReached == -1)
		{
			this.driftLevelReached = 0;

			this.DriftSparksContinuous0.Play();
			this.Invoke(nameof(PlayDriftSound), 0.3f);
		}

		if (this.driftLevelReached == 0 && this.driftTime > this.TimeToDriftLevel1)
		{
			this.driftLevelReached = 1;

			this.DriftSparksContinuous0.Stop();
			this.DriftSparksContinuous1.Play();
			this.DriftSparksEvent.Play();

			this.audioSource.PlayOneShot(this.DriftEventSounds[Random.Range(0, this.DriftEventSounds.Length)]);
		}

		if (this.driftLevelReached == 1 && this.driftTime > this.TimeToDriftLevel2)
		{
			this.driftLevelReached = 2;

			this.DriftSparksContinuous1.Stop();
			this.DriftSparksContinuous2.Play();
			this.DriftSparksEvent.Play();

			this.audioSource.PlayOneShot(this.DriftEventSounds[Random.Range(0, this.DriftEventSounds.Length)]);
		}
	}

	//Disables Drift effects each level if boost levels are reached
	private void DisableDriftSparksAndBoost()
	{
		if (this.driftLevelReached == 2)
		{
			this.Boost(1.5f);
		}
		else if (this.driftLevelReached == 1)
		{
			this.Boost(1.25f);
		}

		if (this.driftLevelReached != -1)
		{
			this.DriftSparksContinuous0.Stop();
			this.DriftSparksContinuous1.Stop();
			this.DriftSparksContinuous2.Stop();

			this.CancelInvoke(nameof(PlayDriftSound));
			this.DriftContinuousSource.Stop();

			this.driftLevelReached = -1;
		}
	}

	//Plays drift sound
	private void PlayDriftSound()
	{
		this.DriftContinuousSource.Play();
	}
	/// <summary>
	/// Cancels the boost if the driftlevel is 0.
	/// </summary>
	private void StopDrifting(bool cancelBoost = false)
	{
		this.isDrifting = false;
		this.driftTime = Mathf.Min(this.driftTime, 1f);

		if (cancelBoost)
		{
			this.driftLevelReached = 0;
		}
	}

	/// <summary>
	/// Gets the floor position of the kart by Raycasting to colliders.
	/// </summary>
	private void GetFloorPosition(Transform anchor, float deltaTime, out Vector3 position, out Vector3 normal, ref bool hasHit, ref bool hasNotHit)
	{
		float gravity = this.Gravity.Evaluate(this.airTime);

		Vector3 rayStart = anchor.position + anchor.up * 0.5f;
		Vector3 rayGoal = anchor.position - anchor.up * (anchor.localPosition.y + gravity * deltaTime);

		RaycastHit hit;
		if (Physics.Linecast(rayStart, rayGoal, out hit, this.FloorLayer))
		{
			hasNotHit = false;
			Debug.DrawLine(rayStart, hit.point, Color.green);

			position = hit.point;
			normal = hit.normal;

			if (hit.collider.tag == "Slow")
			{
				this.isOnSlowZone = true;
			}
		}
		else
		{
			hasHit = false;
			Debug.DrawLine(rayStart, rayGoal, Color.red);

			position = rayGoal;
			normal = Vector3.up;
		}
	}

	/// <summary>
	/// Move Logic
    ///	if 
	/// </summary>
	protected virtual void Move(float deltaTime)
	{
		if (this.starTime > 0f)
		{
			deltaTime *= 1.25f;
		}

		float directionalDriftTime = this.driftTime * Mathf.Sign(this.driftDirection);

		Vector3 moveDirection;

		if (directionalDriftTime != 0f)
		{
			moveDirection = Vector3.Normalize(this.transform.forward + this.transform.right * this.DriftLateralMovement.Evaluate(directionalDriftTime));
			this.appliedTurn = this.currentTurn * this.DriftTurnMultiplier.Evaluate(directionalDriftTime) + this.DriftTurn.Evaluate(directionalDriftTime);
		}
		else
		{
			moveDirection = this.transform.forward;
			this.appliedTurn = this.currentTurn;
		}

		if (this.movementDeactivationTime <= 0)
		{
			Vector3 goalPosition = this.transform.position + ((moveDirection * this.currentSpeedRatio * this.MaximumSpeed) + this.velocity) * deltaTime;
			this.velocity = Vector3.MoveTowards(this.velocity, Vector3.zero, deltaTime * 5);

			RaycastHit hit;
			if (this.rigidbody.SweepTest(goalPosition - this.transform.position, out hit, Vector3.Distance(this.transform.position, goalPosition))
				&& !hit.collider.isTrigger
				&& hit.collider.tag != "Player"
				&& hit.collider.tag != "MoveThrough"
				&& hit.collider.tag != "DamageItem")
			{
				goalPosition = Vector3.MoveTowards(this.transform.position, goalPosition, hit.distance);
			}

			this.transform.position = goalPosition;
			this.transform.Rotate(Vector3.up * this.appliedTurn * this.TurnSpeed * deltaTime, Space.Self);
		}
	}

	private void UpdateMapPin()
	{
		this.MapPin.anchoredPosition = new Vector2(this.transform.position.x, this.transform.position.z);
	}

	private void LookAtOtherKarts(float deltaTime)
	{
		float closestDistance = 5f;
		float closestDot = 0f;
		foreach (KartController otherKart in RaceController.Instance.Karts)
		{
			if (otherKart == this)
			{
				continue;
			}

			float distance = Vector3.Distance(this.transform.position, otherKart.transform.position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestDot = Vector3.Dot(this.transform.right, Vector3.Normalize(otherKart.transform.position - this.transform.position));
			}
		}

		this.lookDot = Mathf.MoveTowards(this.lookDot, closestDot, deltaTime * 2f);
		this.Animator.SetFloat("Look", this.lookDot);
	}
	//If LapIndex is 3 the race is finished.
	public virtual void OnNewLap()
	{
		if (this.LapIndex == 3)
		{
			this.FinishRace();
		}
	}
	//If race is finished set to true and clear items.
	protected virtual void FinishRace()
	{
		if (this.preparedItem)
		{
			this.ThrowPreparedItem();
		}

		this.SetItem(Item.None);

		this.hasFinishedRace = true;
	}
	/// <summary>
	/// If the kart does not have starTime slow speed, disable input and stop drift.
	/// </summary>
	public void Damage(Vector3 direction)
	{
		if (this.starTime > 0f)
		{
			return;
		}

		if (direction.magnitude > 0.1f)
		{
			this.velocity = direction * 10;
		}

		this.currentSpeedRatio *= 0.5f;
		this.currentTurn *= 0.5f;

		this.inputSpeedRatio = 0f;
		this.inputTurn = 0f;
		this.inputDeactivationTime = 0.5f;

		this.StopDrifting(true);

		this.Animator.CrossFade("Hit", 0.1f);
	}
	//applys velocity on fixed update.
	private void FixedUpdate()
	{
		this.rigidbody.velocity *= 0.95f;
	}

	/// <summary>
	/// If the kart enters the a collider apply logic depending on tag.
	/// </summary>
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag == "MoveThrough")
		{
			return;
		}

		Vector3 normal = collision.contacts[0].normal;
		if (normal.y > 0.5f)
		{
			return;
		}

		if (Vector3.Dot(this.transform.forward, normal) > 0.5f)
		{
			this.currentSpeedRatio = Mathf.Min(this.currentSpeedRatio + 0.2f, 2f);
		}
		else
		{
			this.currentSpeedRatio *= 0.5f;
		}

		if (collision.collider.tag != "Player" && collision.rigidbody && !collision.rigidbody.isKinematic)
		{
			collision.rigidbody.AddForceAtPosition(this.transform.forward * this.rigidbody.mass, collision.contacts[0].point, ForceMode.Impulse);
		}

		if (collision.collider.tag == "Damage")
		{
			this.Damage(normal);
		}
		else
		{
			this.velocity += normal * 5 * Mathf.Abs(this.currentSpeedRatio);
		}

		this.audioSource.PlayOneShot(this.CollisionSounds[Random.Range(0, this.CollisionSounds.Length)], 0.2f);
	}

	/// <summary>
	/// If the kart enters the movethrough collider with star time return vector 3
    /// and adjust velocity
	/// </summary>
	private void OnCollisionStay(Collision collision)
	{
		if (collision.collider.tag == "MoveThrough")
		{
			return;
		}

		if (this.starTime > 0f)
		{
			return;
		}

		Vector3 normal = collision.contacts[0].normal;
		if (normal.y > 0.5f)
		{
			return;
		}

		if (Vector3.Dot(this.transform.forward, normal) < 0.5f)
		{
			this.currentSpeedRatio *= 0.9f;
		}

		this.velocity += normal * Mathf.Abs(this.currentSpeedRatio);
	}
	/// <summary>
	/// If the kart enters the Collider of boost, powerups or damage
    /// apply logic.
	/// </summary>
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Speed")
		{
			this.Boost();
		}

		if (other.tag == "Powerup")
		{
			other.GetComponent<Animator>().Play("Collect");

			this.CollectItemBox();
		}

		if (other.tag == "DamageItem")
		{
			ItemBehaviour item = other.GetComponentInParent<ItemBehaviour>();
			if (item == null || item.Owner == this)
			{
				return;
			}

			Vector3 direction = this.transform.position - item.transform.position;
			item.Destroy();
			this.Damage(item.AppliesBounce ? direction.normalized : Vector3.zero);
		}
	}
	//play boost animation and double speed ratio.
	protected virtual void Boost(float ratio = 2f)
	{
		this.Animator.Play("Accelerate", 2, 0f);
		this.currentSpeedRatio = ratio;
		this.audioSource.PlayOneShot(this.SpeedBoostSounds[Random.Range(0, this.SpeedBoostSounds.Length)]);
	}
	//On Collider trigger set currentspeedratio to 2.0
	private void OnTriggerStay(Collider other)
	{
		if (other.tag == "Speed")
		{
			this.currentSpeedRatio = 2f;
		}
	}
}