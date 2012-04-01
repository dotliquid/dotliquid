properties {
    $nunit_dir = "$base_dir\src\packages\NUnit.Runners.2.6.0.12051\tools"
}

task Default -depends Test

task Test {
    exec { & $nunit_dir\nunit-console-x86.exe $source_dir/DotLiquid.Tests/$bin_folder/DotLiquid.Tests.dll /nologo /nodots /xml=$source_dir/DotLiquid.Tests/$bin_folder/DotLiquid.Tests.TestResults.xml }
}