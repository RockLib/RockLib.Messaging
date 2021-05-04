# RockLib.Messaging.Http Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

#### Added

- Adds SourceLink to nuget package.

----

**Note:** Release notes in the above format are not available for earlier versions of
RockLib.Messaging.Http. What follows below are the original release notes.

----

## 1.0.6

Adds net5.0 target.

## 1.0.5

Adds icon to project and nuget package.

## 1.0.4

Updates dependency package.

## 1.0.3

Updates to align with Nuget conventions.

## 1.0.2

Updates RockLib.Messaging version to support RockLib_Messaging.

## 1.0.1

- Updates RockLib.Messaging package to version 2.0.1.
  - Obsoletes and hides the synchronous receiver API.
- Embeds the license file in the nuget package.

## 1.0.0

Initial release containing `HttpClientSender` and `HttpListenerReceiver` implementations of the `ISender` and `IReceiver` interfaces.

## 1.0.0-alpha09

Updates RockLib.Messaging package to the latest prerelease version.

## 1.0.0-alpha08

Updates the RockLib.Messaging package to the latest prerelese, which changes the receiver API to be asynchronous.

## 1.0.0-alpha07

- Extracts a class for required headers of the HttpListenerReceiver constructors.
- Updates RockLib.Messaging package to latest prerelease.

## 1.0.0-alpha06

- Fixes header mapping in HttpClientSender.
- Adds accept parameter to HttpListenerReceiver - when set, any request that does not match this receives a 406 Not Acceptable response.

## 1.0.0-alpha05

Fixes a bug in path regex of http receiver, where it would handle a request with extra path elements instead of returning a 404. For example, if the receiver's path is "/api/{api_version}" and an incoming request had a path of "/api/v1/extra_stuff", the response should be a 404.

## 1.0.0-alpha04

Updates the RockLib.Messaging package to the latest prerelease, 2.0.0-alpha09.

## 1.0.0-alpha03

- HttpClientSender splits message headers by ',' and ';' infer http headers with multiple values.
- Adds a defaultHeaders constructor parameter to HttpClientSender.
- Adds support for Content-Type in HttpClientSender and HttpListenerReceiver.
  - HttpClientSender sets the outgoing request's content type from the current message's headers or from its own default headers.
  - When set in HttpListenerReceiver, messages that don't have that content type receive a 415 response.
- Simplifies HttpListenerReceiver constructors by omitting status description parameters.

## 1.0.0-alpha02

Adds easier-to-use constructor overloads to HttpListenerReceiver.

## 1.0.0-alpha01

Initial prerelease.
