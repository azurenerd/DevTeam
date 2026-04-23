using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AgentSquad.Dashboard.Tests.Helpers;

/// <summary>
/// Converts WebM video files to optimized animated GIFs using FFmpeg two-pass pipeline.
/// Pass 1: Generate optimal color palette from video frames.
/// Pass 2: Encode GIF using the palette for high quality at small file size.
/// </summary>
public static class GifConverter
{
    private static readonly string FfmpegPath = FindFfmpeg();

    /// <summary>
    /// Convert a WebM video to an animated GIF.
    /// </summary>
    /// <param name="webmPath">Path to the source .webm file.</param>
    /// <param name="gifPath">Path for the output .gif file.</param>
    /// <param name="fps">Frames per second (lower = smaller file). Default 4.</param>
    /// <param name="maxWidth">Max width in pixels. Default 1280.</param>
    /// <param name="trimStartSeconds">Desired seconds to skip from start. Auto-capped to safe range.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if conversion succeeded.</returns>
    public static async Task<bool> ConvertAsync(
        string webmPath,
        string gifPath,
        int fps = 4,
        int maxWidth = 1280,
        double trimStartSeconds = 0,
        CancellationToken ct = default)
    {
        if (!File.Exists(webmPath))
            return false;

        // Probe video duration to calculate safe trim
        var duration = await ProbeDurationAsync(webmPath, ct);
        var safeTrim = CalculateSafeTrim(trimStartSeconds, duration);

        var palettePath = Path.Combine(
            Path.GetDirectoryName(gifPath)!,
            $"palette-{Guid.NewGuid():N}.png");

        try
        {
            var filters = $"fps={fps},scale={maxWidth}:-1:flags=lanczos";
            // Use -ss BEFORE -i for fast seek (keyframe-based, reliable with filter chains)
            var seekArg = safeTrim > 0.1 ? $"-ss {safeTrim:F2} " : "";

            // Pass 1: Generate palette
            var pass1 = await RunFfmpegAsync(
                $"{seekArg}-i \"{webmPath}\" -vf \"{filters},palettegen=stats_mode=diff\" -y \"{palettePath}\"",
                ct);
            if (!pass1)
                return false;

            // Pass 2: Encode GIF with palette
            var pass2 = await RunFfmpegAsync(
                $"{seekArg}-i \"{webmPath}\" -i \"{palettePath}\" -filter_complex \"{filters}[x];[x][1:v]paletteuse=dither=bayer:bayer_scale=5\" -y \"{gifPath}\"",
                ct);
            return pass2;
        }
        finally
        {
            try { File.Delete(palettePath); } catch { }
        }
    }

    public static bool IsAvailable => File.Exists(FfmpegPath);

    /// <summary>Calculate a safe trim that won't over-trim short videos.</summary>
    private static double CalculateSafeTrim(double requested, double? videoDuration)
    {
        if (requested <= 0.1) return 0;
        if (videoDuration is null || videoDuration <= 0) return 0;

        // Don't trim videos shorter than 4 seconds
        if (videoDuration < 4.0) return 0;

        // Cap trim to 30% of video duration or requested, whichever is smaller
        var maxTrim = videoDuration.Value * 0.3;
        return Math.Min(requested, Math.Min(maxTrim, 3.0)); // Also hard cap at 3s
    }

    /// <summary>Probe video duration using FFmpeg stderr output.</summary>
    private static async Task<double?> ProbeDurationAsync(string path, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = FfmpegPath,
            Arguments = $"-i \"{path}\" -f null -",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = psi };
        process.Start();
        var stderr = await process.StandardError.ReadToEndAsync(ct);
        try { await process.WaitForExitAsync(ct); } catch { }

        // Parse "Duration: HH:MM:SS.xx" from stderr
        var match = Regex.Match(stderr, @"Duration:\s*(\d+):(\d+):(\d+)\.(\d+)");
        if (!match.Success) return null;

        var hours = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var minutes = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        var seconds = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
        var fraction = double.Parse($"0.{match.Groups[4].Value}", CultureInfo.InvariantCulture);
        return hours * 3600 + minutes * 60 + seconds + fraction;
    }

    private static async Task<bool> RunFfmpegAsync(string args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = FfmpegPath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        // Drain stderr (FFmpeg writes progress there)
        var stderrTask = process.StandardError.ReadToEndAsync(ct);

        try
        {
            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            throw;
        }

        await stderrTask; // just drain it
        return process.ExitCode == 0;
    }

    private static string FindFfmpeg()
    {
        // Check common locations
        var candidates = new[]
        {
            @"C:\Tools\ffmpeg\bin\ffmpeg.exe",
            @"C:\ProgramData\chocolatey\bin\ffmpeg.exe",
        };

        foreach (var c in candidates)
            if (File.Exists(c))
                return c;

        // Fall back to PATH
        return "ffmpeg";
    }
}
