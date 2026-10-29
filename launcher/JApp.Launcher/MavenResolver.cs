/*
 * Copyright (C) 2023 Glavo
 * Copyright (C) 2026 ghostflyby
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// Rewritten from src/main/java/org/glavo/japp/maven/{MavenResolver,MavenRepository}.java:
// unlike the Java side's private cache, artifacts live in (and are read from)
// the standard ~/.m2/repository layout, verified against the published .sha1.

namespace JApp.Launcher;

using System.Security.Cryptography;

public static class MavenResolver
{
    private const string CentralUrl = "https://repo1.maven.org/maven2";

    private static readonly string LocalRepository =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".m2", "repository");

    private static readonly HttpClient Http = new();

    public static async Task<string> ResolveAsync(
        string? repository, string group, string artifact, string version, string? classifier,
        CancellationToken cancellationToken = default)
    {
        switch (repository)
        {
            case null:
            case "central":
            case "local":
                break;
            default:
                throw new ArgumentException("Unknown repo: " + repository);
        }

        var fileName = classifier is null
            ? $"{artifact}-{version}.jar"
            : $"{artifact}-{version}-{classifier}.jar";

        var directory = Path.Combine(LocalRepository,
            group.Replace('.', Path.DirectorySeparatorChar), artifact, version);
        var file = Path.Combine(directory, fileName);

        if (File.Exists(file))
            return file;

        if (repository == "local")
            throw new IOException(fileName + " not exists or is not a regular file");

        var url = $"{CentralUrl}/{group.Replace('.', '/')}/{artifact}/{version}/{fileName}";
        var data = await Http.GetByteArrayAsync(url, cancellationToken).ConfigureAwait(false);

        var expectedHash = await TryDownloadSha1Async(url + ".sha1", cancellationToken).ConfigureAwait(false);
        if (expectedHash is not null)
        {
            var actual = SHA1.HashData(data);
            if (!actual.AsSpan().SequenceEqual(expectedHash))
                throw new IOException($"Failed while verifying {url}");
        }

        Directory.CreateDirectory(directory);
        var temp = Path.Combine(directory, Path.GetRandomFileName());
        File.WriteAllBytes(temp, data);
        File.Move(temp, file, overwrite: true);

        return file;
    }

    private static async Task<byte[]?> TryDownloadSha1Async(string url, CancellationToken cancellationToken)
    {
        try
        {
            var text = (await Http.GetStringAsync(url, cancellationToken).ConfigureAwait(false)).Trim();
            if (text.Length < 40)
                return null;

            return Convert.FromHexString(text[..40]);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
        {
            // No published checksum: proceed without verification, like the Java side.
            return null;
        }
    }
}
