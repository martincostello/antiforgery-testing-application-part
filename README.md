# ASP.NET Core Application Part For Testing Anti-forgery Protected Resources

[![Build status](https://github.com/martincostello/antiforgery-testing-application-part/workflows/build/badge.svg?branch=main&event=push)](https://github.com/martincostello/antiforgery-testing-application-part/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush)

## Introduction

An example application that demonstrates using [ASP.NET Core application parts](https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts "Share controllers, views, Razor Pages and more with Application Parts") for easier integration testing of HTTP resources that are protected by the [anti-forgery](https://docs.microsoft.com/en-us/aspnet/core/security/anti-request-forgery "") features of ASP.NET Core, such as `[ValidateAntiforgeryToken]`.

## Feedback

Any feedback or issues can be added to the issues for this project in [GitHub](https://github.com/martincostello/antiforgery-testing-application-part/issues "Issues for this project on GitHub.com").

## Repository

The repository is hosted in [GitHub](https://github.com/martincostello/antiforgery-testing-application-part "This project on GitHub.com"): https://github.com/martincostello/antiforgery-testing-application-part.git

## License

This project is licensed under the [Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license") license.

## Building and Testing

Compiling the application yourself requires Git and the [.NET Core SDK](https://www.microsoft.com/net/download/core "Download the .NET Core SDK") to be installed (version `3.1.201` or later).

To build and test the application locally from a terminal/command-line, run the following set of commands:

```powershell
git clone https://github.com/martincostello/antiforgery-testing-application-part.git
cd antiforgery-testing-application-part
./build.ps1
```
