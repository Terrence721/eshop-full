# build/

The upstream `build/` folder contained two PowerShell scripts:

- `build/acr-build/queue-all.ps1` — queued Azure Container Registry build tasks for a list of `Dockerfile`s across `src/Services/*` and `src/Web/*`.
- `build/multiarch-manifests/create-manifests.ps1` — created/pushed multi-arch (`linux`/`win`) docker manifests for those images.

Neither applies to this fork:

1. There are **no Dockerfiles** in this repository. Containerization is handled by .NET Aspire's `eShop.AppHost` (Docker images are produced via Aspire's container orchestration / publish pipeline, not hand-maintained per-service Dockerfiles), so an ACR build-task queue has nothing to build.
2. The manifest script's image list and tag scheme (`linux-master`/`win-master`/etc.) belong to the upstream Aspire-era layout and the Windows/Linux matrix that this fork does not ship.

Accordingly `build/` is intentionally left empty rather than copying dead scripts that would only create drift. If container publishing scripts are needed later, they should be written against Aspire's publish outputs, not inherited from upstream.
