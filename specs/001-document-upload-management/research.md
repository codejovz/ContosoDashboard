# Research: Document Upload and Management

## Decision

Implement local secure file storage outside `wwwroot` with metadata stored in the existing SQLite-backed `ApplicationDbContext`. A storage abstraction (`IFileStorageService`) will decouple business logic from physical storage and support a future Azure Blob Storage migration without changing document-handling code.

## Rationale

- The repository is explicitly offline-first and must avoid cloud services for current implementation.
- Existing application architecture is Blazor Server + EF Core, so local filesystem storage is the simplest secure choice.
- Storing metadata separately from files keeps the database lean and enables search/filtering of document attributes.
- An abstraction interface preserves the migration path to Azure Blob Storage while keeping the current implementation self-contained.

## Key Choices

- **File storage location**: Use a secure local folder outside `wwwroot`. This avoids public exposure and enables authorization checks before file download.
- **Metadata storage**: Use an EF Core `Document` entity with text-based category values and normalized tag records rather than a comma-separated tag string.
- **Access control**: Enforce authorization in service methods with project membership, role checks, and explicit sharing records.
- **Task attachments**: Use a join entity (`TaskDocument`) to associate documents with tasks while preserving document ownership and project relationships.
- **Preview support**: Provide browser preview for PDFs and images using a secure download/preview endpoint rather than direct file links.

## Alternatives Considered

1. **Store files in `wwwroot`**
   - Rejected. It exposes files publicly and bypasses the app’s authorization model.

2. **Store files in the database as binaries**
   - Rejected. This is less efficient for large files and does not align with the offline training pattern emphasizing local file handling.

3. **Use cloud storage today**
   - Rejected. The feature must support the local-only training environment and avoid external provider dependencies.

4. **Use a simple comma-separated tag field**
   - Rejected. Normalizing tags into dedicated `DocumentTag` records supports more reliable search and avoids parsing issues.

## Assumptions

- The current application’s security model is based on ASP.NET roles and project membership information.
- There is no explicit `Team` entity in the current model, so team-level sharing will be implemented as either project-level sharing or explicit user shares.
- The current app does not yet include an automated test framework, so the first iteration will be validated manually and can add automated tests later.

## Alternatives for Future Migration

- Add `AzureBlobStorageService` implementing `IFileStorageService` and swap registration in `Program.cs`.
- Introduce full-text indexing for document metadata using SQLite FTS if search performance becomes a concern.
- Add a dedicated team management entity if document sharing requirements grow beyond project membership.
