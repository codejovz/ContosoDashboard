# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and shared document storage infrastructure

- [ ] T001 [P] Add `ContosoDashboard/Models/Document.cs` and `ContosoDashboard/Models/DocumentTag.cs`
- [ ] T002 Update `ContosoDashboard/Data/ApplicationDbContext.cs` with new `DbSet<Document>`, `DbSet<DocumentTag>`, `DbSet<DocumentShare>`, `DbSet<DocumentActivity>`, and `DbSet<TaskDocument>` properties
- [ ] T003 [P] Add `ContosoDashboard/Services/IFileStorageService.cs` and `ContosoDashboard/Services/LocalFileStorageService.cs`
- [ ] T004 Update `ContosoDashboard/Program.cs` to register `IFileStorageService` and configure secure local storage directory outside `wwwroot`
- [ ] T005 [P] Add `ContosoDashboard/Models/DocumentShare.cs`, `ContosoDashboard/Models/DocumentActivity.cs`, and `ContosoDashboard/Models/TaskDocument.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core document service, storage abstraction, and secure access support

- [ ] T006 Implement `ContosoDashboard/Services/IDocumentService.cs` to match `IDocumentService` contract
- [ ] T007 Implement `ContosoDashboard/Services/DocumentService.cs` with upload, metadata update, replace, delete, and authorization skeletons
- [ ] T008 [P] Implement secure download and preview support in `ContosoDashboard/Services/LocalFileStorageService.cs` and add helper methods for `DownloadAsync` and `GetDownloadUrlAsync`
- [ ] T009 Add application configuration support for document storage paths in `appsettings.json` and `appsettings.Development.json`
- [ ] T010 Implement audit logging support in `ContosoDashboard/Services/DocumentService.cs` using `DocumentActivity` records
- [ ] T011 [P] Add shared document retrieval and project document query methods to `DocumentService` in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T012 Add authorization checks in `ContosoDashboard/Services/DocumentService.cs` to enforce project membership, ownership, and explicit sharing

---

## Phase 3: User Story 1 - Upload and Manage Personal Documents (Priority: P1) 🎯 MVP

**Goal**: Enable a logged-in user to upload files, attach required metadata, view their personal documents, edit metadata, replace files, and delete owned documents.

**Independent Test**: Upload a supported file with title and category, view it in personal documents, edit metadata, replace the file, and delete the document.

### Implementation for User Story 1

- [ ] T013 [P] [US1] Add `ContosoDashboard/Pages/Documents.razor` to support personal document upload, metadata entry, personal document listing, and actions
- [ ] T014 [P] [US1] Add `ContosoDashboard/Shared/DocumentList.razor` component for document list display with title, category, upload date, file size, project association, and action buttons
- [ ] T015 [US1] Add `ContosoDashboard/Shared/DocumentMetadataEditor.razor` for title, description, category, and tag editing
- [ ] T016 [US1] Implement `DocumentService.GetDocumentsForUserAsync`, `UploadDocumentAsync`, `UpdateDocumentMetadataAsync`, `ReplaceDocumentFileAsync`, `DeleteDocumentAsync`, and `GetDocumentByIdAsync` in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T017 [US1] Add file type and size validation for PDF, Office, text, JPEG, and PNG in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Pages/Documents.razor`
- [ ] T018 [US1] Add delete confirmation, success/error notification flow, and owner-only edit/delete enforcement in `ContosoDashboard/Pages/Documents.razor`
- [ ] T019 [US1] Add personal document sorting by title, upload date, category, and file size in `ContosoDashboard/Pages/Documents.razor`

---

## Phase 4: User Story 2 - Project Document Sharing and Access (Priority: P2)

**Goal**: Enable project team members to upload project documents, browse project-specific documents, search/filter authorized documents, and preview/download accessible files.

**Independent Test**: Upload a project document, verify it appears in the project documents panel for authorized team members only, and confirm authorized preview/download access.

### Implementation for User Story 2

- [ ] T020 [P] [US2] Add `ContosoDashboard/Pages/SharedDocuments.razor` for shared documents visible to the current user
- [ ] T021 [US2] Update `ContosoDashboard/Pages/ProjectDetails.razor` to include a project documents section and upload support for project members
- [ ] T022 [US2] Implement `DocumentService.GetDocumentsForProjectAsync`, `DocumentService.GetSharedDocumentsForUserAsync`, and `DocumentService.SearchDocumentsAsync` in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T023 [US2] Add project document filtering and search UI in `ContosoDashboard/Pages/ProjectDetails.razor` and `ContosoDashboard/Pages/SharedDocuments.razor`
- [ ] T024 [US2] Add preview and download actions for authorized documents in `ContosoDashboard/Pages/ProjectDetails.razor` and `ContosoDashboard/Pages/SharedDocuments.razor`
- [ ] T025 [US2] Add access enforcement so only project team members or explicitly shared users can see project documents in `ContosoDashboard/Services/DocumentService.cs`

---

## Phase 5: User Story 3 - Document Notifications and Task Attachment (Priority: P3)

**Goal**: Enable document sharing with users or projects, notify recipients inside the app, and allow attaching documents to tasks.

**Independent Test**: Share a document, observe the recipient notification, attach a document to a task, and verify the task-document association.

### Implementation for User Story 3

- [ ] T026 [US3] Implement `DocumentService.ShareDocumentAsync` and `DocumentService.AttachDocumentToTaskAsync` in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T027 [US3] Add share request handling for explicit user shares and project-level shares in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T028 [US3] Add `ContosoDashboard/Pages/DocumentDetails.razor` to view document metadata, share documents, and attach documents to tasks
- [ ] T029 [US3] Update `ContosoDashboard/Pages/Tasks.razor` to support selecting and attaching an existing document to a task
- [ ] T030 [US3] Add in-app notifications for shared documents in `ContosoDashboard/Services/NotificationService.cs` or `ContosoDashboard/Services/DocumentService.cs`
- [ ] T031 [US3] Add `DocumentShare` persistence and task attachment persistence for auditability in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T032 [US3] Add access revocation behavior so deleted documents remove shared access immediately in `ContosoDashboard/Services/DocumentService.cs`

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements, secure handling, and end-to-end validation

- [ ] T033 [P] Update `specs/001-document-upload-management/quickstart.md` with validation scenarios and manual test steps
- [ ] T034 [P] Add documentation notes to `README.md` or repository feature docs about document upload and secure local storage
- [ ] T035 [P] Refine error handling, user feedback, and authorization failure messaging in `ContosoDashboard/Pages/Documents.razor`, `ContosoDashboard/Pages/ProjectDetails.razor`, and `ContosoDashboard/Pages/DocumentDetails.razor`
- [ ] T036 [P] Ensure secure local file storage is used for document uploads and downloads by validating `ContosoDashboard/Services/LocalFileStorageService.cs` and storage path configuration
- [ ] T037 [P] Add audit logging for uploads, downloads, deletes, shares, and task attachments in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T038 [P] Clean up temporary or prototype UI code and verify the document feature works on feature branch `001-document-upload-management`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1: Setup** must be complete before Phase 2 begins.
- **Phase 2: Foundational** must be complete before any user story implementation starts.
- **Phase 3+**: User stories can start after foundational infrastructure is ready.
- **Phase 6: Polish & Cross-Cutting Concerns** depends on all user stories being implemented.

### User Story Dependencies

- **User Story 1 (P1)**: Independent after Phase 2, MVP scope
- **User Story 2 (P2)**: Independent after Phase 2, integrates with shared/document retrieval services
- **User Story 3 (P3)**: Independent after Phase 2, extends sharing and task attachment

### Parallel Opportunities

- `T001`, `T003`, and `T005` can run in parallel during Phase 1 because they touch separate files and independent infrastructure concerns.
- `T006`, `T008`, `T010`, `T011`, and `T012` can be executed in parallel during Phase 2 where storage, service, and authorization work are independent.
- Phase 3 story tasks can be worked in parallel by separate team members once foundational work is complete.
- Within each story, UI component creation tasks marked `[P]` can proceed in parallel with service implementation tasks where dependencies allow.

### Parallel Example: User Story 1

- `T013 [P] [US1] Add ContosoDashboard/Pages/Documents.razor`
- `T014 [P] [US1] Add ContosoDashboard/Shared/DocumentList.razor`
- `T015 [P] [US1] Add ContosoDashboard/Shared/DocumentMetadataEditor.razor`

These can be developed in parallel while `T016 [US1]` implements the backend service methods.

## Implementation Strategy

### MVP First

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate User Story 1 independently before moving to User Story 2

### Incremental Delivery

1. Deliver User Story 1 as the MVP increment
2. Add User Story 2 after MVP validation
3. Add User Story 3 last
4. Use Phase 6 to harden the whole document feature and update docs

### Testing and Validation

- Use `specs/001-document-upload-management/quickstart.md` scenarios to validate behavior manually.
- Confirm upload validation, search, preview, sharing, and task attachment on the feature branch.
- Ensure authorization enforcement is tested for personal documents, project documents, and shared documents.

## Task Summary

- Total task count: 38
- User Story 1 tasks: 7
- User Story 2 tasks: 6
- User Story 3 tasks: 7
- Setup tasks: 5
- Foundational tasks: 7
- Polish tasks: 5
- Parallel opportunities: Setup tasks, foundational service/storage tasks, UI component creation within stories
- Suggested MVP scope: Complete Phase 1, Phase 2, and Phase 3 (User Story 1)

---
