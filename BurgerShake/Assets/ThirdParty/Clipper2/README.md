# Clipper2 dependency for ingredient collider authoring

Upstream: https://github.com/AngusJohnson/Clipper2
Release: Clipper2_2.0.1
Pinned commit: 21ebba05db8894f0c7217ad35ea518080f324946
Source directory: CSharp/Clipper2Lib

The nine C# source files are vendored from the pinned commit. Only UTF-8 BOM removal and line-ending normalization were applied. Upstream targets .NET Standard 2.0 / C# 8. No network access or package installation is required in Unity.

PolygonColliderScaler uses Clipper.InflatePaths with a negative distance, round joins, five decimal places of precision and 0.002 collider-local arc tolerance. This replaces a miter-only offset that repeatedly halved the global inset to preserve narrow features. The requested inset is now applied directly; stems may disappear and narrow connections may split. If the entire shape disappears, the tool leaves the saved collider untouched and reports a warning.

Supported input: non-overlapping solid loops. Input holes/nested loops remain unsupported. No automatic regeneration or runtime collider changes were introduced.

Licensing:
- Clipper2: Boost Software License 1.0, in LICENSE.txt and upstream source headers.
- HashCode.cs additionally carries .NET Foundation MIT and Yann Collet BSD notices; its source notice is retained and the .NET license is included in LICENSE.dotnet.txt.

Verification: same-release native Clipper2 tests covered full 0.53 insets on narrow-stem shapes and the committed Apple/Watermelon outlines, reversed winding, separate islands, splitting, zero inset and total erosion. Native tests validate the geometry algorithm; they do not replace C# compilation or Unity editor tests.
