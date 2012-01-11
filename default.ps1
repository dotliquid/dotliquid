# Thanks to AutoMapper for the inspiration for this build file.

$global:config = 'Release'

properties {
    $base_dir = resolve-path .
    $source_dir = "$base_dir\src"
    $build_dir = "$base_dir\build"
    $package_dir = "$base_dir\package"
}

task Default -depends Clean, Compile, Test, Package

task Clean {
    delete_directory "$package_dir"
}

task Compile -depends Clean {
    Invoke-Psake "$build_dir\compile.ps1" -framework 3.5 -parameters @{"solution_file"="DotLiquid-2008.sln"}
    Invoke-Psake "$build_dir\compile.ps1" -framework 4.0 -parameters @{"solution_file"="DotLiquid.sln"}
}

task Test -depends Compile {
    Invoke-Psake "$build_dir\test.ps1" -framework 3.5 -parameters @{"bin_folder"="bin/$config-3.5"}
    Invoke-Psake "$build_dir\test.ps1" -framework 4.0 -parameters @{"bin_folder"="bin/$config"}
}

task Package {
    Invoke-Psake "$build_dir\package.ps1" -framework 4.0
}

# Helper functions

function global:delete_directory($directory_name) {
    rd $directory_name -recurse -force -ErrorAction SilentlyContinue | out-null
}