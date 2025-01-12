using Sandbox;

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

	protected override void OnUpdate()
	{

	}
}
