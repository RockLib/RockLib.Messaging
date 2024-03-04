# RockLib.Messaging.Kafka Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 3.0.0 - 2024-03-04

#### Changed
- Finalized 3.0.0 version.

## 3.0.0-alpha.1 - 2024-02-28

#### Changed
- Removed netcoreapp3.1 TFM, and added net8.0.
- Updated NuGet package references to latest versions.

## 2.0.0-alpha1 - 2023-05-03

#### Changed
- Updated Confluent.Kafka package reference to `2.1.0`
- Updated Newtonsoft.Json package reference to '13.0.3'

#### Upgrade Considerations [from Confluent.Kafka](https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/CHANGELOG.md#upgrade-considerations)
- OpenSSL 3.0.x upgrade in librdkafka requires a major version bump, as some legacy ciphers need to be explicitly configured to continue working, but it is highly recommended NOT to use them. The rest of the API remains backward compatible.

## 1.0.3 - 2023-02-27

#### Changed
- Updated RockLib.Messaging package reference to `3.0.1`

## 1.0.2 - 2022-12-06

#### Fixed
- Fixed memory access violations that occur when the cancellation token is disposed and still being accessed by the Kafka consumer.
- Removed tracking collection nullability since it is initialized during construction and always clean up collection.
- Updated cancellation token to pass to add method.
- Updated to handle own operation cancelled exception in tracking task.

## 1.0.1 - 2022-10-13

#### Fixed
- Out of memory exceptions - in cases where a system has tons of messages to process from a topic regardless of being able to scale, the system can run into out of memory exceptions due to the consuming task not having any backpressure to prevent it from dumping whole partition(s) (and possibly topic) into the tracking collection.
- Duplicate message processing - similarly, when a system has tons of messages to process and the number of consumers is less than the number of partitions, upon scaling up the new consumer starts processing the partition it was assigned, but messages it consumes are still in memory of the originally assigned consumer, causing the messages to be processed multiple times.

## 1.0.0 - 2022-03-14

#### Added
- Added `.editorconfig` and `Directory.Build.props` files to ensure consistency.

#### Changed
- Supported targets: net6.0, netcoreapp3.1, and net48.
- As the package now uses nullable reference types, some method parameters now specify if they can accept nullable values.
- `KafkaReceiver` no longer allows synchronous processing to occur, so the `synchronousProcessing` parameters in the constructors have been removed. This also means the `SynchronousProcessing` property on `KafkaReceiverOptions` has been removed.

## 1.0.0-rc12 - 2021-11-15

#### Added

- Adds StatisticsEmitted event to KafkaRecevier and KafkaSender to support output of Kafka statistics.

## 1.0.0-rc11 - 2021-10-27

#### Added

- Adds payload schemaId detection to support schema validation.

## 1.0.0-rc10 - 2021-10-07

#### Added

- Adds constructor to KafkaSender class that accommodates specifying the SchemaId as well as Kafka producer config.

## 1.0.0-rc9 - 2021-09-17

#### Added

- Adds SchemaId property to KafkaSender class. Setting it directs the broker to validate messages against the specified schema.

## 1.0.0-rc8 - 2021-08-12

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".
- Updates RockLib.Messaging to latest version, [2.5.3](https://github.com/RockLib/RockLib.Messaging/blob/main/RockLib.Messaging/CHANGELOG.md#253---2021-08-12).
- Updates RockLib.Reflection.Optimized to latest version, [1.3.2](https://github.com/RockLib/RockLib.Reflection.Optimized/blob/main/RockLib.Reflection.Optimized/CHANGELOG.md#132---2021-08-11).
- Updates Confluent.Kafka to latest version, [1.7.0](https://github.com/confluentinc/confluent-kafka-dotnet/releases/tag/v1.7.0).

## 1.0.0-rc7 - 2021-05-07

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
