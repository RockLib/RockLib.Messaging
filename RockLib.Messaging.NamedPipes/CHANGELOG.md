# RockLib.Messaging.NamedPipes Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 4.0.0 - 2024-03-04

#### Changed
- Finalized 4.0.0 version.

## 4.0.0-alpha.1 - 2024-02-28

#### Changed
- Removed netcoreapp3.1 TFM, and added net8.0.
- Updated NuGet package references to latest versions.

## 3.0.1 - 2023-02-27

#### Changed
- Updated RockLib.Messaging package reference to `3.0.1`

## 3.0.0 - 2022-03-08

#### Added
- Added `.editorconfig` and `Directory.Build.props` files to ensure consistency.

#### Changed
- Supported targets: net6.0, netcoreapp3.1, and net48.
- As the package now uses nullable reference types, some method parameters now specify if they can accept nullable values.

## 2.2.8 - 2021-08-12

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".
- Updates RockLib.Messaging to latest version, [2.5.3](https://github.com/RockLib/RockLib.Messaging/blob/main/RockLib.Messaging/CHANGELOG.md#253---2021-08-12).

## 2.2.7 - 2021-05-20

#### Added

- Adds `reloadOnChange` parameter to DI extension methods.

## 2.2.6 - 2021-05-07

#### Added

- Adds SourceLink to nuget package.

#### Changed

- Updates RockLib.Messaging package to latest version, which includes SourceLink.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Messaging.NamedPipes. What follows below are the original release notes.

----

## 2.2.5

Adds net5.0 target.

## 2.2.4

Adds icon to project and nuget package.

## 2.2.3

Updates dependency package.

## 2.2.2

Updates to align with Nuget conventions.

## 2.2.1

Updates the RockLib.Messaging dependency to the latest version, 2.3.0.

## 2.2.0

Updates RockLib.Messaging to 2.2.0, so that the Add(Receiver|Sender|TransactionalSender) extension methods also register a lookup delegate.

## 2.1.0

Adds dependency injection extension methods and builders.

## 2.0.2

Updates RockLib.Messaging version to support RockLib_Messaging.

## 2.0.1

- Updates RockLib.Messaging package to version 2.0.1.
  - Obsoletes and hides the synchronous receiver API.
- Embeds the license file in the nuget package.

## 2.0.0

- Moves the named pipes feature out of the main package and into this package.
- Upgrades to RockLib.Messaging 2.0.
- Fixes a bug in the named pipe sender and receiver where it would cause an app to hang on shutdown if not disposed.

## 2.0.0-alpha09

Updates RockLib.Messaging package to the latest prerelease version.

## 2.0.0-alpha08

Updates the RockLib.Messaging package to the latest prerelese, which changes the receiver API to be asynchronous.

## 2.0.0-alpha07

Updates RockLib.Messaging package to latest prerelease.

## 2.0.0-alpha06

Updates the RockLib.Messaging package to the latest prerelease, 2.0.0-alpha09.

## 2.0.0-alpha05

Updates the RockLib.Messaging package to its latest prerelease (2.0.0-alpha08).

## 2.0.0-alpha04

Updates the RockLib.Messaging package to its latest prerelease (2.0.0-alpha07).

## 2.0.0-alpha03

Updates the messaging package to the latest version.

## 2.0.0-alpha02

- Updates the RockLib.Messaging nuget package reference to 2.0.0-alpha03.
- Adds a `CancellationToken` parameter to the `NamedPipeSender.SendAsync` method.
