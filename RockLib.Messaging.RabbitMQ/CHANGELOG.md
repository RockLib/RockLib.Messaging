# RockLib.Messaging.RabbitMQ Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
