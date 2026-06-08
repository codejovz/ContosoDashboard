# Quickstart: Document Upload and Management

## Goal

Implement document upload and management in ContosoDashboard using the existing Blazor Server application architecture and local filesystem storage.

## Steps

1. Create the document data model.
   - Add `Document`, `DocumentTag`, `DocumentShare`, `DocumentActivity`, and `TaskDocument` model classes under `ContosoDashboard/Models`.
   - Register new `DbSet<T>` properties in `ApplicationDbContext`.

2. Add storage abstraction.
   - Define `IFileStorageService` in `ContosoDashboard/Services`.
   - Implement `LocalFileStorageService` to upload, delete, and download files from a secure path outside `wwwroot`, for example `AppData/uploads`.
   - Register the implementation in `Program.cs`.

3. Add document business logic.
   - Create `IDocumentService` and `DocumentService`.
   - Encapsulate upload sequence: validate file type/size → generate secure path → save file → persist metadata → log activity.
   - Add metadata update, file replace, delete, share, search, project document retrieval, and task attachment methods.

4. Extend authorization and access control.
   - Reuse existing role and project membership checks from `ProjectService` and `TaskService`.
   - Ensure document access is allowed only for owners, project members, project managers, shared users, or shared project teams.

5. Add UI pages and components.
   - Add `Pages/Documents.razor` for personal documents.
   - Add `Pages/SharedDocuments.razor` for shared documents.
   - Add `Pages/DocumentDetails.razor` to view metadata, download, preview, edit, and share.
   - Update `Pages/ProjectDetails.razor` to include a project documents panel.
   - Update `Pages/Tasks.razor` to support attaching documents to tasks.
   - Add shared UI components such as `Shared/DocumentList.razor` and `Shared/DocumentMetadataEditor.razor`.

6. Add secure download/preview support.
   - Add a protected endpoint or service to stream files from the secure storage location.
   - Use `FileStreamResult` or equivalent to return file content with proper MIME type.
   - Avoid direct `wwwroot` file exposure.

7. Add notifications.
   - Use the existing notification service to notify recipients when documents are shared or when project documents are added.

8. Validate manually.
   - Test file uploads for supported types and size rejection.
   - Validate metadata entry, editing, document search, and sorting/filtering.
   - Confirm project documents appear only to authorized project members.
   - Verify task attachment and shared-document notification flows.

## Notes

- Ensure the feature is implemented on branch `001-document-upload-management`.
- Keep the document storage design local and offline-first to remain consistent with repository constraints.
