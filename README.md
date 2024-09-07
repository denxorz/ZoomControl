# ZoomControl

[![Build Status](https://github.com/denxorz/ZoomControl/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/denxorz/ZoomControl/actions/workflows/dotnet-desktop.yml) [![Coverage Status](https://coveralls.io/repos/github/denxorz/ZoomControl/badge.svg?branch=master)](https://coveralls.io/github/denxorzZoomControl?branch=master) ![NuGet](https://img.shields.io/nuget/dt/Denxorz.ZoomControl) [![License](http://img.shields.io/:license-mspl-blue.svg)](https://github.com/denxorz/ZoomControl/blob/master/LICENSE)

## What does it do?
ZoomControl is used as a WPF Viewbox with zoom abilities.

This package is based on the following projects: 

* https://wpfextensions.codeplex.com
* https://github.com/andypelzer/GraphSharp/blob/master/Graph%23.Controls

## Examples
```C#
   <Grid>
        <denxorz:ZoomControl>
            <Grid
                Width="800"
                Height="700"
                Background="Beige">
                <Canvas>
                    <Rectangle
                        Canvas.Left="102"
                        Canvas.Top="450"
                        Width="100"
                        Height="200"
                        Fill="Aqua" />
                </Canvas>
            </Grid>
        </denxorz:ZoomControl>
   </Grid>
```

A sample project can be found here: https://github.com/denxorz/ZoomControl/tree/master/Sample.

## Tools and Products Used
* [Microsoft Visual Studio Community](https://www.visualstudio.com)
* [JetBrains Resharper](https://www.jetbrains.com/resharper/)
* [Inedo ProGet](https://inedo.com/proget)
* [Icons8](https://icons8.com/)
* [NuGet](https://www.nuget.org/)
* [GitHub](https://github.com/)

## Thanks to 
* [MauNguyenVan](https://github.com/MauNguyenVan) for contributing

## Versions & Release Notes
* version 1.2: Add Framework 4.8 and NET8 support
* version 1.1: Downgrade .NET Framework version from 4.6.1 to 4.5.2
* version 1.0: First version
