## YamlSerializer ##

This is a fork of the project [YamlSerializer](https://yamlserializer.codeplex.com/) on codeplex made by Osamu TAKEUCHI

This is mainly to evaluate Yaml in .NET, though this fork contains a couple of significant changes from original version:

 - Add support for Portable Class Library (.NET4.0+, Windows Phone 8, Windows App Store)
 - Add `IYamlTypeConverter` in order to support type conversion (as `TypeConverter` are no longer accessible in .NET on WP8, Windows App Store)
 - Add `IYamlSerializable` to support custom serialization
 - Add several new options in the `YamlConfig` object:
    - `EmitYamlVersion` : If true, emits the yaml version of the document. YAML 1.2 for example. Default is true.
    - `EmitStartAndEndOfDocument` : If true, emits the yaml start '---'and end of document '...'. Default is true.
    - `OrderMappingKeyByName` : Order the key in a mapping by alphabetical order to leverage on a more predictive order for comparison, versionning...etc. Default is true
    - Several `Register(...)` methods
    - `LookupAssemblies` to specify from which assemblies types can be searched.  

Almost all tests are working fine on Windows .NET4.0+ version. Not yet tested on other platforms. There is still one test not working (scalar as binary arrays for non byte[] buffers) 

Following is the original readme of YamlSerializer:
___

### Description
	A library that serialize / deserialize C# native objects into YAML1.2 text.

### Development environment 
	Visual C# 2008 Express Edition
	Sandcastle (2008-05-29)
	SandcastleBuilder 1.8.0.2
	HTML Help workshop 4.74.8702
	NUnit 2.5.0.9122
	TestDriven.NET 2.0

### Support web page 
	http://yamlserializer.codeplex.com/

### License
	YamlSerializer is distributed under the MIT license as following:

---
The MIT License (MIT)
Copyright (c) 2009 Osamu TAKEUCHI <osamu@big.jp>

Permission is hereby granted, free of charge, to any person obtaining a copy of 
this software and associated documentation files (the "Software"), to deal in the 
Software without restriction, including without limitation the rights to use, copy, 
modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, subject to the 
following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
