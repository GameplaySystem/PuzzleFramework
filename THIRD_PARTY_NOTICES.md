# Third-Party Notices

These notices apply only to the named third-party material, not to project-owned framework code.
No repository-wide open-source license is granted. The intended use is portfolio inspection;
third-party rights remain governed by their respective licenses.

## Unity Project Template

`PuzzleFramework/Assets/TutorialInfo/`, `PuzzleFramework/Assets/Readme.asset`, and the initial
Unity project scaffolding originate from Unity's Universal 3D template. The tutorial scripts,
icon, and layout match the template bundled with Unity 6000.3.17f1
(`com.unity.template.3d-cross-platform` 17.0.14). Its license notice is reproduced below:

> com.unity.template.urp-blank copyright © 2021 Unity Technologies ApS
>
> Licensed under the Unity Companion License for Unity-dependent projects--see [Unity Companion License](https://unity.com/legal/licenses/unity-companion-license).
>
> Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.

The bundled template's third-party notice file contains unfilled placeholders, not identified
additional components. They have not been represented here as actual dependencies.

## Unity Package Manager Dependencies

`PuzzleFramework/Packages/manifest.json` and `packages-lock.json` declare Unity packages, including
URP, Input System, Test Framework, IDE integrations, and other editor tooling. Their implementation
and license files are obtained by Unity Package Manager, not vendored in this repository.
Do not publish `Library/PackageCache`. Each resolved package retains its own license and notices.

## Scope

No DOTween, artist FBXs, audio, or font assets are bundled in this framework repository.
Prototype artwork and dependencies must be reviewed separately before publishing a consumer.
