using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle("YamlSerializer")]
[assembly: AssemblyDescription("This library serializes arbitrary .NET native objects into YAML text. It can also manipulates a generic YAML documents.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Osamu TAKEUCHI <osamu@big.jp>")]
[assembly: AssemblyProduct("YamlSerializer")]
[assembly: AssemblyCopyright("Copyright ©  2009 Osamu TAKEUCHI <osamu@big.jp>")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// アセンブリのバージョン情報は、以下の 4 つの値で構成されています:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// すべての値を指定するか、下のように '*' を使ってビルドおよびリビジョン番号を 
// 既定値にすることができます:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.9.0.2")]
[assembly: AssemblyFileVersion("0.9.0.2")]

// for test
#if DEBUG
[assembly: InternalsVisibleTo("YamlSerializerTest")]
#endif
