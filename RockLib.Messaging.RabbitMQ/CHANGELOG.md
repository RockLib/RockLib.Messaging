# RockLib.Messaging.RabbitMQ Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.0.1 - 2023-02-27

#### Changed
- Updated RockLib.Messaging package reference to `3.0.1`

## 1.0.0 - 2022-03-07

#### Added
- Added `.editorconfig` and `Directory.Build.props` files to ensure consistency.

#### Changed
- Supported targets: net6.0, netcoreapp3.1, and net48.
- As the package now uses nullable reference types, some method parameters now specify if they can accept nullable values.

## 1.0.0-alpha10 - 2021-08-12

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".
- Updates RockLib.Messaging to latest version, [2.5.3](https://github.com/RockLib/RockLib.Messaging/blob/main/RockLib.Messaging/CHANGELOG.md#253---2021-08-12).
- Updates RabbitMQ.Client to latest version, [6.2.2](https://github.com/rabbitmq/rabbitmq-dotnet-client/releases/tag/v6.2.2).

## 1.0.0-alpha09 - 2021-05-07

#### Added

- Adds SourceLink to nuget package.

#### Changed

- Updates RockLib.Messaging package to latest version, which includes SourceLink.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Messaging.RabbitMQ. What follows below are the original release notes.

----

## 1.0.0-alpha08

- Adds net5.0 target.
- Updates RabbitMQ.Client package to latest version.

## 1.0.0-alpha07

Adds icon to project and nuget package.

## 1.0.0-alpha06

Updates dependency package.

## 1.0.0-alpha05

Updates to align with Nuget conventions.

## 1.0.0-alpha04

Updates RockLib.Messaging version to support RockLib_Messaging.

## 1.0.0-alpha03

- Fixes bug in RabbitReceiverMessage: when a received message had no headers, a null reference exception was thrown.
- Updates RockLib.Messaging to the latest version, 2.0.3.

## 1.0.0-alpha02

- Renames QueueName to Queue in RabbitReceiver.
- Fixes null reference exception in RabbitSender when RoutingKeyHeaderName is not provided.

## 1.0.0-alpha01

Initial prerelease.
