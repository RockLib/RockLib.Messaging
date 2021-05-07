# RockLib.Messaging.Kafka Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.0.0-rc7

#### Added

- Adds SourceLink to nuget package.

#### Changed

- Updates RockLib.Messaging and RockLib.Reflection.Optimized packages to latest versions, which include SourceLink.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Messaging.Kafka. What follows below are the original release notes.

----

## 1.0.0-rc6

Updates the DI extensions to allow for automatic reloading when its options change.

## 1.0.0-rc5

Adds net5.0 target.

## 1.0.0-rc4

- Adds SynchronousProcessing setting to KafkaReceiver.
- Fixes bug in KafkaReceiver async processing.

## 1.0.0-rc3

- Allow all kafka options to be configured from the DI extension methods.
- Disallow a ConsumerConfig.EnableAutoCommit value of false in KafkaReceiver constructor #2.

## 1.0.0-rc2

Adds constructors that take full Kafka configuration models.

## 1.0.0-rc1

Initial release candidate.

## 1.0.0-alpha05

- Changes Kafka message type to `<string, byte[]>` for sender and receiver.
- Adds settings for KafkaReceiver: EnableAutoCommit, EnableAutoOffsetStore, and AutoOffsetReset.
- Adds support for Kafka message keys.
- Updates dependencies.

## 1.0.0-alpha04

Updates and expands Kafka support.

## 1.0.0-alpha03

Updates RockLib.Messaging version to support RockLib_Messaging.

## 1.0.0-alpha02

Improvements to sad path:
- `KafkaSender` throws an exception if a message isn't successfully sent.
- `KafkaReceiver` invokes its `Error`, `Disconnected`, and `Connected` events accordingly.

## 1.0.0-alpha01

Initial prerelease.
