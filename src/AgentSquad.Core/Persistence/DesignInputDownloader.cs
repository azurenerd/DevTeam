using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using AgentSquad.Core.Configuration;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentSquad.Core.Persistence;

/// <summary>
/// Downloads external design input documents from HTTP(S) URLs and extracts their text content.
/// Supports .docx (OpenXML), .pdf (basic text extraction), .html (strip tags), .md/.txt (as-is).
/// Caches downloaded content in the agent workspace to avoid re-downloading on restart.
/// </summary>
public class DesignInputDownloader
{
    private readonly HttpClient _httpClient;
    private readonly AgentSquadConfig _config;
    private readonly ILogger<DesignInputDownloader> _logger;
    private readonly string? _cacheDir;

    private static readonly Regex UrlPattern = new(
        @"https?://[^\s""'<>\)]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HtmlTagPattern = new(
        @"<[^>]+>",
        RegexOptions.Compiled);

    private static readonly Regex MultiWhitespacePattern = new(
        @"[ \t]{2,}",
        RegexOptions.Compiled);

    public DesignInputDownloader(
        HttpClient httpClient,
        IOptions<AgentSquadConfig> config,
        ILogger<DesignInputDownloader> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Cache dir in workspace root
        if (!string.IsNullOrWhiteSpace(_config.Workspace.RootPath))
        {
            _cacheDir = Path.Combine(_config.Workspace.RootPath, ".design-input-cache");
        }
    }

    /// <summary>
    /// Extracts HTTP(S) URLs from the given text (project description or other config fields).
    /// </summary>
    public static IReadOnlyList<string> ExtractUrls(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<string>();
        return UrlPattern.Matches(text).Select(m => m.Value.TrimEnd('.', ',', ';', ')')).Distinct().ToList();
    }

    /// <summary>
    /// Downloads and extracts text from all configured design input URLs plus any URLs
    /// detected in the project description. Returns the combined text content.
    /// </summary>
    public async Task<string?> DownloadDesignInputsAsync(CancellationToken ct = default)
    {
        var urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Add explicitly configured URLs
        foreach (var url in _config.Project.DesignInputUrls)
        {
            if (!string.IsNullOrWhiteSpace(url))
                urls.Add(url.Trim());
        }

        // Extract URLs from project description
        foreach (var url in ExtractUrls(_config.Project.Description))
        {
            urls.Add(url);
        }

        if (urls.Count == 0) return null;

        var sb = new StringBuilder();
        var successCount = 0;

        foreach (var url in urls)
        {
            try
            {
                var content = await DownloadAndExtractAsync(url, ct);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var fileName = GetFileNameFromUrl(url);
                    sb.AppendLine($"### External Design Input: `{fileName}`");
                    sb.AppendLine($"**Source:** {url}");
                    sb.AppendLine();
                    // Truncate very large documents
                    var truncated = content.Length > 15000
                        ? content[..15000] + "\n\n<!-- truncated at 15000 chars -->"
                        : content;
                    sb.AppendLine(truncated);
                    sb.AppendLine();
                    successCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download design input from {Url}", url);
            }
        }

        if (successCount > 0)
        {
            _logger.LogInformation(
                "Downloaded {Count} external design input(s) from {Total} URL(s)",
                successCount, urls.Count);
            return sb.ToString().TrimEnd();
        }

        return null;
    }

    /// <summary>
    /// Downloads a single URL and extracts its text content based on content type / file extension.
    /// Uses local cache if available.
    /// </summary>
    private async Task<string?> DownloadAndExtractAsync(string url, CancellationToken ct)
    {
        // Check cache first
        var cached = await ReadFromCacheAsync(url);
        if (cached is not null)
        {
            _logger.LogDebug("Using cached design input for {Url}", url);
            return cached;
        }

        _logger.LogInformation("Downloading design input from {Url}", url);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Apply auth headers if configured
        var authHeader = GetAuthHeaderForUrl(url);
        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authHeader);
        }

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
        var extension = GetExtensionFromUrl(url);

        string? extractedText;

        if (IsDocx(contentType, extension))
        {
            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            extractedText = ExtractTextFromDocx(bytes);
        }
        else if (IsPdf(contentType, extension))
        {
            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            extractedText = ExtractTextFromPdf(bytes);
        }
        else if (IsHtml(contentType, extension))
        {
            var html = await response.Content.ReadAsStringAsync(ct);
            extractedText = ExtractTextFromHtml(html);
        }
        else
        {
            // Treat as plain text (markdown, txt, etc.)
            extractedText = await response.Content.ReadAsStringAsync(ct);
        }

        // Cache the extracted text
        if (!string.IsNullOrWhiteSpace(extractedText))
        {
            await WriteToCacheAsync(url, extractedText);
        }

        return extractedText;
    }

    /// <summary>
    /// Extracts plain text from a .docx file using OpenXML SDK.
    /// </summary>
    internal static string? ExtractTextFromDocx(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null) return null;

        var sb = new StringBuilder();
        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            var text = paragraph.InnerText;
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text);
            }
            else
            {
                // Preserve paragraph breaks
                sb.AppendLine();
            }
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Basic text extraction from PDF. Extracts text streams from page content.
    /// This is a lightweight approach that handles simple text PDFs.
    /// For complex PDFs, the raw bytes are returned as a note.
    /// </summary>
    internal static string? ExtractTextFromPdf(byte[] bytes)
    {
        // Simple PDF text extraction: look for text between BT/ET operators
        // and parenthesized strings. This handles basic text PDFs.
        try
        {
            var content = Encoding.Latin1.GetString(bytes);
            var sb = new StringBuilder();

            // Extract text from Tj and TJ operators (parenthesized strings)
            var textPattern = new Regex(@"\(([^)]*)\)", RegexOptions.Compiled);
            var inTextBlock = false;

            foreach (var line in content.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed == "BT") { inTextBlock = true; continue; }
                if (trimmed == "ET") { inTextBlock = false; sb.AppendLine(); continue; }

                if (inTextBlock)
                {
                    foreach (Match match in textPattern.Matches(line))
                    {
                        var text = UnescapePdfString(match.Groups[1].Value);
                        if (!string.IsNullOrWhiteSpace(text))
                            sb.Append(text);
                    }
                }
            }

            var result = sb.ToString().Trim();
            if (result.Length > 50) return result;

            // Fallback: try to find readable ASCII text sequences
            return ExtractReadableTextFromBinary(bytes);
        }
        catch
        {
            return ExtractReadableTextFromBinary(bytes);
        }
    }

    /// <summary>
    /// Strips HTML tags and extracts readable text content.
    /// </summary>
    internal static string ExtractTextFromHtml(string html)
    {
        // Remove script and style blocks
        var cleaned = Regex.Replace(html, @"<(script|style)[^>]*>.*?</\1>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Replace block-level tags with newlines
        cleaned = Regex.Replace(cleaned, @"<(br|p|div|h[1-6]|li|tr|blockquote)[^>]*>", "\n", RegexOptions.IgnoreCase);

        // Remove all remaining tags
        cleaned = HtmlTagPattern.Replace(cleaned, "");

        // Decode common HTML entities
        cleaned = cleaned
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&apos;", "'")
            .Replace("&#39;", "'")
            .Replace("&nbsp;", " ");

        // Collapse whitespace
        cleaned = MultiWhitespacePattern.Replace(cleaned, " ");

        // Collapse multiple blank lines
        cleaned = Regex.Replace(cleaned, @"\n{3,}", "\n\n");

        return cleaned.Trim();
    }

    private string? GetAuthHeaderForUrl(string url)
    {
        foreach (var (prefix, header) in _config.Project.DesignInputAuthHeaders)
        {
            if (url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return header;
        }
        return null;
    }

    private async Task<string?> ReadFromCacheAsync(string url)
    {
        if (_cacheDir is null) return null;

        var cacheFile = GetCacheFilePath(url);
        if (!File.Exists(cacheFile)) return null;

        // Invalidate cache after 24 hours
        var fileInfo = new FileInfo(cacheFile);
        if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc > TimeSpan.FromHours(24))
        {
            try { File.Delete(cacheFile); } catch { /* best effort */ }
            return null;
        }

        return await File.ReadAllTextAsync(cacheFile);
    }

    private async Task WriteToCacheAsync(string url, string content)
    {
        if (_cacheDir is null) return;

        try
        {
            Directory.CreateDirectory(_cacheDir);
            var cacheFile = GetCacheFilePath(url);
            await File.WriteAllTextAsync(cacheFile, content);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to cache design input for {Url}", url);
        }
    }

    private string GetCacheFilePath(string url)
    {
        // Use a hash of the URL as the filename to avoid path issues
        var hash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(url)))[..16];
        return Path.Combine(_cacheDir!, $"{hash}.txt");
    }

    private static string GetFileNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var fileName = Path.GetFileName(path);
            return string.IsNullOrWhiteSpace(fileName) ? uri.Host : fileName;
        }
        catch
        {
            return url.Length > 60 ? url[..60] + "..." : url;
        }
    }

    private static string GetExtensionFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
        }
        catch
        {
            return "";
        }
    }

    private static bool IsDocx(string contentType, string extension) =>
        extension == ".docx" ||
        contentType.Contains("officedocument.wordprocessingml", StringComparison.OrdinalIgnoreCase) ||
        contentType.Contains("vnd.openxmlformats", StringComparison.OrdinalIgnoreCase);

    private static bool IsPdf(string contentType, string extension) =>
        extension == ".pdf" ||
        contentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase);

    private static bool IsHtml(string contentType, string extension) =>
        extension is ".html" or ".htm" ||
        contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase);

    private static string UnescapePdfString(string s)
    {
        return s
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t")
            .Replace("\\(", "(")
            .Replace("\\)", ")")
            .Replace("\\\\", "\\");
    }

    /// <summary>
    /// Extracts readable ASCII text sequences from binary content (fallback for complex PDFs).
    /// </summary>
    private static string? ExtractReadableTextFromBinary(byte[] bytes)
    {
        var sb = new StringBuilder();
        var currentWord = new StringBuilder();

        foreach (var b in bytes)
        {
            if (b is >= 32 and <= 126 or (byte)'\n' or (byte)'\r' or (byte)'\t')
            {
                currentWord.Append((char)b);
            }
            else
            {
                if (currentWord.Length > 3)
                {
                    sb.Append(currentWord);
                    sb.Append(' ');
                }
                currentWord.Clear();
            }
        }

        if (currentWord.Length > 3)
            sb.Append(currentWord);

        var result = sb.ToString().Trim();
        // Only return if we got meaningful text (not just PDF structure)
        return result.Length > 100 ? result : "[PDF content could not be extracted as text]";
    }
}
