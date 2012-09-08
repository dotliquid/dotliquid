properties {
    $nuget_dir = "$base_dir\src\packages\NuGet.CommandLine.1.6.0\tools"
}

task Default -depends Package

task Package {
    create_directory "$package_dir"
     
    # Copy NuSpec template files to package dir
    cp "$build_dir\DotLiquid.nuspec" "$package_dir"
    cp "$build_dir\LICENSE.txt" "$package_dir"

    # Copy binary files to package dir
    copy_files "$source_dir\DotLiquid\bin\$config-3.5" "$package_dir\lib\NET35" "*.dll","*.pdb"
    copy_files "$source_dir\DotLiquid\bin\$config" "$package_dir\lib\NET40" "*.dll","*.pdb"

    # Copy source files to package dir
    copy_files "$source_dir\DotLiquid" "$package_dir\src\DotLiquid" "*.cs"

    # Version number is hard-coded so that we can add -beta suffix if necessary
    $version = "1.7.0"

    # Build the NuGet package
    exec { & $nuget_dir\NuGet.exe pack -Symbols -Version "$version" -OutputDirectory "$package_dir" "$package_dir\DotLiquid.nuspec" }

    # Push NuGet package to nuget.org
    exec { & $nuget_dir\NuGet.exe push "$package_dir\DotLiquid.$version.nupkg" }
}

# Helper functions

function global:create_directory($directory_name)
{
    mkdir $directory_name -ErrorAction SilentlyContinue | out-null
}

function global:copy_files($source, $destination, $include = @(), $exclude = @()) {
    create_directory $destination
    
    $items = Get-ChildItem $source -Recurse -Include $include -Exclude $exclude
    foreach ($item in $items) {
        $dir = $item.DirectoryName.Replace($source,$destination)
        $target = $item.FullName.Replace($source,$destination)

        if (!(test-path($dir))) {
            create_directory $dir
        }
        
        if (!(test-path($target))) {
            cp -path $item.FullName -destination $target
        }
    }
}