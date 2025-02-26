$framework = '4.0'

Properties {
    $root_dir = Split-Path $psake.build_script_file	
    $build_artifacts_dir = "$root_dir\build\"
    $package_dir = "$root_dir\package"
    $code_dir = "source"
    $solution = "dbdeploy.net.sln"
    #$api_key = Get-Content "api.key"
    $version = "1.2.1"
    $configuration = "Debug"
    $scripts_dir = "$root_dir\scripts"
}

Task Default -depends Build

Task Build {
    Exec { msbuild $solution }
}

Task Clean {
    Exec { msbuild "$solution" /t:Clean /p:Configuration=$configuration /v:quiet "/p:OutDir=$build_artifacts_dir\" }
    
    if (Test-Path $build_artifacts_dir){
        Remove-Item $build_artifacts_dir -recurse
    }
    if (Test-Path $package_dir){
        Remove-Item $package_dir -recurse
    }
}

Task BuildPackage -depends Clean {
    if (-not (Test-Path $build_artifacts_dir)){
        mkdir $build_artifacts_dir
    }
        
    if (-not (Test-Path $package_dir)){
        mkdir $package_dir
    }
    
    Write-Host "Building" -ForegroundColor Green
    Exec { msbuild "$solution" /t:Build /p:Configuration=$configuration /v:quiet "/p:OutDir=$build_artifacts_dir" }

    mkdir $build_artifacts_dir\console
    mkdir $build_artifacts_dir\powershell
    mkdir $build_artifacts_dir\nant
    mkdir $build_artifacts_dir\msbuild
    mkdir $build_artifacts_dir\scripts
    
    Copy-Item -Path $build_artifacts_dir\dbproviders.xml,$build_artifacts_dir\dbdeploy.exe,$build_artifacts_dir\dbdeploy.exe.config,$build_artifacts_dir\Net.Sf.Dbdeploy.dll -Destination $build_artifacts_dir\console
    Copy-Item -Path $build_artifacts_dir\dbproviders.xml,$build_artifacts_dir\dbdeploy.Powershell.dll,$build_artifacts_dir\dbdeploy.Powershell.dll.config,$build_artifacts_dir\Net.Sf.Dbdeploy.dll -Destination $build_artifacts_dir\powershell
    Copy-Item -Path $build_artifacts_dir\dbproviders.xml,$build_artifacts_dir\dbdeploy.NAnt.dll,$build_artifacts_dir\Net.Sf.Dbdeploy.dll,$build_artifacts_dir\NAnt.Core.dll -Destination $build_artifacts_dir\nant
    Copy-Item -Path $build_artifacts_dir\dbproviders.xml,$build_artifacts_dir\msbuild.dbdeploy.task.dll,$build_artifacts_dir\Net.Sf.Dbdeploy.dll -Destination $build_artifacts_dir\msbuild
    
    Copy-Item -Path $scripts_dir\*.sql -Destination $build_artifacts_dir\scripts
    
    Get-ChildItem build -Exclude console,msbuild,nant,powershell,scripts  |Remove-Item
    
    Write-Host "Creating packages" -ForegroundColor Green
    Get-ChildItem $build_artifacts_dir\ -recurse | Write-Zip -IncludeEmptyDirectories -EntryPathRoot "build" -OutputPath $package_dir\dbdeploy.net-$version.zip

    Write-Host "Package created at $package_dir\dbdeploy.net-$version.zip" -ForegroundColor Green
}


