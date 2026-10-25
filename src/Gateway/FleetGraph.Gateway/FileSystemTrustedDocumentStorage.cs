using System.Text.RegularExpressions;
using HotChocolate.Execution;
using HotChocolate.Language;
using HotChocolate.PersistedOperations;

namespace FleetGraph.Gateway;

/// <summary>
/// Read-only trusted document store: <c>trusted-documents/{id}.graphql</c>.
/// The id is the SHA-256 (hex) of the document, produced by the client build. In production
/// the store would be a registry (Nitro, a blob container, Redis) fed by the client's CI.
/// </summary>
internal sealed partial class FileSystemTrustedDocumentStorage(string directory) : IOperationDocumentStorage
{
    public async ValueTask<IOperationDocument?> TryReadAsync(
        OperationDocumentId documentId,
        CancellationToken cancellationToken = default)
    {
        // Ids come from the client: never let them escape the directory.
        if (!SafeId().IsMatch(documentId.Value))
        {
            return null;
        }

        var path = Path.Combine(directory, documentId.Value + ".graphql");

        if (!File.Exists(path))
        {
            return null;
        }

        var text = await File.ReadAllTextAsync(path, cancellationToken);
        return new OperationDocument(Utf8GraphQLParser.Parse(text));
    }

    public ValueTask SaveAsync(
        OperationDocumentId documentId,
        IOperationDocument document,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException(
            "Trusted documents are registered by the client build, not at runtime.");

    [GeneratedRegex("^[a-zA-Z0-9_-]{1,128}$")]
    private static partial Regex SafeId();
}
