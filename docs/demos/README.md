# Demo Artifacts — Index

DoD condition #7 requires a 10–30 s clip per feature. **The clips do not live in this
repository** (decision **D-22**). They live at:

    ~/dev/raktabeej-assets/demos/<feature-id>.mp4

alongside the other source assets excluded by D-12, and are backed up with them.

This file is the index. A feature is not Done until its row is here, added in the same commit
as the rest of the feature.

## Why the clips are not committed

- The repository is **public** (D-17). Git LFS bandwidth is consumed whenever the objects are
  fetched, including by third parties cloning the repo, against a 1 GB monthly allowance
  (D-12). That is not controllable on a public repo.
- Roughly 139 features at a few megabytes each would put a substantial binary store in git
  history, which cannot be pruned without a history rewrite the repository integrity rules
  forbid.
- **It is the only reversible choice.** Plain blobs and LFS objects are both awkward to remove
  from history after the fact. Starting outside the repo means adopting LFS later costs nothing
  and cleans up nothing.

The tradeoff accepted: there is no browsable in-repo gallery, and devlog material is published
to a platform rather than served from GitHub.

## Index

| Feature | Clip | What it shows | Recorded |
|---|---|---|---|
| _none yet_ | — | M0-ENV work is not filmable. M0-DBG-01 is the first feature with a demo artifact | — |
