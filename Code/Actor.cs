using Sandbox;
using Sandbox.Citizen;
using Sandbox.UI;
using System.Xml.Linq;

public abstract partial class Actor : Component
{
	[Property]
	[Category( "Info" ), Order( 1 )]
	public bool RandomNameOnSpawn { get; set; } = true;

	[Property, HideIf( "RandomNameOnSpawn", true )]
	[Category( "Info" ), Order( 1 )]
	public virtual string FirstName { get; set; } = "John";

	[Property, HideIf( "RandomNameOnSpawn", true )]
	[Category( "Info" ), Order( 1 )]
	public virtual string LastName { get; set; } = "Doe";

	[Property, HideIf( "RandomNameOnSpawn", true )]
	[Category( "Info" ), Order( 1 )]
	public virtual string UserName { get; set; }

	[Property, HideIf( "RandomNameOnSpawn", true )]
	[Category( "Info" ), Order( 1 )]
	public string FullName => string.IsNullOrWhiteSpace( UserName ) ? $"{FirstName} {LastName}" : UserName;

	[Property]
	[Category( "Info" ), Order( 1 )]
	public Gender Gender { get; set; } = Gender.Undefined;

	[Property, HideIf( "RandomNameOnSpawn", true )]
	[Button( "Random Name", "casino" )]
	[Category( "Info" ), Order( 1 )]
	public void GenerateRandomName()
	{
		var randomName = Actor.RandomName( Gender );
		FirstName = randomName.Item1;
		LastName = randomName.Item2;
	}

	[Property]
	[Category( "Stats" ), Order( 2 )]
	[Range( 0f, 200f, 10f )]
	public float WalkSpeed { get; set; } = 70f;

	[Property]
	[Category( "Stats" ), Order( 2 )]
	[Range( 0f, 400f, 10f )]
	public float RunSpeed { get; set; } = 160f;

	[Property]
	[Category( "Stats" ), Order( 2 )]
	[WideMode]
	public List<string> InteractPhrases { get; set; } = new();

	[Property]
	[Category( "Components" ), Order( 5 )]
	public SkinnedModelRenderer ModelRenderer { get; set; }

	[Property]
	[Category( "Components" ), Order( 5 )]
	public BoxCollider Collider { get; set; }

	[Property]
	[Category( "Components" ), Order( 5 )]
	public NavMeshAgent Agent { get; set; }

	[Property]
	[Category( "Components" ), Order( 5 )]
	public CitizenAnimationHelper AnimationHelper { get; set; }

	[Property]
	[Category( "Components" ), Order( 5 )]
	public Interaction Interaction { get; set; }
	public bool IsRunning { get; set; } = false;
	public float WishSpeed => IsRunning ? RunSpeed : WalkSpeed;

	private ModelPhysics _ragdoll;
	internal Player TalkingTo;
	internal TimeUntil StopTalking;

	protected override void OnStart()
	{
		_spawnPos = WorldPosition;
		_spawnRot = WorldRotation;

		if ( RandomNameOnSpawn )
			GenerateRandomName();

		if ( Interaction.IsValid() )
			Interaction.InteractionName = FullName;
	}

	TimeUntil _nextMove = 3f;
	Vector3 _spawnPos;
	Rotation _spawnRot;
	int _lineCount = 0;

	protected override void OnFixedUpdate()
	{
		if ( ModelRenderer.IsValid() && AnimationHelper.IsValid() )
		{
			AnimationHelper.WithVelocity( Agent.Velocity );
		}

		if ( Agent.IsValid() )
		{
			Agent.MaxSpeed = WishSpeed;
			Agent.UpdateRotation = Agent.Velocity.Length >= 60f;

			if ( _nextMove )
			{
				_lineCount++;
				//WalkTo( _spawnPos + _spawnRot.Forward * 30f * _lineCount );
				_nextMove = 2f;
				//Log.Info( _spawnPos );
			}
		}

		if ( TalkingTo.IsValid() )
		{
			if ( StopTalking )
				StopTalk();
			else
				LookAt( TalkingTo );
		}
	}

	protected override void DrawGizmos()
	{
		var draw = Gizmo.Draw;

		var nameTransform = global::Transform.Zero
			.WithPosition( Vector3.Up * 84f )
			.WithRotation( Rotation.FromYaw( 90f ) * Rotation.FromRoll( 90f ) );

		draw.WorldText( FullName, nameTransform, "Poppins", 12f );
	}

	[Category( "Debug" ), Order( 6 )]
	[Button( "Ragdoll", "person_off" )]
	public void Ragdoll()
	{
		if ( !ModelRenderer.IsValid() ) return;
		if ( _ragdoll.IsValid() ) return;

		_ragdoll = AddComponent<ModelPhysics>();
		_ragdoll.Renderer = ModelRenderer;
		_ragdoll.Model = ModelRenderer.Model;

		Collider.Enabled = false;
	}

	[Category( "Debug" ), Order( 6 )]
	[Button( "Unragdoll", "person_outline" )]
	public void Unragdoll()
	{
		if ( !ModelRenderer.IsValid() ) return;
		if ( !_ragdoll.IsValid() ) return;

		_ragdoll.Destroy();
		Collider.Enabled = true;
	}

	/// <summary>
	/// Walk to a destination
	/// </summary>
	/// <param name="target"></param>
	public void WalkTo( Vector3 target )
	{
		if ( !Agent.IsValid() ) return;

		IsRunning = false;
		Agent.MoveTo( target );
	}

	/// <summary>
	/// Run to a destination
	/// </summary>
	/// <param name="target"></param>
	public void RunTo( Vector3 target )
	{
		if ( !Agent.IsValid() ) return;

		IsRunning = true;
		Agent.MoveTo( target );
	}

	public virtual void Talk( Player target )
	{
		LookAt( target );

		TalkingTo = target;
		var duration = 2f;
		StopTalking = duration;
		Interaction.InteractionCooldown = duration;
	}

	public virtual void LookAt( Player target )
	{
		AnimationHelper.LookAtEnabled = true;
		var lookStart = WorldPosition + Vector3.Up * 64f * WorldScale.z;
		var lookEnd = target.WorldPosition + Vector3.Up * 64f * target.WorldScale.z;
		var lookDirection = Vector3.Direction( lookStart, lookEnd );
		AnimationHelper.WithLook( lookDirection );
	}

	public virtual void StopTalk()
	{
		AnimationHelper.LookAtEnabled = false;
		TalkingTo = null;
		AnimationHelper.WithLook( Vector3.Zero );
	}
}
