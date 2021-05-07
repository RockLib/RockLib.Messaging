# RockLib.Messaging.SNS Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.2.6

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
