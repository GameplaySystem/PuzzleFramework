# Framework Validation Setup

The installable package lives at the repository root under
`Packages/com.gaming.puzzleframework`. The Unity project is the nested `PuzzleFramework/` folder.
Its committed manifest does **not** currently install the sibling framework package or enable
that package's tests. Opening that project alone will not discover the framework tests.

For an isolated local validation host:

1. Use Unity 6000.3.17f1, the version recorded in the nested project's ProjectVersion.txt.
2. Use a disposable Unity project with Unity Test Framework installed. Do not change a shared
   active checkout's project settings while another task is using it.
3. In Package Manager, install the package from disk by choosing this checkout's
   `Packages/com.gaming.puzzleframework/package.json`.
4. In that disposable host's `Packages/manifest.json`, add a top-level
   `"testables": ["com.gaming.puzzleframework"]` property while preserving its dependencies.
5. Allow compilation, then open **Window > General > Test Runner > EditMode**.

Unity documents package test discovery in [Add tests to your package](https://docs.unity3d.com/6000.3/Documentation/Manual/cus-tests.html).
This task has documented the missing setup, not created or verified a new test host.

Keep local `file:` dependencies out of committed consumer manifests. Normal consumers install
the full-SHA Git dependency according to the [dependency workflow](Workflow/FrameworkPackageDependencyWorkflow.md).
The [watchlist](IMPLEMENTATION_WATCHLIST.md) records an existing catalog-test assertion failure;
historical focused test passes must not be described as a clean full-suite result.
