# Data Model: Document Upload and Management

## Documents

**Document**: Represents an uploaded file and all associated metadata.

- `DocumentId` (int, PK)
- `Title` (string, required, max 255)
- `Description` (string, optional, max 2000)
- `Category` (string, required, max 100)
- `ProjectId` (int?, FK to `Project`) - optional association for project documents
- `UploaderId` (int, FK to `User`) - person who uploaded the document
- `UploadDate` (DateTime, required)
- `OriginalFileName` (string, required, max 255)
- `StoredFilePath` (string, required, max 500) - secure path outside `wwwroot`
- `FileSize` (long, required)
- `FileType` (string, required, max 255)
- `IsPreviewable` (bool, derived or stored) - common preview types such as PDF or image

Relationships:
- `Document` has many `DocumentTag`
- `Document` has many `DocumentShare`
- `Document` has many `DocumentActivity`
- `Document` has many `TaskDocument`

## Tags

**DocumentTag**: Normalized tags for search and filtering.

- `DocumentTagId` (int, PK)
- `DocumentId` (int, FK)
- `Tag` (string, required, max 100)

Indexes:
- Index on `DocumentId`
- Index on `Tag`

## Task Attachments

**TaskDocument**: Associates documents with a task.

- `TaskDocumentId` (int, PK)
- `TaskId` (int, FK to `TaskItem`)
- `DocumentId` (int, FK to `Document`)
- `AttachedDate` (DateTime, required)

Relationships:
- `TaskDocument` belongs to `TaskItem`
- `TaskDocument` belongs to `Document`

## Sharing

**DocumentShare**: Records explicit share relationships.

- `DocumentShareId` (int, PK)
- `DocumentId` (int, FK)
- `SharedWithUserId` (int?, FK to `User`) - explicit user share
- `SharedWithProjectId` (int?, FK to `Project`) - project-level share representing team access
- `SharedByUserId` (int, FK to `User`)
- `SharedDate` (DateTime, required)
- `Role` (string?, optional, max 100) - optional label for team shares or role-based context

Notes:
- If the repository does not support a dedicated `Team` entity, project-level share records can act as team shares.
- Explicit user shares remain first-class.

## Audit Logging

**DocumentActivity**: Tracks document lifecycle events.

- `DocumentActivityId` (int, PK)
- `DocumentId` (int, FK)
- `UserId` (int, FK to `User`)
- `ActivityType` (enum: Upload, Download, Delete, Share, Replace, MetadataUpdate)
- `ActivityDate` (DateTime, required)
- `Details` (string?, max 1000)

Indexes:
- Index on `DocumentId`
- Index on `(UserId, ActivityDate)`

## Additional Model Notes

- Document categories are stored as descriptive text to satisfy the requirement that category values be text-based.
- `DocumentId` is an integer key to align with existing User, Project, and Task entities.
- `StoredFilePath` must be generated using a secure pattern such as `{uploaderId}/{projectId or personal}/{guid}{extension}`.
- `IsPreviewable` can be derived from `FileType` for quick UI decisions, but `FileType` remains the authoritative field.
