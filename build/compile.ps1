task Default -depends Compile

task Compile {
    exec { msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /nologo $source_dir\$solution_file }
}