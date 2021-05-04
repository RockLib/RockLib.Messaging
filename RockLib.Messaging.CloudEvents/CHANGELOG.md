# RockLib.Messaging.CloudEvents Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

#### Added

- Adds SourceLink to nuget package.

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
