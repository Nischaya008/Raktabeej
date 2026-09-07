using System.Reflection;

namespace RaktabeejCore.Tests;

public class ArchitectureTests
{
	// Guards decision D-06: RaktabeejCore stays a pure .NET library so the rules
	// engine remains testable in seconds without booting Godot.
	[Fact]
	public void CoreAssemblyDoesNotReferenceGodot()
	{
		// Reads the assembly reference table, so it catches a Godot type actually
		// being used in core. Roslyn drops references nothing consumes, so an
		// unused Godot reference in the csproj would not surface here.
		var godotReferences = Assembly.Load("RaktabeejCore")
			.GetReferencedAssemblies()
			.Select(reference => reference.Name)
			.Where(name => name is not null
				&& name.Contains("Godot", StringComparison.OrdinalIgnoreCase))
			.ToArray();

		Assert.True(
			godotReferences.Length == 0,
			$"RaktabeejCore must not reference Godot. Found: {string.Join(", ", godotReferences)}");
	}
}
