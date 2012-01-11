properties {
    $nunit_dir = "$base_dir\src\packages\NUnit.2.5.10.11092\tools"
}

task Default -depends Test

task Test {
    exec { & $nunit_dir\nunit-console-x86.exe $source_dir/DotLiquid.Tests/$bin_folder/DotLiquid.Tests.dll /nologo /nodots /xml=$source_dir/DotLiquid.Tests/$bin_folder/DotLiquid.Tests.TestResults.xml }
}