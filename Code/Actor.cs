using Sandbox;
using Sandbox.UI;
using System.Xml.Linq;

public abstract partial class Actor : Component
{
	[Property]
	[Category( "Info" )]
	public virtual string FirstName { get; set; } = "John";

	[Property]
	[Category( "Info" )]
	public virtual string LastName { get; set; } = "Doe";

	[Property]
	[Category( "Info" )]
	public virtual string UserName { get; set; }

	[Property]
	[Category( "Info" )]
	public string FullName => string.IsNullOrWhiteSpace( UserName ) ? $"{FirstName} {LastName}" : UserName;

	[Property]
	[Category( "Info" )]
	public Gender Gender { get; set; } = Gender.Undefined;

	[Button( "Random Name", "casino" )]
	[Category( "Info" )]
	public void GenerateRandomName()
	{
		var randomName = Actor.RandomName( Gender );
		FirstName = randomName.Item1;
		LastName = randomName.Item2;
	}

	[Property]
	[Category( "Components" )]
	public SkinnedModelRenderer ModelRenderer { get; set; }

	[Property]
	[Category( "Components" )]
	public BoxCollider Collider { get; set; }

	private ModelPhysics _ragdoll;

	protected override void OnUpdate()
	{

	}

	protected override void DrawGizmos()
	{
		var draw = Gizmo.Draw;

		var nameTransform = global::Transform.Zero
			.WithPosition( Vector3.Up * 84f )
			.WithRotation( Rotation.FromYaw( 90f ) * Rotation.FromRoll( 90f ) );

		draw.WorldText( FullName, nameTransform, "Poppins", 12f );
	}

	[Category( "Debug" )]
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

	[Category( "Debug" )]
	[Button( "Unragdoll", "person_outline" )]
	public void Unragdoll()
	{
		if ( !ModelRenderer.IsValid() ) return;
		if ( !_ragdoll.IsValid() ) return;

		_ragdoll.Destroy();
		Collider.Enabled = true;
	}
}
