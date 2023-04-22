using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CliWrap.Buffered;
using CliWrap.Tests.Utils;
using FluentAssertions;
using Xunit;

namespace CliWrap.Tests;

public class PathResolutionSpecs
{
    [Fact(Timeout = 15000)]
    public async Task I_can_execute_a_command_on_an_executable_using_its_short_name()
    {
        // Arrange
        var cmd = Cli.Wrap("dotnet")
            .WithArguments("--version");

        // Act
        var result = await cmd.ExecuteBufferedAsync();

        // Assert
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Trim().Should().MatchRegex(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$");
    }

    [SkippableFact(Timeout = 15000)]
    public async Task I_can_execute_a_command_on_a_script_using_its_short_name()
    {
        Skip.IfNot(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            "Path resolution for scripts is only required on Windows."
        );

        // Arrange
        using var dir = TempDir.Create();
        await File.WriteAllTextAsync(
            Path.Combine(dir.Path, "test-script.cmd"),
            "@echo hello"
        );

        using (TempEnvironmentVariable.ExtendPath(dir.Path))
        {
            var cmd = Cli.Wrap("test-script");

            // Act
            var result = await cmd.ExecuteBufferedAsync();

            // Assert
            result.ExitCode.Should().Be(0);
            result.StandardOutput.Trim().Should().Be("hello");
        }
    }
}
