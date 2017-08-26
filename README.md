# Latch

[![Build Status](https://travis-ci.org/denxorz/Latch.svg?branch=master)](https://travis-ci.org/denxorz/Latch) [![Coverage Status](https://coveralls.io/repos/github/denxorz/Latch/badge.svg?branch=master)](https://coveralls.io/github/denxorz/Latch?branch=master) [![NuGet](https://buildstats.info/nuget/Denxorz.Latch)](https://www.nuget.org/packages/Denxorz.Latch/) [![License](http://img.shields.io/:license-mit-blue.svg)](https://github.com/denxorz/Latch/blob/master/LICENSE)

## What does it do?
Latch makes sure that a piece of code is not executed when another piece of code is executing.

This package is based on the following articles: 

* http://www.minddriven.de/index.php/technology/development/design-patterns/latch-design-pattern
* http://codebetter.com/jeremymiller/2007/07/02/build-your-own-cab-12-rein-in-runaway-events-with-the-quot-latch-quot/

## Examples
```C#
// Remove the event handler for the moment
someComboBox.SelectedIndexChanged -= someHandler;

// do something that would probably make someComboBox fire the SelectedIndexChanged event

// Put the event handler back
someComboBox.SelectedIndexChanged += someHandler;
```

This example shows a common WinForms problem which can be solved with a latch.

```C#
// Declare the Latch as field: Latch someComboBoxLatch = new Latch();

someComboBoxLatch.RunInsideLatch(() =>
{
    // do something that would probably make someComboBox fire the SelectedIndexChanged event
});
```
Surrounding the critical code by the latch makes sure it is only executed once. Recursive calls and StackOverflows can be avoided.

```C#
// Declare the FullLatch as field: FullLatch someLatch = new FullLatch();

someLatch.LatchAndRun(() =>
{
    // do something and make sure the latch is set
});

someLatch.RunIfNotLatched(() =>
{
    // do something that should only run when not latched
});
```
This way you will have full control over the latch, and you can choose which parts latch 
and which parts are only run if not latched.

## Tools and Products Used
* [Microsoft Visual Studio Community](https://www.visualstudio.com)
* [JetBrains Resharper](https://www.jetbrains.com/resharper/)
* [NUnit](https://www.nunit.org/)
* [FakeItEasy](https://fakeiteasy.github.io/)
* [Inedo ProGet](https://inedo.com/proget)
* [Icons8](https://icons8.com/)
* [NuGet](https://www.nuget.org/)
* [GitHub](https://github.com/)