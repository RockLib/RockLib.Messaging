# RockLib.Messaging.SNS Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 4.0.0 - 2025-02-04

#### Changed
- Finalized 4.0.0 version.

## 4.0.0-alpha.1 - 2025-02-03

#### Changed
- Removed .NET 6 as a target framework.
- Updated the following packages:
  - RockLib.Messaging.4.0.1 -> RockLib.Messaging.5.0.0-alpha.1
  - AWSSDK.SimpleNotificationService.3.7.301.1 -> AWSSDK.SimpleNotificationService.3.7.400.86

## 3.1.1 - 2024-07-19

#### Changed
- RockLib.Messaging.4.0.0 -> RockLib.Messaging.4.0.1 for vulnerability fix.

## 3.1.0 - 2024-05-17

#### Changed
- Bumping version number because 3.0.0 was already published to NuGet but had been unlisted.

## 3.0.0 - 2024-05-16

#### Changed
- Add logic to retrieve `messageGroupId` and `messageDeduplicationId` from the `SenderMessage` header and assign them to `PublishMessage.MessageGroupId` and `PublishMessage.MessageDeduplicationId` respectively

## 3.0.0-alpha.2 - 2024-05-16

#### Changed
- Add logic to retrieve `messageGroupId` and `messageDeduplicationId` from the `SenderMessage` header and assign them to `PublishMessage.MessageGroupId` and `PublishMessage.MessageDeduplicationId` respectively

## 3.0.0-alpha.1 - 2024-02-28

#### Changed
- Removed netcoreapp3.1 TFM, and added net8.0.
- Updated NuGet package references to latest versions.

## 2.0.1 - 2023-02-27

#### Changed
- Updated RockLib.Messaging package reference to `3.0.1`

## 2.0.0 - 2022-03-03
	
#### Added
- Added `.editorconfig` and `Directory.Build.props` files to ensure consistency.

#### Changed
- Supported targets: net6.0, netcoreapp3.1, and net48.
- As the package now uses nullable reference types, some method parameters now specify if they can accept nullable values.
	
## 1.2.8 - 2021-08-12

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".
- Updates RockLib.Messaging to latest version, [2.5.3](https://github.com/RockLib/RockLib.Messaging/blob/main/RockLib.Messaging/CHANGELOG.md#253---2021-08-12).
- Updates AWSSDK.SimpleNotificationService to latest version, [3.7.2.22](https://github.com/aws/aws-sdk-net/blob/master/SDK.CHANGELOG.md#37950-2021-08-12-1814-utc).

## 1.2.7 - 2021-05-20

#### Added

- Adds `reloadOnChange` parameter to DI extension methods.

## 1.2.6 - 2021-05-07

#### Added

- Adds SourceLink to nuget package.

#### Changed

- Updates RockLib.Messaging package to latest version, which includes SourceLink.
- Updates AWSSDK.SimpleNotificationService package to latest version.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Messaging.SNS. What follows below are the original release notes.

----

## 1.2.5

Adds net5.0 target.

## 1.2.4

Adds icon to project and nuget package.

## 1.2.3

Updates dependency package.

## 1.2.2

Updates to align with Nuget conventions.

## 1.2.1

Updates the RockLib.Messaging and AWSSDK.SimpleNotificationService dependencies to the latest versions, 2.3.0 and 3.3.101.174 respectively.

## 1.2.0

- Updates RockLib.Messaging to 2.2.0, so that the Add(Receiver|Sender|TransactionalSender) extension methods also register a lookup delegate.
- Updates AWSSDK.SimpleNotificationService to 3.3.101.148.

## 1.1.0

Adds dependency injection extension methods and builders.

## 1.0.2

Updates RockLib.Messaging version to support RockLib_Messaging.

## 1.0.1

- Updates RockLib.Messaging package to version 2.0.1.
  - Obsoletes and hides the synchronous receiver API.
- Embeds the license file in the nuget package.
- Updates AWSSDK.SimpleNotificationService package to version 3.3.3.17.

## 1.0.0

Initial release containing `SNSSender` implementation of the `ISender` interface.

## 1.0.0-alpha02

- Renames SNSTopicSender to SNSSender.
- Updates RockLib.Messaging package to the latest prerelease version.

## 1.0.0-alpha01

Initial prerelease.
