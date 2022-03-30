# RockLib.Messaging.CloudEvents Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

#### Added
- Added `.editorconfig` and `Directory.Build.props` files to ensure consistency.

#### Changed
- Supported targets: net6.0, netcoreapp3.1, and net48.
- As the package now uses nullable reference types, some method parameters now specify if they can accept nullable values.
- The field `url` is now a Uri type in `CloudEvent.ToHttpRequestMessage`.

## 1.0.2 - 2021-08-12

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".
- Updates RockLib.Messaging to latest version, [2.5.3](https://github.com/RockLib/RockLib.Messaging/blob/main/RockLib.Messaging/CHANGELOG.md#253---2021-08-12).

## 1.0.1 - 2021-05-07

#### Added

- Adds SourceLink to nuget package.

#### Changed

- Updates RockLib.Messaging package to latest version, which includes SourceLink.
- Updates Newtonsoft.Json package to latest version.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Messaging.CloudEvents. What follows below are the original release notes.

----

## 1.0.0

Initial Release.

## 1.0.0-rc2

Adds net5.0 target.

## 1.0.0-rc1

Initial release candidate.

## 1.0.0-alpha02

- Replaces the setters from `StringData` and `BinaryData` properties with `SetData` extension methods.
- The getters from `StringData` and `BinaryData` don't do any utf-8/base-64 encoding/decoding; they return null if the event's data doesn't match the type of the property.
- Adds generic `SetData`, `GetData` and `TryGetData` methods, each with an optional `DataSerialization` enum parameter (values: `Json` and `Xml`).
- Changes the type of the `Source`, `DataContentType`, and `DataSchema` properties to type `string`. Each of the property setters throws if the value is not a valid URI or Content-Type.

## 1.0.0-alpha01

Initial prerelease.
