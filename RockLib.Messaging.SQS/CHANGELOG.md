# RockLib.Messaging.SQS Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 3.0.2 - 2022-08-18

#### Fixed
- Updated Wildcard syntax with the correct wildcard syntax ".* " which is consistent with the aws cli.

## 3.0.2-alpha.1 - 2022-08-10

#### Fixed
- Updated Wildcard syntax with the correct wildcard syntax ".* " which is consistent with the aws cli.

## 3.0.1 - 2022-03-22

#### Fixed
- Updated AWSSDK.SQS dependency version due to recent [bug](https://github.com/aws/aws-sdk-net/issues/1992)

## 3.0.0 - 2022-03-11
	
#### Added
- Added `.editorconfig` and `Directory.Build.props` files to ensure consistency.
- Added "SQS.MessageID" as header to SQSReceiverMessages when not attempting to unpack as a SNS message.

#### Changed
- Supported targets: net6.0, netcoreapp3.1, and net48.
- As the package now uses nullable reference types, some method parameters now specify if they can accept nullable values.
- The field `QueueUrl` was updated to Uri type from string in the following places:
  - SQSReceiver
  - SQSReceiverOptions
  - SQSSender
  - SQSSenderOptions

## 2.3.0 - 2021-09-07

#### Added

- Adds ability to terminate the message visibility timeout when rolling back.

#### Changed

- Changes "Quicken Loans" to "Rocket Mortgage".
- Updates RockLib.Messaging to latest version, [2.5.3](https://github.com/RockLib/RockLib.Messaging/blob/main/RockLib.Messaging/CHANGELOG.md#253---2021-08-12).
- Updates AWSSDK.SQS to latest version, [3.7.0.52](https://github.com/aws/aws-sdk-net/blob/master/SDK.CHANGELOG.md#37950-2021-08-12-1814-utc).

## 2.2.9 - 2021-05-20

#### Added

- Adds `reloadOnChange` parameter to DI extension methods.

## 2.2.8 - 2021-05-07

#### Added

- Adds SourceLink to nuget package.

#### Changed

- Updates RockLib.Messaging package to latest version, which includes SourceLink.
- Updates AWSSDK.SQS and Newtonsoft.Json packages to latest versions.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Messaging.SQS. What follows below are the original release notes.

----

## 2.2.7

Adds net5.0 target.

## 2.2.6

In the `AddSQSSender` and `AddSQSReceiver` extension methods, attempt to resolve the `IAmazonSQS` using the `IServiceProvider` if one was not specified by the options.

## 2.2.5

Adds icon to project and nuget package.

## 2.2.4

Updates dependency package.

## 2.2.3

Updates to align with Nuget conventions.

## 2.2.2

- Maps SQS message attributes to RockLib receiver message headers, prepended with "SQS.".
  - For example, the SQS attribute "MessageGroupId" is available as the receiver message header "SQS.MessageGroupId".
- Adds an `SqsClient` property to the `SqsSenderOptions` or `SqsReceiverOptions` classes.
  - Makes it possible to specify the `IAmazonSQS` to be used by `SqsSender` and `SqsReceiver` from DI extensions.
- Updates the RockLib.Messaging and AWSSDK.SQS dependencies to their latest versions, 2.3.1 and 3.3.102.128 respectively.

## 2.2.1

Updates the RockLib.Messaging and AWSSDK.SQS dependencies to the latest versions, 2.3.0 and 3.3.102.117 respectively.

## 2.2.0

- Updates RockLib.Messaging to 2.2.0, so that the Add(Receiver|Sender|TransactionalSender) extension methods also register a lookup delegate.
- Updates AWSSDK.SQS to 3.3.102.91.

## 2.1.0

Adds dependency injection extension methods and builders.

## 2.0.4

`SQSSender` gains the ability to set the `DelaySeconds` of the SQS `SendMessageRequest` by adding a "SQS.DelaySeconds" header to the sender message.

## 2.0.3

`SQSSender` can set the `MessageGroupId` or `MessageDeduplicationId` properties on outgoing SQS message. If the sender message has a header named "SQS.MessageGroupId" or "SQS.MessageDeduplicationId", then the SQS message will have its `MessageGroupId` or `MessageDeduplicationId` property set accordingly. `MessageGroupId` can also be set by passing a value for the new `messageGroupId` constructor parameter.

## 2.0.2

Updates RockLib.Messaging version to support RockLib_Messaging.

## 2.0.1

- Updates RockLib.Messaging package to version 2.0.1.
  - Obsoletes and hides the synchronous receiver API.
- Embeds the license file in the nuget package.
- Updates AWSSDK.SQS package to version 3.3.3.53.

## 2.0.0

- Upgrades to RockLib.Messaging 2.0.
- Renames `SQSQueueSender` and `SQSQueueReceiver` to `SQSSender` and `SQSReceiver`.
- Adds optional `region` parameter to `SQSSender` and `SQSReceiver`.
- Adds optional `unpackSNS` flag to `SQSReceiver`.
  - Useful when the SQS queue receives messages from an SNS topic - the payloads of such messages contain a JSON object representing the entire raw SNS message.
  - When set, the JSON object is unpacked, and the body and headers are populated from the unpacked message.

## 2.0.0-alpha09

- Renames SQSQueueSender and SQSQueueReceiver to SQSSender and SQSReceiver.
- Updates RockLib.Messaging package to the latest prerelease version.

## 2.0.0-alpha08

- Adds optional `region` parameter to `SQSQueueSender` and `SQSQueueReceiver` constructors.
- Updates the RockLib.Messaging package to the latest prerelese, which changes the receiver API to be asynchronous.

## 2.0.0-alpha07

Updates RockLib.Messaging package to latest prerelease.

## 2.0.0-alpha06

Updates the RockLib.Messaging package to the latest prerelease, 2.0.0-alpha09.

## 2.0.0-alpha05

Updates the RockLib.Messaging package to its latest prerelease (2.0.0-alpha08).

## 2.0.0-alpha04

Updates the RockLib.Messaging package to its latest prerelease (2.0.0-alpha07).

## 2.0.0-alpha03

Updates the messaging and sqs packages to the latest versions.

## 2.0.0-alpha02

- Updates the RockLib.Messaging nuget package reference to 2.0.0-alpha03.
- Adds a `CancellationToken` parameter to the `SQSQueueSender.SendAsync` method.


## 1.0.0

Initial release.
